using CardsAgainstHumanity.Core;
using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Server
{
    public class GameServer
        : WebServerBase
    {
        public GameServer(int port)
#if DEBUG
            : base($"http://localhost:{port}/")
#else
            : base($"http://*:{port}/")
#endif
        { }

        protected CardDatabase CardDatabase { get; set; } = CardDatabase.InitializeFromSet(CardDatabase.MainSet);

        public override void ProcessRequest(HttpListenerContext context)
        {
            string requestTarget = context.Request.RawUrl.Trim();
            if (requestTarget.StartsWith("/"))
            {
                requestTarget = requestTarget.Substring(1);
            }
            if (requestTarget.EndsWith("/"))
            {
                requestTarget = requestTarget.Remove(requestTarget.Length - 1);
            }

            string[] path;
            if (string.IsNullOrWhiteSpace(requestTarget))
            {
                path = new[] { "/" };
                //Console.WriteLine($"Received request for /");
            }
            else
            {
                path = requestTarget.Split('/');
                //Console.WriteLine($"Received request for {requestTarget}.");
            }
            
            if (this.ProcessRequestInternal(path, context))
            {
                //Console.WriteLine($"Processed request for {requestTarget ?? "/"}.");
            }
            else
            {
                Console.WriteLine($"Could not process request for {requestTarget}, returning empty result...");
                //base.ProcessRequest(context);
            }
        }

        protected bool ProcessRequestInternal(string[] path, HttpListenerContext context)
        {
            switch (path[0])
            {
                case "/":
                    this.ProcessHomeSiteRequest(context);
                    return true;
                case "test":
                    this.ProcessTestSiteRequest(context);
                    return true;
                case "create":
                    Regex createRegex = new Regex(@"creategame\?name=(?<Name>\w+)&pass=(?<Password>\w+)");
                    if (path.Length == 2 && createRegex.IsMatch(path[1]))
                    {
                        Match match = createRegex.Match(path[1]);
                        this.ProcessCreateGameRequest(context, match.Groups["Name"].Value, match.Groups["Password"].Value);
                    }
                    else
                    {
                        this.ProcessCreateGameSiteRequest(context);
                    }
                    return true;
                case "join":
                    this.ProcessJoinGameSiteRequest(context);
                    return true;
                case "favicon.ico":
                    return true; //Just swallow
                default:
                    return false;
            }
        }

        protected void ProcessHomeSiteRequest(HttpListenerContext context)
        {
            Console.WriteLine($"Delivering main page to {context.Request.UserHostAddress}...");
            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online</title>
    </head>
    <body>
        <h1>Cards Against Humanity Online</h1>
        <p>Welcome to Cards Against Humanity Online - a small webserver which allows you to play the game 'Cards Against Humanity' together with your friends online - have fun!</p>
        <p>
            <button onclick='window.location.href=""join"";'>Join Game</button>
            <button onclick='window.location.href=""create"";'>Create Game</button>
            <a href='/test/'>Test page</a>
        </p>
        <br>
        <p>Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)</p>
        <p>This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
    </body>
</html>";
            context.WriteString(response);
        }

        protected void ProcessJoinGameSiteRequest(HttpListenerContext context)
        {
            Console.WriteLine($"Delivering 'join game'-page to {context.Request.UserHostAddress}...");
            IEnumerable<Game> games = new[]
            {
                new Game() { Id = 1, Name = "Test-Game #1", Players = new List<Player>() { new Player(), new Player() } },
                new Game() { Id = 2, Name = "Test-Game #2", Players = new List<Player>() { new Player(), new Player(), new Player(), new Player(), new Player() } }
            };

            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online - Join Game</title>
    </head>
    <body>
        <h1>Cards Against Humanity Online - Join Game</h1>
        <p>Click on the name of a game to join it.</p>
        <p>
            {string.Join(Environment.NewLine, games.Select(game => $"<p><a href='/../game/{game.Id}'>{game.Name}</a> ({game.Players.Count} players)</p>"))}
        </p>
        <br>
        <p>Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)</p>
        <p>This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
    </body>
</html>";
            context.WriteString(response);
        }

        protected void ProcessCreateGameSiteRequest(HttpListenerContext context)
        {
            Console.WriteLine($"Delivering 'create game'-page to {context.Request.UserHostAddress}...");
            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online - Create Game</title>
        <script language='JavaScript'>
            function createGame() {{
                var request = new XMLHttpRequest();
                request.onreadystatechange = function() {{
                    if (request.readyState == 4 && request.status == 200) {{
                        window.location.href = ""/../game/"" + request.responseText;
                    }}
                }}
                request.open(""GET"", ""/create/creategame?name="" + namebox.value + ""&pass="" + passwordbox.value, true);
                request.send(null);
            }}
        </script>
    </head>
    <body>
        <h1>Cards Against Humanity Online - Create Game</h1>
        <p>Create a new game with the specified name and password.</p>
        <p>Name: <input id='namebox'></input></p>
        <p>Password: <input id='passwordbox'></input></p>
        <p><button onclick='createGame()'>Create Game</button></p>
        <br>
        <p>Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)</p>
        <p>This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
    </body>
</html>";
            context.WriteString(response);
        }

        protected void ProcessTestSiteRequest(HttpListenerContext context)
        {
            Console.WriteLine($"Delivering the test-page to {context.Request.UserHostAddress}...");

            BlackCard blackCard = this.CardDatabase.GetBlackCard();
            IEnumerable<WhiteCard> whiteCards = Enumerable.Repeat(1, 10).Select(o => this.CardDatabase.GetWhiteCard());

            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online</title>
    </head>
    <body>
        <h1>Cards Against Humanity Online - Random Card Test</h1>
        <h2>Random Black Card</h2>
        <p>{blackCard.Text}</p>
        <h2>Random White Cards</h2>
        {string.Join(Environment.NewLine, whiteCards.Select(card => $"<p>{card.Text}</p>"))}
    </body>
</html>";
            context.WriteString(response);
        }



        protected void ProcessCreateGameRequest(HttpListenerContext context, string name, string password)
        {
            Console.WriteLine($"{context.Request.UserHostAddress} created a new game with name '{name}' and password '{password}'.");
            context.WriteString("1"); //Dummy-game with id 1
        }
    }
}
