using System;

namespace FpsGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the PORT from the environment variable, or default to 7777
            string portEnv = Environment.GetEnvironmentVariable("PORT") ?? "7777";
            int port = int.Parse(portEnv);

            Server server = new Server(port);
            server.Start();

            Task.Delay(-1).Wait(); // keeps the server alive forever

            Console.ReadLine();

            server.Stop();
        }
    }
}

