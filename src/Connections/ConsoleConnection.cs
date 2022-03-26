using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace _7DTDWebsockets.Connections
{
    internal class ConsoleConnection : ConsoleConnectionAbstract
    {
        public List<string> lines;

        public ConsoleConnection() => lines = new List<string>();

        public override string GetDescription() => "Websocket Mod Console";

        public override void SendLine(string _text) => lines.Add(_text);

        public override void SendLines(List<string> _output)
        {
            foreach (string line in _output)
            {
                SendLine(line);
            }
        }

        public override void SendLog(string _formattedMessage, string _plainMessage, string _trace, LogType _type, DateTime _timestamp, long _uptime)
        {
            if (!IsLogLevelEnabled(_type)) return;
            SendLine(_formattedMessage);
        }
    }
}
