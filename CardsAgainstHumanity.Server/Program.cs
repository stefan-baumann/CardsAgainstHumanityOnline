using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardsAgainstHumanity.Server;
using System.Diagnostics;
using CardsAgainstHumanity.Core;

namespace CardsAgainstHumanity.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Cards Against Humanity Online Server");
            Console.WriteLine(new string('-', Console.WindowWidth - 1));

            int port = 8080;
            GameServer server = new GameServer(port);
            server.Start();

            //Command-line methods
            while (true)
            {
                string line = Console.ReadLine();
                switch (line)
                {
                    case "open":
                        Console.WriteLine("Opening the local web page...");
                        Process.Start($"http://localhost:{port}");
                        break;
                    case "stop":
                        server.Stop();
                        return;
                    case "listgames":
                        Console.WriteLine("Active games:");
                        foreach(Game game in server.Games.Values)
                        {
                            Console.WriteLine($"#{game.Id} - {game.Name} ({game.Players.Count} players)");
                        }
                        break;
                }
            }
        }
    }
}
