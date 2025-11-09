using LocalChatClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalChat.Services.Interfaces
{
    internal interface IServer : IDisposable
    {
        Task StartAsync();
        Task StopAsync();
        Task BroadcastAsync(string message, User? excludeUser = null);
        Task SendToUserAsync(string message, string userId);
        IReadOnlyList<User> ConnectedUsers { get; }
        event EventHandler<User> UserConnected;
        event EventHandler<User> UserDisconnected;
    }
}
