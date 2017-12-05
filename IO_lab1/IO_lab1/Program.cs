using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IO_lab1
{
    class Program
    {
        private static readonly Object obj = new object();
        static void Main(string[] args)
        { 
            invokeFifthTask();
        } 

        static void ThreadSleep(Object stateInfo)
        {
            int time = (int)((object[])stateInfo)[0];
            Thread.Sleep(time);
            Console.WriteLine("Watek czekal: " + time);
        }

        static void invokeFirstTask()
        {
            ThreadPool.QueueUserWorkItem(ThreadSleep, new object[] { 20 });
            Thread.Sleep(1000);
            ThreadPool.QueueUserWorkItem(ThreadSleep, new object[] { 50 });
            Thread.Sleep(1000);
        }

        static void createEchoServer(Object state)
        {
            int port = 1024;
            int i;
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            TcpListener server = new TcpListener(ip, port);
            server.Start();

            byte[] buffer = new byte[256];
            String data;
            while(true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Connected");

                NetworkStream stream = client.GetStream();
                while((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = System.Text.Encoding.ASCII.GetString(buffer, 0, i);
                    Console.WriteLine("Received: " + data);

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes("Message sent to client");
                    stream.Write(msg, 0, msg.Length);
                }
                client.Close();
            }
        }

        static void createClient(Object stateInfo)
        {
            int port = 1024;
            String message = "Msg from client nr: " + stateInfo.ToString();


            TcpClient client = new TcpClient();
            client.Connect("localhost", port);

            NetworkStream stream = client.GetStream();
            byte[] buffer = System.Text.Encoding.ASCII.GetBytes(message);

            Console.WriteLine("Client sent msg: ", stateInfo.ToString(), message);
            stream.Write(buffer, 0, buffer.Length);

            buffer = new byte[256];


            int bytes = stream.Read(buffer, 0, buffer.Length);

            String responseData = System.Text.Encoding.ASCII.GetString(buffer);

            Console.WriteLine("Client received: ", responseData);


            stream.Close();
            client.Close();
           
            Thread.Sleep(3000);
        }

        static void invokeSecondTask()
        {
            ThreadPool.QueueUserWorkItem(createEchoServer);
            Console.WriteLine("Podaj ile klientów utworzyć");
            int number = int.Parse(Console.ReadLine());


            for (int i = 0; i < number; i++)
            {
                ThreadPool.QueueUserWorkItem(createClient, i);
            }


            Thread.Sleep(3000);
        }

        static void acceptAndReceiveFromClients(Object state)
        {
            TcpClient client = (TcpClient)state;
            byte[] buff = new byte[512];

            client.GetStream().Read(buff, 0, 512);
            colorConsoleMsg(ConsoleColor.Red, "Server received from buffer: " + new ASCIIEncoding().GetString(buff));
            client.GetStream().Write(buff, 0, buff.Length);
            client.Close();
        }


        static void colorConsoleMsg(ConsoleColor color, String msg)
        {
            lock (obj)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(msg);
                Console.ResetColor();
            }

        }

        static void invokeThirdTask()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 1024);
            server.Start();
            ThreadPool.QueueUserWorkItem(createClient, "Client nr" );
            while (true)
            {
                ThreadPool.QueueUserWorkItem(acceptAndReceiveFromClients, server.AcceptTcpClient());
            }
        }

        /*
        static void invokeFifthTask()
        {
            int[] array = new int[30];
            Random randNum = new Random();
            for(int i = 0; i<array.Length; i++)
            {
                array[i] = randNum.Next(0, 10000);
            }
            int[] partialSums = new int[3];
            Parallel.For(0, 3, (counter) =>
            {
                int sum = 0;
                for (int i = counter * 10; i < (counter + 1) * 10; i++)
                    sum += array[i];
                partialSums[counter] = sum;
                Console.WriteLine( partialSums[counter]);
            });
        }
        */


    }
}
