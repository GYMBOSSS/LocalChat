using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace LocalChatClient.Models
{
    internal class User
    {
        TcpClient client {  get; set; }
        string userName { get; set; }
        public User(TcpClient client, string userName)
        {
            this.client = client;
            this.userName = userName;
        }
    }
}