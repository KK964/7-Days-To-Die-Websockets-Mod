using WebSocketSharp.Server;

//original work done by KK
//removed uncessary using statements -MM

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
