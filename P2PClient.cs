using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using WebSocketSharp;

namespace BlockchainCore
{
    public class P2PClient
    {
        IDictionary<string, WebSocket> wsDict = new Dictionary<string, WebSocket>();
        public string FileLocation = @"C:\Users\geneva\OneDrive\Desktop\School Work\Final Project\network";

        public void Connect(string url)
        {
            if (!wsDict.ContainsKey(url))
            {
                WebSocket ws = new WebSocket(url);
                ws.OnClose += (sender, e) => {
                    ws.Connect();
                    ws.Send("Hi Server Node");
                    ws.Send($"ws://127.0.0.1:{Program.Port}");
                    ws.Send(JsonConvert.SerializeObject(Program.Kim));
                };
                ws.OnError += (sender, e) => {
                    ws.Connect();
                    ws.Send("Hi Server Node");
                    ws.Send($"ws://127.0.0.1:{Program.Port}");
                    ws.Send(JsonConvert.SerializeObject(Program.Kim));
                };
                ws.OnMessage += async (sender, e) => 
                {
                    if (e.Data == "Hi Client Node")
                    {
                        Console.WriteLine(e.Data);
                    }
                    else
                    {
                        var newChain = JsonConvert.DeserializeObject<Blockchain>(e.Data);

                        if (Program.Kim.Chain.Count == 1)
                        {
                            //var newNewTransactions = new List<Transaction>();
                            ////newNewTransactions.AddRange(newChain.PendingTransactions);
                            //newNewTransactions.AddRange(Program.Kim.PendingTransactions);

                            //newChain.PendingTransactions = newNewTransactions;
                            Program.Kim = newChain;
                            return;
                        }
                        if (!newChain.IsValid() || newChain.Chain.Count <= Program.Kim.Chain.Count) return;
                        var newTransactions = new List<Transaction>();
                        newTransactions = (List<Transaction>)newChain.PendingTransactions;

                        newChain.PendingTransactions = newTransactions;
                        Program.Kim = newChain;

                        await File.WriteAllTextAsync(FileLocation + Program._name + "KIMBLOCKCHAIN.txt", JsonConvert.SerializeObject(Program.Kim));
                    }
                };
                ws.Connect();
                ws.Send("Hi Server Node");
                ws.Send($"ws://127.0.0.1:{Program.Port}");
                ws.Send(JsonConvert.SerializeObject(Program.Kim));
                //ws.OnClose += (Event) => { Console.WriteLine("closed socket"); };
                //ws.on("close", function close() {
                //    console.log("Disconnected");
                //});
                wsDict.Add(url, ws);
            }
            else if(url == "ws://127.0.0.1:6001")
            {
                foreach (var item in wsDict)
                {
                    if (item.Key == url)
                    {
                        item.Value.Close();
                    }
                }
                WebSocket ws = new WebSocket(url);
                ws.Connect();
                ws.Send("Hi Server Node");
                ws.Send(JsonConvert.SerializeObject(Program.Kim));
            }
        }
        void webSocketClient_OnClose(object sender, WebSocketSharp.CloseEventArgs e)
        {

        }
        public void Send(string url, string data)
        {
            foreach (var item in wsDict)
            {
                if (item.Key == url)
                {
                    item.Value.Send(data);
                }
            }
        }
        public void SendToLightNode(string url, string data)
        {
            foreach (var item in wsDict)
            {
                if (item.Key == url)
                {
                    item.Value.Send(data);
                }
            }
        }

        public void Broadcast(string data)
        {
            foreach (var item in wsDict)
            {
                item.Value.Send(data);
            }
        }

        public IList<string> GetServers()
        {
            IList<string> servers = new List<string>();
            foreach (var item in wsDict)
            {
                servers.Add(item.Key);
            }
            return servers;
        }

        public void Close()
        {
            foreach (var item in wsDict)
            {
                item.Value.Close();
            }
        }
    }
}
