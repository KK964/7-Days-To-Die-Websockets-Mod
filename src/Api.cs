using System.Xml;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using _7DTDWebsockets.patchs;
using _7DTDWebsockets.Connections;

//original work done by KK
//removed some unecessary using statements and slight change to authentication method by Mustached_Maniac

namespace _7DTDWebsockets
{
    public class API : IModApi
    {
        public static API Instance { get; private set; }
        private static HttpConnection Http;

        public void InitMod(Mod mod)
        {
            Instance = this;
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

            int port = 9000;
            string auth = "";
            if (data.ContainsKey("Port")) port = int.Parse(((data["Port"] ?? "9000")).ToString());
            if (data.ContainsKey("Authentication")) auth = (data["Authentication"] ?? string.Empty).ToString();

            RunTimePatch.PatchAll();
            StartConnection(port, auth);
            ModEvents.GameShutdown.RegisterHandler(GameShutdown);
            //ModEvents.ChatMessage.RegisterHandler(ChatMessage); chat messages handled differently, temporarily disabling this to make work in V1.0
            ModEvents.PlayerLogin.RegisterHandler(PlayerLogin);
            ModEvents.PlayerDisconnected.RegisterHandler(PlayerDisconnect);
            ModEvents.PlayerSpawnedInWorld.RegisterHandler(PlayerSpawnedInWorld);
        }

        private void StartConnection(int port, string auth)
        {
            Log.Out($"[Websocket] Starting api on port: {port}");
            new Thread(() =>
            {
                Http = new HttpConnection(port, auth);
                Http.server.AddWebSocketService<WebsocketConnection>("/");
                Http.server.Start();
            }).Start();
        }

        private void StopConnection()
        {
            if (Http != null) Http.server.Stop();
        }

        public static void Send(string eventName, string arguments)
        {
            Send(eventName + " " + arguments);
        }

        public static void Send(string message)
        {
            if (Http == null || WebsocketConnection.WebSocketInstance == null) return;
            WebsocketConnection.WebSocketInstance.SendBroadcast(message);
        }

        private void GameShutdown()
        {
            StopConnection();
        }

        /*
        Temporarily disabling the chat message since it's handled differently now and caused the mod not to work
        
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
        */
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
    }
}
