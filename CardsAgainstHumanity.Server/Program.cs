using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardsAgainstHumanity.Server;

namespace CardsAgainstHumanity.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServerBase server = new GameServer(8080);
            server.Start();
            Console.ReadKey();
        }
    }
}
