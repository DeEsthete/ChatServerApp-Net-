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
                    //-------Помещаем сокет в массив
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
                        socketIndex = 0;
                    }
                    //-------
                    //-------Принимаем данные с сокета
                    int bytes;
                    byte[] buffer = new byte[1024];
                    StringBuilder stringBuilder = new StringBuilder();

                    do
                    {
                        bytes = sockets[socketIndex].Receive(buffer);
                        stringBuilder.Append(Encoding.Default.GetString(buffer));
                    }
                    while (sockets[socketIndex].Available > 0);

                    UserMessage newMessage = JsonConvert.DeserializeObject<UserMessage>(stringBuilder.ToString());
                    //-------
                    //-------Посылаем необходимое сообщение
                    if (newMessage.Message == "init")
                    {
                        Console.WriteLine("В чат вошел... " + newMessage.UserName);
                        for (int i = 0; i < sockets.Count; i++)
                        {
                            if (i != socketIndex)
                            {
                                sockets[i].Send(Encoding.Default.GetBytes("В чат вошел " + newMessage.UserName));
                            }
                        }
                    }
                    else if (newMessage.Message == "exit")
                    {
                        Console.WriteLine("Из чата вышел... " + newMessage.UserName);
                        for (int i = 0; i < sockets.Count; i++)
                        {
                            if (i != socketIndex)
                            {
                                sockets[i].Send(Encoding.Default.GetBytes("Из чата вышел " + newMessage.UserName));
                            }
                        }
                        sockets[socketIndex].Shutdown(SocketShutdown.Both);
                    }
                    else
                    {
                        Console.WriteLine("Сообщение отправил... " + newMessage.UserName);
                        for (int i = 0; i < sockets.Count; i++)
                        {
                            if (i != socketIndex)
                            {
                                sockets[i].Send(Encoding.Default.GetBytes(newMessage.UserName + ": " + newMessage.Message));
                            }
                        }
                    }
                    //-------

                    //clientSocket.Shutdown(SocketShutdown.Receive);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                serverSocket.Close();
                for (int i = 0; i < sockets.Count; i++)
                {
                    sockets[i].Shutdown(SocketShutdown.Both);
                }
                Console.WriteLine("Сервер завершил свою работу...");
            }
        }
    }
}
