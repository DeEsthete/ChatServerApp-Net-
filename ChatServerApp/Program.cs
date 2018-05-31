using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerApp
{
    class Program
    {
        private const int SERVER_PORT = 3535;
        private static List<Socket> sockets;
        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), SERVER_PORT);
            serverSocket.Bind(endPoint);

            Server server = new Server();
            server.Work(serverSocket);
            
        }
    }
}
