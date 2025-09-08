using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using LocalChatClient;

namespace LocalChat
{
    internal class Server
    {
        TcpListener server;
        static List<User> users;

        public Server(IPEndPoint endPoint)
        {
            server = new TcpListener(endPoint);
            users = new List<User>();

            waitClientAsync(server, users);
        }

        static async void waitClientAsync(TcpListener server, List<User> users)
        {
            try
            {
                server.Start();
                Console.WriteLine("Сервер запущен, ожидание подключений...");

                while (true)
                {
                    TcpClient client = await server.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();
                    
                    
                    User user = new User(client, await recieveMessage(stream));
                    Console.WriteLine($"Подключен пользователь: {user.getUserName()} - {user.getClient().Client.RemoteEndPoint}");

                    if (client.Connected)
                    {
                        users.Add(user);
                    }

                    foreach (User user_ in users) { 
                        NetworkStream userStream = user_.getClient().GetStream();
                        sendMessage(getListUsers(users), userStream);
                    }

                    handleClientAsync(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                server.Stop();
            }
        }

        static async Task handleClientAsync(User user)
        {
            NetworkStream stream = user.getClient().GetStream();
            string optMessage = await recieveMessage(stream);
            if (optMessage == null)
            {
                Console.WriteLine("Клиент отключен");
            }
            else if (optMessage == "WRITE") {
                sendMessage(getListUsers(users), stream);
                int userIndex = int.Parse(await recieveMessage(stream));
                User currentUser = users[userIndex];
                sendMessage($"Начат чат с пользователем {currentUser.getUserName()}:{currentUser.getClient().Client.RemoteEndPoint.ToString()}",stream);
                NetworkStream sendStream = currentUser.getClient().GetStream();
                while (true){
                    sendMessage($"{currentUser.getUserName()}: {await recieveMessage(stream)}", sendStream);
                }
            }
            else if (optMessage == "GETUSERS") {  sendMessage(getListUsers(users), stream);}
        }

        static string getListUsers(List<User> users)//формирует строку, в которой отображается список пользователей
        {
            string message = "Подключенные пользователи:\n";
            for (int i = 0; i < users.Count; i++)
            {
                message = message + $"\t{i}. {users[i].getUserName()}\n";
            }

            return message;
        }

        static public void sendMessage(string message, NetworkStream stream)//отправляет сообщение
        {
        
            byte[] byteOptMessage = Encoding.UTF8.GetBytes(message);
            stream.Write(byteOptMessage, 0, byteOptMessage.Length);
        }

        static public async Task<string> recieveMessage(NetworkStream stream)//получает сообщение
        { 
            byte[] buffer = new byte[4096];
            int byteReaded = await stream.ReadAsync(buffer, 0,buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, byteReaded);

            return message;
        }
    }
}
