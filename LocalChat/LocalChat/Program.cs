using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Collections;
using AleksandrP.Services;

namespace LocalChat
{
    internal class Program
    {
        const int PORT = 8080;

        static async Task Main(string[] args)
        {
            IPAddress serverIp = IPAddress.Loopback;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            
            using Server server = new Server(endPoint);

            server.UserConnected += (sender, user) =>
            {
                Console.WriteLine($"[SYSTEM] {user.UserName} joined the chat");
            };

            server.UserDisconnected += (sender, user) =>
            {
                Console.WriteLine($"[SYSTEM] {user.UserName} left the chat");
            };

            await server.StartAsync();
            Console.WriteLine("Server started. Type 'stopServer' to stop the server");

            while (true)
            { 
                var input = Console.ReadLine();
                if (input?.ToLower() == "stopserver")
                {
                    await server.StopAsync();
                    break;
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }                
    }
}
