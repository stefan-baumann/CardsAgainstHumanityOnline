using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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
                Console.WriteLine($"Received request for /");
            }
            else
            {
                path = requestTarget.Split('/');
                Console.WriteLine($"Received request for {requestTarget}.");
            }
            
            if (this.ProcessRequestInternal(path, context))
            {
                Console.WriteLine($"Processed request for {requestTarget}.");
            }
            else
            {
                Console.WriteLine($"Could not process request for {requestTarget}.");
                base.ProcessRequest(context);
            }
        }

        protected bool ProcessRequestInternal(string[] path, HttpListenerContext context)
        {
            switch (path[0])
            {
                case "test":
                    this.ProcessTestRequest(context);
                    return true;
                default:
                    return false;
            }
        }



        protected void ProcessTestRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;

            BlackCard blackCard = this.CardDatabase.GetBlackCard();
            IEnumerable<WhiteCard> whiteCards = Enumerable.Repeat(1, 10).Select(o => this.CardDatabase.GetWhiteCard());

            string response = $@"<html>
    <head>

    </head>
    <body>
        <h1>Cards Against Humanity Random Card Test</h1>
        <h2>Random Black Card</h2>
        <p>{blackCard.Text}</p>
        <h2>Random White Cards</h2>
        {string.Join(Environment.NewLine, whiteCards.Select(card => $"<p>{card.Text}</p>"))}
    </body>
</html>";

            context.WriteString(response);
        }
    }
}
