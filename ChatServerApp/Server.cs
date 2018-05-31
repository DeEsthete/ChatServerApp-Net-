using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServerApp
{
    public class Server
    {
        private const int MAX_CLIENT_QUEUE = 3;
        private List<Socket> sockets = new List<Socket>();

        public void Work(Socket serverSocket)
        {
            try
            {
                Console.WriteLine("Сервер запущен...");
                serverSocket.Listen(MAX_CLIENT_QUEUE);
                while (true)
                {
                    //-------Помещаем сокет в массив
                    sockets.Add(serverSocket.Accept());
                    int socketIndex = sockets.Count - 1;
                    //-------
                    ConnectClient(socketIndex);
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

        public Task ConnectClient(int socketIndex)
        {
            return Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
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
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}
