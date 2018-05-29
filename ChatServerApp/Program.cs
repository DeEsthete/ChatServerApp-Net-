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
        private const int MAX_CLIENT_QUEUE = 3;
        private static List<Socket> sockets;
        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), SERVER_PORT);
            serverSocket.Bind(endPoint);
            sockets = new List<Socket>();

            try
            {
                serverSocket.Listen(MAX_CLIENT_QUEUE);
                Console.WriteLine("Сервер запущен...");

                while (true)
                {
                    Socket clientSocket = serverSocket.Accept();

                    int socketIndex = -1;
                    for (int i = 0; i < sockets.Count; i++)
                    {
                        if (sockets[i] == clientSocket)
                        {
                            socketIndex = i;
                        }
                    }
                    if (socketIndex == -1)
                    {
                        sockets.Add(clientSocket);
                    }

                    Console.WriteLine("Входящее соединение...");
                    //-----------------------------|||--------------------------
                    int bytes;
                    byte[] buffer = new byte[1024];
                    StringBuilder stringBuilder = new StringBuilder();

                    do
                    {
                        bytes = clientSocket.Receive(buffer);
                        stringBuilder.Append(Encoding.Default.GetString(buffer));
                    }
                    while (clientSocket.Available > 0);

                    UserMessage newMessage = JsonConvert.DeserializeObject<UserMessage>(stringBuilder.ToString());
                    clientSocket.Send();

                    clientSocket.Shutdown(SocketShutdown.Receive);
                    //-----------------------------|||--------------------------
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                serverSocket.Close();
            }
        }
    }
}
