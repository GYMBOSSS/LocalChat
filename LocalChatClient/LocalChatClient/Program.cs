using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace LocalChatClient
{
    internal class Program
    {
        const int PORT = 11111;
        static async Task Main(string[] args)
        {
            TcpClient tcpClient = new TcpClient();
            Console.WriteLine("Введите своё имя");
            string name = Console.ReadLine();
            User user = new User(tcpClient, name);
            tcpClient.ConnectAsync(IPAddress.Loopback, PORT);

            if (tcpClient.Connected) {
                Console.WriteLine("Подключение выполнено");
                NetworkStream stream = tcpClient.GetStream();

                sendMessage(name, stream);
                handleServerMessages(tcpClient);
                Console.WriteLine("Список команд:\n /chat - войти в чат\n" +
                                                   "/users - посмотреть список подключенных пользоватетей\n" +
                                                   "/back - выйти из чата, когда ты уже переписываешься\n");
                while (tcpClient.Connected) {
                    Console.WriteLine("Введите команду(/help - для полного списка команд): ");
                    string command = Console.ReadLine();
                    if (command == "/chat"){
                        sendMessage("WRITE", stream);
                        sendMessage(Console.ReadLine(), stream);
                        Console.WriteLine("Чат начат(напиши /back, чтобы выйти из чата)");
                        while (true){
                            string message = Console.ReadLine();
                            if (message == "/back"){
                                break;
                            }
                            else{ 
                                sendMessage(message, stream);
                            }
                        }
                    }
                    else if(command == "/users"){
                        sendMessage("GETUSERS", stream);
                    }
                    else if (command == "/help") {
                        Console.WriteLine("Список команд:\n /chat - войти в чат\n" +
                                                   "/users - посмотреть список подключенных пользоватетей\n" +
                                                   "/back - выйти из чата, когда ты уже переписываешься\n");
                    }
                    else{
                        Console.WriteLine("Вы ввели неверную команду, попробуйте ещё раз");
                    }
                }
            }
            else{
                Console.WriteLine($"Не удалось подключиться к серверу: {IPAddress.Loopback}:{PORT}");
            }
        }

        static async Task handleServerMessages(TcpClient client) {
            NetworkStream stream = client.GetStream();
            while (true) {
                byte[] buffer = new byte[4096];
                int bytesReaded = await stream.ReadAsync(buffer,0,buffer.Length);
                string message = Encoding.UTF8.GetString(buffer,0,bytesReaded);
                
                Console.WriteLine(message);
            }
        }

        static public void sendMessage(string message, NetworkStream stream) {
            byte[] byteOptMessage = Encoding.UTF8.GetBytes(message);
            stream.Write(byteOptMessage, 0, byteOptMessage.Length);
        }

        static public async Task<string> recieveMessage(NetworkStream stream)
        {
            byte[] buffer = new byte[4096];
            int byteReaded = await stream.ReadAsync(buffer, 0, buffer.Length);
            string message = Encoding.UTF8.GetString(buffer, 0, byteReaded);

            return message;
        }
    }
}
