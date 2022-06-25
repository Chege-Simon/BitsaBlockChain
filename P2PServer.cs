using EllipticCurve;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BlockchainCore
{
    public class P2PServer: WebSocketBehavior
    {
        private bool _chainSynched;
        private WebSocketServer _wss;
        public string FileLocation = @"C:\Users\geneva\OneDrive\Desktop\School Work\Final Project\network\";
        public void Start()
        {
            _wss = new WebSocketServer($"ws://127.0.0.1:{Program.Port}");
            _wss.AddWebSocketService<P2PServer>("/Blockchain");
            _wss.Start();
            Console.WriteLine($"Started server at ws://127.0.0.1:{Program.Port}");
        }

        protected override async void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "Hi Server Node")
            {
                Console.WriteLine(e.Data);
                Send("Hi Client Node");
                Send(JsonConvert.SerializeObject(Program.Kim));

            }
            else if (e.Data.Contains("ws://127.0.0.1:"))
            {
                Program.Client.Connect($"{e.Data}/Blockchain");
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
            else if (JsonConvert.DeserializeObject<Load>(e.Data).Signature != null)
            {
                Console.WriteLine("Verifing Transaction Signature!");
                Load load = JsonConvert.DeserializeObject<Load>(e.Data);
                // Verify if signature is valid
                if (!Ecdsa.verify(load.trxHash, load.Signature, load.Pubkey))
                {
                    Console.WriteLine("Signature is valid!");
                    Console.WriteLine("Mining New Block!");
                    //Transaction newTransaction = JsonConvert.DeserializeObject<Transaction>(e.Data);
                    Transaction newTransaction = load.Transaction;
                    Program.Kim.CreateTransaction(new Transaction(newTransaction.FromAddress, newTransaction.ToAddress, newTransaction.Amount));
                    Program.Kim.ProcessPendingTransactions(Program._name);
                    Program.Client.Broadcast(JsonConvert.SerializeObject(Program.Kim));
                    Console.WriteLine("New Block Added.");
                    Console.WriteLine("Updating BlockChain.");
                    await File.WriteAllTextAsync(FileLocation + Program._name + "KIMBLOCKCHAIN.txt", JsonConvert.SerializeObject(Program.Kim));
                }

            }
            else
            {
                Blockchain newChain = JsonConvert.DeserializeObject<Blockchain>(e.Data);

                if (newChain.IsValid() && newChain.Chain.Count > Program.Kim.Chain.Count)
                {
                    List<Transaction> newTransactions = new List<Transaction>();
                    //newTransactions.AddRange(newChain.PendingTransactions);
                    newTransactions = (List<Transaction>)newChain.PendingTransactions;

                    newChain.PendingTransactions = newTransactions;
                    Program.Kim = newChain;

                    await File.WriteAllTextAsync(FileLocation + Program._name + "KIMBLOCKCHAIN.txt", JsonConvert.SerializeObject(Program.Kim));
                }

                if (!_chainSynched)
                {
                    Send(JsonConvert.SerializeObject(Program.Kim));
                    _chainSynched = true;
                }
            }
        }
    }
}
