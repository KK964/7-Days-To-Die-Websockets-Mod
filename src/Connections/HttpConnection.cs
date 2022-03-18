using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net;
using System.IO;

using WebSocketSharp;
using WebSocketSharp.Server;

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

            if (!string.IsNullOrEmpty(authentication) && !req.Headers.Contains("Authentication"))
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

        private void Server_OnPost(object sender, HttpRequestEventArgs e)
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

        }

    }
}
