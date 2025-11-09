using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using LocalChatClient;
using LocalChat.Services.Interfaces;

namespace AleksandrP.Services
{
    internal class Server : IServer
    {
        private readonly TcpListener _listener;
        private readonly List<User> _connectedUsers;
        private readonly object _usersLock = new object();
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _isRunning;

        public IReadOnlyList<User> ConnectedUsers
        {
            get
            {
                lock (_usersLock) 
                {
                    return _connectedUsers.ToList();
                }
            }
        }

        public event EventHandler<User>? UserConnected;
        public event EventHandler<User>? UserDisconnected;  
        public Server(IPEndPoint endPoint)
        {
            _listener = new TcpListener(endPoint);
            _connectedUsers = new List<User>();
        }

        public async Task StartAsync()
        {
            if (_isRunning) return;

            _cancellationTokenSource = new CancellationTokenSource();
            _isRunning = true;

            try
            {
                _listener.Start();
                Console.WriteLine($"Server is started on {_listener.LocalEndpoint}");

                _ = Task.Run(() => AcceptClientsAsync(_cancellationTokenSource.Token));
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error in server starting: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            if (!_isRunning) return;

            _cancellationTokenSource?.Cancel();
            _isRunning = false;

            lock (_usersLock) 
            {
                foreach (var user in _connectedUsers)
                { 
                    user.Client.Close();
                }
                _connectedUsers.Clear();
            }

            _listener.Stop();
            Console.WriteLine("Server stopped");
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) 
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    _ = Task.Run(() => HandleClientAsync(tcpClient, cancellationToken), cancellationToken);
                }
                catch (ObjectDisposedException ex) 
                {
                    break;
                }
                catch(Exception ex) 
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            User? user = null;
            try
            {
                var userName = await ReceiveMessageAsync(tcpClient.GetStream(), cancellationToken);
                if (string.IsNullOrWhiteSpace(userName))
                {
                    tcpClient.Close();
                    return;
                }

                user = new User(tcpClient, userName.Trim());

                lock (_usersLock)
                {
                    _connectedUsers.Add(user);
                }

                UserConnected?.Invoke(this, user);
                Console.WriteLine($"User connected: {user.UserName} - {user.RemoteEndPoint}");

                await BroadcastUserListAsync();

                await ProcessClientMessagesAsync(user, cancellationToken);
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is IOException)
            { }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client: {ex.Message}");
            }
            finally 
            {
                if (user != null)
                {
                    lock (_usersLock) 
                    {
                        _connectedUsers.Remove(user);
                    }

                    UserDisconnected?.Invoke(this, user);
                    Console.WriteLine($"User disconnected: {user.UserName}");

                    await BroadcastUserListAsync();
                }
            }
        }
        private async Task ProcessClientMessagesAsync(User user, CancellationToken cancellationToken)
        {
            var stream = user.Stream;

            while(!cancellationToken.IsCancellationRequested && user.Client.Connected)
            {
                try
                {
                    while (!stream.DataAvailable)
                    {
                        await Task.Delay(100, cancellationToken);
                        continue;
                    }

                    var message = await ReceiveMessageAsync(stream, cancellationToken);

                    if (string.IsNullOrEmpty(message)) break;

                    await ProcessClientCommandAsync(user, message, cancellationToken);
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is IOException)
                {
                    break;
                }
                catch (Exception ex) 
                {
                    Console.WriteLine($"Error processing message from {user.UserName}: {ex.Message}");
                    break;
                }
            } 
        }
        private async Task ProcessClientCommandAsync(User user, string command, CancellationToken cancellationToken)
        {
            switch (command.ToUpper())
            {
                case "GETUSERS":
                    await SendUserListAsync(user);
                    break;

                case "WRITE":
                    await HandlePrivateChatAsync(user, cancellationToken);
                    break;

                default:
                    // Broadcast regular message
                    await BroadcastAsync($"{user.UserName}: {command}", user);
                    break;
            }
        }
        private async Task HandlePrivateChatAsync(User fromUser, CancellationToken cancellationToken)
        {
            try
            {
                await SendUserListAsync(fromUser);

                var targetIndexStr = await ReceiveMessageAsync(fromUser.Stream, cancellationToken);
                if (!int.TryParse(targetIndexStr, out int targetIndex)) return;

                User? targetUser = null;
                lock (_usersLock)
                {
                    if (targetIndex >= 0 && targetIndex < _connectedUsers.Count)
                    {
                        targetUser = _connectedUsers[targetIndex];
                    }
                }

                if (targetUser == null || targetUser == fromUser)
                {
                    await fromUser.SendAsync("Invalid user selection.");
                    return;
                }

                await fromUser.SendAsync($"Started chat with {targetUser.UserName}");

                while (!cancellationToken.IsCancellationRequested &&
                    fromUser.Client.Connected &&
                    targetUser.Client.Connected)
                {
                    var message = await ReceiveMessageAsync(fromUser.Stream, cancellationToken);
                    if (string.IsNullOrEmpty(message)) break;

                    if (message.Equals("/back", StringComparison.OrdinalIgnoreCase)) break;

                    await targetUser.SendAsync($"{fromUser.UserName}: {message}");
                }

                await fromUser.SendAsync("Chat ended");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error in private chat: {ex.Message}");
            }
        }

        public async Task BroadcastAsync(string message, User? excludeUser = null)
        {
            List<User> usersToNotify;
            lock (_usersLock)
            {
                usersToNotify = _connectedUsers.ToList();
            }

            var tasks = usersToNotify
                .Where(user => user != excludeUser)
                .Select(user => SendSafeAsync(user, message));

            await Task.WhenAll(tasks);
        }

        public async Task SendToUserAsync(string userId, string message)
        {
            User? user;
            lock (_usersLock)
            {
                user = _connectedUsers.FirstOrDefault(u => u.Id == userId);
            }
            
            if (user != null)
            {
                await SendSafeAsync(user, message);
            }
        }

        private async Task SendSafeAsync(User user, string message)
        {
            try
            {
                await user.SendAsync(message);
            }
            catch(Exception ex) 
            {
                Console.WriteLine($"Error sending message to {user.UserName}: {ex.Message}");
            }
        }

        private async Task BroadcastUserListAsync()
        {
            var userList = GetFormattedUserList();
            await BroadcastAsync(userList);
        }
        private async Task SendUserListAsync(User user)
        {
            var userList = GetFormattedUserList();
            await SendSafeAsync(user, userList);
        }

        private string GetFormattedUserList()
        {
            lock (_usersLock)
            {
                return GetListUsers(_connectedUsers);
            }
        }

        static string GetListUsers(List<User> users) //формирует строку, в которой отображается список пользователей
        {
            string message = "Подключенные пользователи:\n";
            for (int i = 0; i < users.Count; i++)
            {
                message = message + $"\t{i}. {users[i].UserName}\n";
            }

            return message;
        }

        static public async Task<string> ReceiveMessageAsync(NetworkStream stream, CancellationToken cancellationToken)//получает сообщение
        { 
            byte[] buffer = new byte[4096];
            int byteReaded = await stream.ReadAsync(buffer, 0,buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, byteReaded);

            return message;
        }

        public void Dispose()
        {
            StopAsync().Wait();
            _cancellationTokenSource?.Dispose();
        }
    }
}
