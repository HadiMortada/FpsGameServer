using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace FpsGameServer
{
    public class Server
    {
        private readonly int port;
        private TcpListener? listener;
        private bool isRunning;
        private readonly List<ClientHandler> clients = new();
        private readonly Dictionary<int, string> playerPositions = new();
        private readonly object clientLock = new();

        public Server(int port)
        {
            this.port = port;
        }

        public void Start()
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            isRunning = true;
            Console.WriteLine($"[Server] Listening on port {port}...");

            new Thread(AcceptClients) { IsBackground = true }.Start();
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                TcpClient tcpClient = listener!.AcceptTcpClient();
                Console.WriteLine("[Server] Client connected.");

                var handler = new ClientHandler(tcpClient, this);
                lock (clientLock) clients.Add(handler);

                new Thread(handler.HandleClient) { IsBackground = true }.Start();
            }
        }

        public void Broadcast(string message, ClientHandler sender)
        {
            lock (clientLock)
            {
                foreach (var client in clients)
                {
                    if (client != sender)
                        client.Send(message);
                }
            }
        }

        public void BroadcastToAll(string message)
        {
            lock (clientLock)
            {
                foreach (var client in clients)
                {
                    client.Send(message);
                }
            }
        }

        public void SendExistingPlayerPositionsTo(ClientHandler newClient)
        {
            lock (clientLock)
            {
                foreach (var kvp in playerPositions)
                {
                    string posMsg = $"POS:{kvp.Key}:{kvp.Value}";
                    newClient.Send(posMsg);
                }
            }
        }

        public void UpdatePlayerPosition(int clientId, string pos)
        {
            lock (clientLock)
            {
                playerPositions[clientId] = pos;
            }
        }

        public void RemoveClient(ClientHandler client)
        {
            lock (clientLock)
            {
                clients.Remove(client);
                playerPositions.Remove(client.ClientId);
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener!.Stop();
            Console.WriteLine("[Server] Server stopped.");
        }
    }
}
