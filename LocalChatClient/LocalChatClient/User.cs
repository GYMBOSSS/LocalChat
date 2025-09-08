using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace LocalChatClient
{
    internal class User
    {
        TcpClient client;
        string userName;

        public User(TcpClient client, string userName)
        {
            this.client = client;
            this.userName = userName;
        }

        public TcpClient getClient() { return client; }
        public string getUserName() { return userName; }
    }
}