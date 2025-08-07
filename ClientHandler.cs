using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace FpsGameServer
{
    public class ClientHandler
    {
        public int ClientId { get; }
        private static int nextId = 1;
        private readonly TcpClient client;
        private readonly Server server;
        private NetworkStream? stream;

        public ClientHandler(TcpClient client, Server server)
        {
            this.client = client;
            this.server = server;
            this.ClientId = nextId++;
        }

        public void HandleClient()
        {
            try
            {
                stream = client.GetStream();
                using var reader = new StreamReader(stream, Encoding.UTF8);

                // 1. Send client its ID
                Send($"YOUR_ID:{ClientId}");

                // 2. Send the new client all existing player positions
                server.SendExistingPlayerPositionsTo(this);

                // 3. Notify all clients about the new player
                string joinMessage = $"PLAYER_JOIN:{ClientId}:0.0,-0.5,0.0";
                server.BroadcastToAll(joinMessage);

                // 4. Read messages
                string? message;
                while ((message = reader.ReadLine()) != null)
                {
                    if (message.StartsWith("POS:"))
                    {
                        string data = message.Substring(4);
                        string newMessage = $"POS:{ClientId}:{data}";
                        server.UpdatePlayerPosition(ClientId, data);
                        server.Broadcast(newMessage, this);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client] Error: {ex.Message}");
            }
            finally
            {
                client.Close();
                server.RemoveClient(this);

                // Notify others that this player left
                string leaveMsg = $"PLAYER_LEFT:{ClientId}";
                server.BroadcastToAll(leaveMsg);
            }
        }

        public void Send(string message)
        {
            if (stream != null && client.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message + "\n");
                stream.Write(data, 0, data.Length);
            }
        }
    }
}
