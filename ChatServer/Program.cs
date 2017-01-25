using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatServer
{
    class Receiver
    {
        NetworkStream NS = null;
        StreamReader SR = null;
        StreamWriter SW = null;
        TcpClient client;

        public void startClient(TcpClient clientSocket)
        {
            client = clientSocket;
            Thread echo_thread = new Thread(echo);
            echo_thread.Start();
        }

        public void echo()
        {
            NS = client.GetStream(); // 소켓에서 메시지를 가져오는 스트림
            SR = new StreamReader(NS, Encoding.UTF8); // Get message
            SW = new StreamWriter(NS, Encoding.UTF8); // Send message

            string GetMessage = string.Empty;

            while (client.Connected == true) //클라이언트 메시지받기
            {
                try
                {
                    GetMessage = SR.ReadLine();

                    if (GetMessage == "G00D-BY2")
                    {
                        string name = Program.clientList[client];
                        Console.WriteLine("{0}가 접속을 종료했습니다. ID : {1}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString(), name);
                        Program.clientList.Remove(client); // 키 값으로 값 삭제

                        SW.Close();
                        SR.Close();
                        client.Close();
                        NS.Close();

                        if (Program.clientList.Count >= 1)
                        {
                            Program.sendAll(name + "님이 접속을 종료했습니다.");
                        }

                        break;
                    }
                    else
                    {
                        Program.sendAll(GetMessage);
                        Console.WriteLine("Log: {0} - {1} [{2}]", ((IPEndPoint)client.Client.RemoteEndPoint).ToString(), GetMessage, DateTime.Now);
                    }
                }

                catch (Exception ee)
                {
                    Console.WriteLine(ee.ToString());
                }
            }
        }
    }

    class Program
    {
        public static Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>();

        public static void sendAll(string msg)
        {
            foreach (var list in clientList)
            {
                TcpClient client = list.Key as TcpClient;
                NetworkStream stream = client.GetStream();
                StreamWriter sendMsg = new StreamWriter(stream, Encoding.UTF8);

                sendMsg.WriteLine(msg); // 메시지 보내기
                sendMsg.Flush();
            }
        }

        static void Main(string[] args)
        {
            TcpListener Listener = null;
            TcpClient client = null;

            NetworkStream NS = null;
            StreamReader SR = null;

            const int PORT = 44444;

            Console.WriteLine("서버가 시작되었습니다.");
            Console.WriteLine("서버 포트 : {0}", PORT);
            Console.WriteLine("----------------------------");

            try
            {
                Listener = new TcpListener(PORT);
                Listener.Start(); // Listener 동작 시작

                while (true)
                {
                    client = Listener.AcceptTcpClient();
                    NS = client.GetStream(); // 소켓에서 메시지를 가져오는 스트림
                    SR = new StreamReader(NS, Encoding.UTF8); // Get message

                    string GetMessage = SR.ReadLine();
                    string[] MsgResult = GetMessage.Split(':');

                    if (MsgResult[0] == "my-id") // id 구분
                    {
                        Console.WriteLine("클라이언트 {0}가 접속했습니다. - ID : {1}", ((IPEndPoint)client.Client.RemoteEndPoint).ToString(), MsgResult[1]); // 클라이언트 구별
                        clientList.Add(client, MsgResult[1]); // 키 값
                        sendAll(MsgResult[1] + "님이 접속했습니다.");
                        Console.WriteLine("현재 유저 수 {0}명", clientList.Count);
                    }

                    Receiver r = new Receiver();
                    r.startClient(client);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
