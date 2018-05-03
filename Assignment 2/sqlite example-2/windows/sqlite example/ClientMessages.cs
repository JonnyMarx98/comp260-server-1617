using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;

namespace MUDServer
{
    class ClientMessageBase
    {
        public ClientMessageBase(Socket client)
        {
            this.client = client;
        }
        public Socket client;
    }

    class ClientJoined : ClientMessageBase
    {
        public ClientJoined(Socket client) : base(client)
        {

        }
    }

    class ClientLost : ClientMessageBase
    {
        public ClientLost(Socket client) : base(client)
        {

        }
    }

    class ClientMessage : ClientMessageBase
    {
        public ClientMessage(Socket client, String message) : base(client)
        {
            this.message = message;
        }
        public String message;
    }
}
