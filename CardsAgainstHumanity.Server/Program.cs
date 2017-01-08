using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardsAgainstHumanity.Server;
using System.Diagnostics;
using CardsAgainstHumanity.Core;
using System.Net;
using System.IO;

namespace CardsAgainstHumanity.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Cards Against Humanity Online Server";
            Console.WriteLine("Cards Against Humanity Online Server");
            Console.WriteLine("Available Commands:\nopen - Open the local webpage in your default browser.\nstop - Stop the server\nlistgames - Show a list of all currently active games\ntest - Create a fake game for testing purposes and join it with the local browser");
            Console.WriteLine(new string('-', Console.WindowWidth - 1));

            int port = 31815; //C/3, A/1, H/8, O/15
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
                        foreach(Game game in server.Game.Games.Values)
                        {
                            Console.WriteLine($"#{game.Id} - {game.Name} ({game.Players.Count} players)");
                        }
                        break;
                    
                    case "test":
                        Console.WriteLine("Creating test game with fake players and launching the browser...");
                        Game testGame = server.Game.CreateGame("Test-Game", "test");
                        User dummy1 = server.Game.CreateUser("Fake Player #01");
                        User dummy2 = server.Game.CreateUser("Fake Player #02");
                        server.Game.JoinGame(testGame.Id, "test", dummy1.Id);
                        server.Game.JoinGame(testGame.Id, "test", dummy2.Id);

                        Process.Start($"http://localhost:{port}/join/{testGame.Id}?pass=test");
                        break;
                    default:
                        Console.WriteLine($"The command '{line}' could not be recognized.");
                        break;
                }
            }
        }
    }
}
