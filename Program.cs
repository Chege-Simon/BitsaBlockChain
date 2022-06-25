using Newtonsoft.Json;
using System;
using System.Threading;

namespace BlockchainCore
{
    class Program
    {
        public static int Port;
        private static P2PServer _server;
        public static readonly P2PClient Client = new P2PClient();
        public static Blockchain Kim = new Blockchain();
        public static string _name = "Unknown";

        static void Main(string[] args)
        {
            Kim.InitializeChain();

            if (args.Length >= 1)
                Port = int.Parse(args[0]);
            if (args.Length >= 2)
                _name = args[1];

            if (Port > 0)
            {
                _server = new P2PServer();
                _server.Start();
            }
            if (_name != "Unkown")
            {
                Console.WriteLine($"Current user is {_name}");
            }

            Console.WriteLine("=========================");
            Console.WriteLine("1. Connect to other network nodes");
            //Console.WriteLine("2. Add a transaction");
            //Console.WriteLine("3. Display Blockchain");
            Console.WriteLine("4. Exit");
            Console.WriteLine("=========================");

            int selection = 0;
            while (selection != 4)
            {
                switch (selection)
                {
                    case 1:
                        string serverUrl = "ws://127.0.0.1:600";
                        try
                        {
                            for (int i = 2; i < 5; i++)
                            {
                                serverUrl = "ws://127.0.0.1:600";
                                serverUrl += i;
                                if (serverUrl != $"ws://127.0.0.1:{Port}" && i < Port-6000)
                                {
                                    Console.WriteLine("Conncting to: " + serverUrl);
                                    Client.Connect($"{serverUrl}/Blockchain");
                                    Thread.Sleep(TimeSpan.FromSeconds(3));
                                }
                            }
                        }
                        catch (Exception)
                        {

                            break;
                        }
                        break;
                    case 2:
                        Console.WriteLine("Please enter the receiver name");
                        string receiverName = Console.ReadLine();
                        Console.WriteLine("Please enter the amount");
                        string amount = Console.ReadLine();
                        Kim.CreateTransaction(new Transaction(_name, receiverName, int.Parse(amount)));
                        Kim.ProcessPendingTransactions(_name);
                        Client.Broadcast(JsonConvert.SerializeObject(Kim));
                        break;
                    case 3:
                        Console.WriteLine("Blockchain");
                        Console.WriteLine(JsonConvert.SerializeObject(Kim, Formatting.Indented));
                        break;

                }

                Console.WriteLine("Please select an action");
                string action = Console.ReadLine();
                selection = int.Parse(action);
            }

            Client.Close();
        }
    }
}
