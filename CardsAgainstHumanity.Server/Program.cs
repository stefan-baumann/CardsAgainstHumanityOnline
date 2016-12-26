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
            Console.WriteLine("Available Commands:\nopen - Open the local webpage in your default browser.\nstop - Stop the server\nlistgames - Show a list of all currently active games\ntest - Create a fake game for testing purposes");
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
                        foreach(Game game in server.Games.Values)
                        {
                            Console.WriteLine($"#{game.Id} - {game.Name} ({game.Players.Count} players)");
                        }
                        break;
                    case "test":
                        Console.WriteLine("Creating test game with fake players and launching the browser...");
                        HttpWebRequest request = HttpWebRequest.CreateHttp($"http://localhost:{port}/create/creategame?name=test&pass=test");
                        int id = int.Parse(new StreamReader(request.GetResponse().GetResponseStream()).ReadToEnd());
                        server.Games[id].Players.Add(new Player() { User = new User() { Id = 998, Name = "Dummy #01", Token = "a" }, Points = 2 });
                        server.Games[id].Players.Add(new Player() { User = new User() { Id = 999, Name = "Dummy #02", Token = "b" }, Points = 3 });
                        Process.Start($"http://localhost:{port}/join/{id}");
                        break;
                    default:
                        Console.WriteLine($"The command '{line}' could not be recognized.");
                        break;
                }
            }
        }
    }
}
