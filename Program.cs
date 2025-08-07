using System;

namespace FpsGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server(7777);
            server.Start();

            Console.WriteLine("Press Enter to shut down...");
            Console.ReadLine();
            server.Stop();
        }
    }
}
