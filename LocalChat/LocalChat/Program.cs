using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Collections;

namespace LocalChat
{
    internal class Program
    {
        const int PORT = 11111;

        static async Task Main(string[] args)
        {
            IPAddress serverIp = IPAddress.Loopback;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);
            
            Server server = new Server(endPoint);

            Console.WriteLine("Напишите stopServer, чтобы остановить сервер");
            Console.ReadLine();
        }                
    }
}
