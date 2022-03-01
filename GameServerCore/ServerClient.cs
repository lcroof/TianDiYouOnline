using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore
{
    public class ServerClient
    {
        public WebSocket socket;
        public Guid clientId;

        public ServerClient(WebSocket socket, Guid clientId)
        {
            this.socket = socket;
            this.clientId = clientId;
        }
    }
}
