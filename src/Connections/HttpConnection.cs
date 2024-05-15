using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Collections.Specialized;

using WebSocketSharp;
using WebSocketSharp.Server;
using _7DTDWebsockets.Extensions;

namespace _7DTDWebsockets.Connections
{
    internal class HttpConnection
    {
        public HttpServer server { get; private set; }
        private string authentication;

        public HttpConnection(int port, string auth)
        {
            if (!string.IsNullOrEmpty(auth))
                authentication = GetHash(auth);
            server = new HttpServer(port);
            server.OnGet += Server_OnGet;
            server.OnPost += Server_OnPost;
        }

        private string GetHash(string raw)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        private bool IsAuthenticated(HttpRequestEventArgs e)
        {
            var req = e.Request;

            if (!string.IsNullOrEmpty(authentication) && !req.Headers.ContainsKey("Authentication"))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(authentication))
            {
                string auth = req.Headers["Authentication"];
                if (auth.StartsWith("Bearer ")) auth = auth.Substring("Bearer ".Length);
                if (authentication != GetHash(auth))
                    return false;
            }
            return true;
        }

        private void Server_OnGet(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;

            if (!IsAuthenticated(e))
            {
                res.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            string path = req.RawUrl;
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/api"))
            {
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            //TODO: Add paths for getting game data

            res.Close();
        }

        private async void Server_OnPost(object sender, HttpRequestEventArgs e)
        {
            var req = e.Request;
            var res = e.Response;

            if (!IsAuthenticated(e))
            {
                res.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            string path = req.RawUrl;
            if (string.IsNullOrEmpty(path) || !path.StartsWith("/api"))
            {
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            path = path.Substring("/api".Length);

            string content = "";
            if (req.HasEntityBody)
            {
                Stream stream = req.InputStream;
                Encoding encoding = req.ContentEncoding;
                StreamReader reader = new StreamReader(stream, encoding);
                content = reader.ReadToEnd();
                stream.Close();
                reader.Close();
            }

            //TODO: Add dynamic paths to methods

            if (path == "/command")
            {
                byte[] responseBytes;
                res.ContentType = "text/plain";
                res.ContentEncoding = Encoding.UTF8;
                List<string> cmdResponse = RunCommand(content);
                responseBytes = Encoding.UTF8.GetBytes(string.Join("\n", cmdResponse));
                res.ContentLength64 = responseBytes.LongLength;
                res.Close(responseBytes, true);
            }
        }

        private List<string> RunCommand(string command)
        {
            SdtdConsole sdtd = SingletonMonoBehaviour<SdtdConsole>.Instance;
            ConsoleConnection console = new ConsoleConnection();
            sdtd.ExecuteAsync(command, console);
            FieldInfo queueField = typeof(SdtdConsole).GetField("m_commandsToExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            try
            {
                bool running = true;
                while (running)
                {
                    bool hasCommand = false;
                    object cmdQueue = queueField.GetValue(sdtd);
                    foreach (object cmd in (cmdQueue as IEnumerable))
                    {
                        Type type = cmd.GetType();
                        string com = (string)cmd.GetType().GetField("command", BindingFlags.Public | BindingFlags.Instance).GetValue(cmd);
                        if (com == command) hasCommand = true;
                    }
                    running = hasCommand;
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return console.lines;
        }
    }
}
