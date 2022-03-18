using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace _7DTDWebsockets.Connections
{
    internal class WebsocketConnection : WebSocketBehavior
    {
        public static WebsocketConnection WebSocketInstance;
        public WebsocketConnection()
        {
            WebSocketInstance = this;
        }

        public void SendBroadcast(string msg)
        {
            Sessions.Broadcast(msg);
        }
    }
}
