using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IO_lab4
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server(IPAddress.Any);
            Client c1 = new Client();
            var s = server.serverTaskRun();
            CancellationTokenSource cts1 = new CancellationTokenSource();
            c1.Connect();
            var t = c1.keepPinging("msg", cts1.Token);
            
            cts1.CancelAfter(3000);
            Task.WaitAll(s, t);
            
        }
        class Server
        {
            TcpListener serverListener;
            Task serverTask;
            public Task ServerTask
            {
                get { return serverTask; }
            }
            int port;
            IPAddress address;
            public IPAddress Address
            {
                get { return address; }
                set
                {
                    if (!isRunning) address = value;
                    else throw new Exception();
                }
            }
            bool isRunning = false;
            CancellationTokenSource cts = new CancellationTokenSource();

            public Server()
            {
                Address = IPAddress.Any;
                port = 2048;
            }
            public Server(int port)
            {
                this.port = port;
            }
            public Server(IPAddress address)
            {
                this.address = address;
            }

            async public Task serverTaskRun()
            {
                this.serverListener = new TcpListener(IPAddress.Any, 2048);
                serverListener.Start();
                Random r = new Random();
                while (true)
                {
                    TcpClient client = await serverListener.AcceptTcpClientAsync();
                    byte[] buffer = new byte[1024];
                    await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                        async (t) =>
                        {
                            int i = t.Result;
                            while (true)
                            {
                                client.GetStream().WriteAsync(buffer, 0, i);
                                i = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                                Console.ForegroundColor = (ConsoleColor)r.Next(0, 14);
                                Console.WriteLine(Encoding.ASCII.GetString(buffer, 0, i));
                            }
                        });
                }
            }
        }
            class Client
            {
                #region variables
                TcpClient client;
                #endregion
                #region properties
                #endregion
                #region Constructors
                #endregion
                #region Methods
                public void Connect()
                {
                    client = new TcpClient();
                    client.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2048));
                }
                public async Task<string> Ping(string message)
                {
                    byte[] buffer = new ASCIIEncoding().GetBytes(message);
                    client.GetStream().WriteAsync(buffer, 0, buffer.Length);
                    buffer = new byte[1024];
                    var t = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
                    return Encoding.UTF8.GetString(buffer, 0, t);
                }

                public async Task<IEnumerable<string>> keepPinging(string message, CancellationToken token)
                {
                    List<string> messages = new List<string>();
                    bool done = false;
                    while (!done)
                    {
                        if (token.IsCancellationRequested)
                            done = true;
                        messages.Add(await Ping(message));
                    }
                    return messages;
                }
                #endregion
            }
        
    }
}
