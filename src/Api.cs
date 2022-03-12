using Platform.Steam;
using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

using _7DTDWebsockets.patchs;

namespace _7DTDWebsockets
{
    public class API : IModApi
    {
        public void InitMod(Mod mod)
        {
            string path = ModManager.GetMod("WebsocketIntegration").Path + "/Config.xml";
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.IgnoreWhitespace = true;
            settings.IgnoreComments = true;
            XmlReader reader = XmlReader.Create(path, settings);

            Dictionary<string, object> data = new Dictionary<string, object>();

            Log.Out($"[Websocket] Attempting read of \"{path}\"");

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    if (reader.IsEmptyElement) return;
                    string n;
                    string s;
                    n = reader.Name;
                    reader.Read();
                    if (reader.IsStartElement()) n = reader.Name;
                    s = reader.ReadString();
                    data.Add(n, s);
                }
            }

            foreach (var i in data)
            {
                Log.Out($"[Websocket] Config: {i.Key} : {i.Value}");
            }

            string host = (data["Host"] ?? "localhost").ToString();
            string port = (data["Port"] ?? "9000").ToString();

            RunTimePatch.PatchAll();
            Websocket.Start(host + ":" + port);
            ModEvents.GameShutdown.RegisterHandler(GameShutdown);
            ModEvents.ChatMessage.RegisterHandler(ChatMessage);
            ModEvents.PlayerLogin.RegisterHandler(PlayerLogin);
            ModEvents.PlayerDisconnected.RegisterHandler(PlayerDisconnect);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
        }

        private void GameShutdown()
        {
            Websocket.Stop();
        }


        private class ChatMsg
        {
            public Player player;
            public string Message;
            public ChatMsg(Player pl, string msg)
            {
                player = pl;
                Message = msg;
            }
        }
        private bool ChatMessage(ClientInfo clientInfo, EChatType chatType, int senderId, string message, string mainName, bool localizeMain, List<int> recipientEntityIds)
        {
            if (clientInfo == null || string.IsNullOrEmpty(message)) return true;
            ChatMsg m = new ChatMsg(new Player(clientInfo), message);
            Send("ChatMessage", JsonConvert.SerializeObject(m));
            return true;
        }

        private class PlayerOnlyObj
        {
            public Player player;
            public PlayerOnlyObj(Player pl)
            {
                player = pl;
            }
        }

        private bool PlayerLogin(ClientInfo clientInfo, string noIdea, StringBuilder stringBuilder)
        {
            if (clientInfo == null) return true;
            Send("PlayerJoin", JsonConvert.SerializeObject(new PlayerOnlyObj(new Player(clientInfo))));
            return true;
        }

        private void PlayerDisconnect(ClientInfo clientInfo, bool idk)
        {
            if (clientInfo == null) return;
            Send("PlayerLeave", JsonConvert.SerializeObject(new PlayerOnlyObj(new Player(clientInfo))));
        }

        class PlayerSpawnIn
        {
            public Player player;
            public string type;
            public PlayerSpawnIn(Player player, string type)
            {
                this.player = player;
                this.type = type;
            }
        }

        private void PlayerSpawnedInWorld(ClientInfo clientInfo, RespawnType respawnType, Vector3i vector3I)
        {
            if (clientInfo == null) return;
            Send("PlayerSpawnIn", JsonConvert.SerializeObject(new PlayerSpawnIn(new Player(clientInfo), respawnType.ToString())));
        }

        public class Player
        {
            public string name;

            public Player (ClientInfo clientInfo)
            {
                this.name = clientInfo.playerName ?? "Unknwon";
            }

            public Player (EntityPlayer player)
            {
                this.name = player.EntityName;
            }
        }

        public static void Send(string eventName, string arguments)
        {
            Websocket.Send(eventName + " " + arguments);
        }

        public class Websocket
        {
            public static WebSocketServer server;
            public static EventSocket eventSocket;

            public static void Start(string host)
            {
                Log.Out($"[Websocket] Starting socket server on: ws://{host}/");
                server = new WebSocketServer("ws://" + host);
                server.AddWebSocketService<EventSocket>("/");
                server.Start();
            }

            public static void Stop()
            {
                if (server != null) server.Stop();
            }

            public static void Send(string message)
            {
                if (server == null || eventSocket == null) return;
                eventSocket.SendBroadcast(message);
            }

            public class EventSocket : WebSocketBehavior
            {
                public void SendBroadcast(string msg)
                {
                    Sessions.Broadcast(msg);
                }

                protected override void OnOpen()
                {
                    if (eventSocket == null) eventSocket = this;
                }

                protected override void OnError(ErrorEventArgs e)
                {
                    if (eventSocket == null) eventSocket = this;
                }

                protected override void OnClose(CloseEventArgs e)
                {
                    if (eventSocket == null) eventSocket = this;
                }

                protected override void OnMessage(MessageEventArgs e)
                {
                    Sessions.Broadcast(e.Data);
                }
            }
        }
    }
}