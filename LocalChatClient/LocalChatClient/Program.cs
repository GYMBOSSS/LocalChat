using LocalChatClient.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.IO;

namespace LocalChatClient
{
    internal class Program
    {
        const int PORT = 11111;
        static async Task Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            string name;
            FileInfo fileInfo = new FileInfo("cfg.txt");
            if (fileInfo.Exists)
            {
                name = File.ReadAllText(fileInfo.FullName);
            }
            else
            {
                Console.WriteLine("Введите своё имя");
                name = Console.ReadLine();
                File.WriteAllText(fileInfo.FullName, name);
            }

            User user = new User(tcpClient, name != null ? name : "JustUser");
            _ = tcpClient.ConnectAsync(IPAddress.Loopback, PORT);

            if (tcpClient.Connected) 
            {
                Console.WriteLine("Подключение выполнено");
                NetworkStream stream = tcpClient.GetStream();

                sendMessage(name, stream);
                handleServerMessages(tcpClient);
                spisok();

                while (tcpClient.Connected) 
                {
                    Console.WriteLine("Введите команду(/help - для полного списка команд): ");
                    string command = Console.ReadLine();

                    switch (command)
                    {
                        case "/chat":
                            sendMessage("WRITE", stream);

                            string currentUserIndex = Console.ReadLine();
                            sendMessage(currentUserIndex,stream);

                            Console.WriteLine("Чат начат(напиши /back, чтобы выйти из чата)");
                            while (true)
                            {
                                string message = Console.ReadLine();
                                if (message == "/back")
                                {
                                    break;
                                }
                                else
                                { 
                                    sendMessage(message, stream);
                                }
                            }
                            break;

                        case "/users":
                            sendMessage("GETUSERS", stream);
                            break;

                        case "/help":
                            spisok();
                            break;

                        default:
                            Console.WriteLine("Вы ввели неверную команду, попробуйте ещё раз");
                            break;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Не удалось подключиться к серверу: {IPAddress.Loopback}:{PORT}");
            }
        }

        static async Task handleServerMessages(TcpClient client) // получает сообщения от сервера
        {
            NetworkStream stream = client.GetStream();
            while (true) 
            {
                byte[] buffer = new byte[4096];
                int bytesReaded = await stream.ReadAsync(buffer,0,buffer.Length);
                string message = Encoding.UTF8.GetString(buffer,0,bytesReaded);
                
                Console.WriteLine(message);
            }
        }
        static public void sendMessage(string message, NetworkStream stream) // отправляет сообщение
        {
            byte[] byteOptMessage = Encoding.UTF8.GetBytes(message);
            stream.Write(byteOptMessage, 0, byteOptMessage.Length);
        }

        public static void spisok() // для вывода списка комманд
        {
            Console.WriteLine("Список команд:\n /chat - войти в чат\n" +
                                                   "/users - посмотреть список подключенных пользоватетей\n" +
                                                   "/back - выйти из чата, когда ты уже переписываешься\n");
        }
    }
}
