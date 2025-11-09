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
        public string Id { get; } = Guid.NewGuid().ToString();
        public string UserName { get; }
        public TcpClient Client { get; }
        public NetworkStream Stream => Client.GetStream();
        public IPEndPoint RemoteEndPoint => (IPEndPoint)Client.Client.RemoteEndPoint;
        public User(TcpClient _client, string _userName)
        {
            UserName = _userName ?? throw new ArgumentException(nameof(_userName));
            Client = _client ?? throw new ArgumentException(nameof(_client));
        }

        public async Task SendAsync(string message)
        {
            try
            {
                byte[] byteMessage = Encoding.UTF8.GetBytes(message);
                await Stream.WriteAsync(byteMessage, 0, byteMessage.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to {UserName}: {ex.Message}");
            }
        }
    }
}