using CardsAgainstHumanity.Core;
using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Server
{
    public static class GamePageConstructor
    {
        private static string Stylesheet { get; } = @".playerlist {
    vertical-align: top;
}

.card-container {
    display: flex;
    flex-wrap: nowrap;
    -webkit-flex-wrap: wrap;
}

.card > span {
    display: inline;
}

.black-card {
    background: #111111;
    color: white;
}

.white-card {
    background: #efefef;
    cursor: pointer;
    cursor: hand;
}

.white-card:hover {
    background: #dfdfdf;
}

@media (min-width:320px) {
	h1 {
		font-size: 4vw;
		margin-bottom: 1vw;
	}
	h3 {
		margin-top: 0vw;
		margin-bottom: 1vw;
	    font-size: 3vw;
	}

	span, p, b, body, tr, td, table, div {
	    font-size: 2.5vw;
	}

	.card {
	    padding: 1vw 1vw 1vw 1vw;
	    margin: 0 1vw 1vw 0;
	    width: 16vw;
	    min-height: 20vw;
	    display: flex;
	}

    #field {
        margin-bottom: 6vw;
    }
}
@media (min-width:1200px) {
	h1 {
		font-size: 2vw;
		margin-bottom: .5vw;
	}
	h3 {
		margin-top: 0vw;
		margin-bottom: 0.5vw;
	    font-size: 1.5vw;
	}

	span, p, b, body, tr, td, table, div {
	    font-size: 0.75vw;
	}

	.card {
	    padding: 0.5vw 0.5vw 0.5vw 0.5vw;
	    margin: 0 0.5vw 0.5vw 0;
	    width: 6vw;
	    min-height: 7.5vw;
	    display: flex;
	}

    #field {
        margin-bottom: 2vw
    }
}";

        public static string ConstructGamePage(Game game, User user)
        {
            return $@"<html>
    <head>
        <title>Cards Against Humanity Online - {game.Name}</title>
{GameServer.DefaultHeader}
        <style>
            {GamePageConstructor.Stylesheet}
        </style>
        <script language='Javascript'>
            function chooseCard(index) {{
                console.log(""Choosing card #"" + index + ""..."");
                var request = new XMLHttpRequest();
                request.onreadystatechange = function() {{
                    if (request.readyState == 4 && request.status == 200) {{
                        refresh();
                    }}
                }}
                request.open(""GET"", ""/game/{game.Id}/do/playcard?index="" + index, true);
                request.send(null);
            }}

            function chooseCardJudge(index) {{
                console.log(""Choosing card #"" + index + "" as the winning card..."");
                var request = new XMLHttpRequest();
                request.onreadystatechange = function() {{
                    if (request.readyState == 4 && request.status == 200) {{
                        refresh();
                    }}
                }}
                request.open(""GET"", ""/game/{game.Id}/do/judge?index="" + index, true);
                request.send(null);
            }}
            
            var intervalId = 0;
            function refresh() {{
                console.log(""Refreshing the game..."");
                var request = new XMLHttpRequest();
                request.onreadystatechange = function() {{
                    if (request.readyState == 4 && request.status == 200) {{
                        if (request.responseText == """") {{
                            clearInterval(intervalId);
                            console.log(""Stopped the update cycle."");
                        }} else if (request.responseText != ""ok"") {{
                            document.getElementById(""content"").innerHTML = request.responseText;
                            console.log(""Updated the content."");
                        }} else {{
                            console.log(""No update necessary."");
                        }}
                    }}
                }}
                var state = document.getElementById(""field"").dataset.state;
                request.open(""GET"", ""/refreshgame/{game.Id}?state="" + state, true);
                request.send(null);
            }}

            (function () {{
                intervalId = setInterval(function() {{
                    refresh(); 
                }}, 1000);
                refresh();
            }})();
        </script>
    </head>
    <body>
        <h1>Cards Against Humanity Online - {game.Name}</h1>
        <table width='100%'>
            <tr>
                <td colspan=""2"" id=""content"">
                    <span id=""field"" data-state=""{-1}"">Waiting for the server...</span>
                </td>
            </tr>
            <tr class=""footer"">
                <td width=""25%"">
                    Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)
                </td>
                <td width=""75%"">
                    This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
                </td>
            </tr>
        </table>
    </body>
</html>";
        }



        public static string ConstructRefreshPage(Game game, User user)
        {
            return $@"
<table width=""100%"" id=""field"" data-state=""{game.UpdateCounter}"">
    <tr>
        <td colspan=""2"">
            {GamePageConstructor.ConstructField(game, user)}
            {GamePageConstructor.ConstructWhiteCardCollection(game, user)}
        </td>
    </tr>
    <tr>
        <td width=""25%"">
            {GamePageConstructor.ConstructPlayerList(game, user)}
        </td>
        <td width=""75%"">
            {GamePageConstructor.ConstructHistoryList(game, user)}
        </td>
    </tr>
</table>";
        }



        private static string ConstructField(Game game, User user)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine($@"<h3>Field</h3><div class='card-container'><div class='card black-card'><span>{game.CurrentBlackCard?.Text}</span></div>");
            Player player = game.Players.First(p => p.User == user);
            if (player != game.Judge)
            {
                switch (game.State)
                {
                    case GameState.PlayingCards:
                        foreach (var playedCard in game.PlayedWhiteCards)
                        {
                            if (playedCard.Key == player)
                            {
                                result.AppendLine($@"<div class='card white-card'><span>{playedCard.Value.Text}</span></div>");
                            }
                            else
                            {
                                result.AppendLine($@"<div class='card white-card'><span></span></div>");
                            }
                        }
                        break;

                    case GameState.Judging:
                        foreach (var playedCard in game.PlayedWhiteCards)
                        {
                            result.AppendLine($@"<div class='card white-card'><span>{playedCard.Value.Text}</span></div>");
                        }
                        break;
                }
            }
            else
            {
                switch (game.State)
                {
                    case GameState.PlayingCards:
                        foreach (var playedCard in game.PlayedWhiteCards)
                        {
                            result.AppendLine($@"<div class='card white-card'><span></span></div>");
                        }
                        break;

                    case GameState.Judging:
                        List<WhiteCard> whiteCards = game.PlayedWhiteCards.Values.ToList();
                        for (int i = 0; i < game.PlayedWhiteCards.Count; i++)
                        {
                            result.AppendLine($@"<div class='card white-card' onclick='chooseCardJudge({i})'><span>{whiteCards[i].Text}</span></div>");
                        }
                        break;
                }
            }
            result.AppendLine(@"</div>");

            return result.ToString();
        }

        private static string ConstructWhiteCardCollection(Game game, User user)
        {
            if (user != game.Judge?.User)
            {
                StringBuilder result = new StringBuilder();

                result.AppendLine($@"<h3>Your White Cards</h3><div class='card-container'>");
                Player player = game.Players.First(p => p.User == user);
                if (game.PlayedWhiteCards.ContainsKey(player))
                {
                    for (int i = 0; i < player.WhiteCards.Count; i++)
                    {
                        result.AppendLine($@"<div class='card white-card'><span>{player.WhiteCards[i].Text}</span></div>");
                    }
                }
                else
                {
                    for (int i = 0; i < player.WhiteCards.Count; i++)
                    {
                        result.AppendLine($@"<div class='card white-card' onclick='chooseCard({i})'><span>{player.WhiteCards[i].Text}</span></div>");
                    }
                }
                result.AppendLine(@"</div>");

                return result.ToString();
            }
            else
            {
                if (game.State == GameState.Judging)
                {
                    return @"<p>You're the judge - select the card you like the most.<p><br>";
                }
                else
                {
                    return @"<p>You're the judge - wait for the other players to select their cards.<p><br>";
                }
            }
        }

        private static string ConstructPlayerList(Game game, User user)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(@"<div class='playerlist' height='100%'><h3>Players</h3>");
            foreach (Player player in game.Players.OrderByDescending(p => p.Points))
            {
                result.Append($@"<{(player.User == user ? "b" : "p")}>{player.User.Name}");
                if (game.Judge == player)
                {
                    result.Append(@" (Judge)");
                }
                else if (game.PlayedWhiteCards.ContainsKey(player))
                {
                    result.Append(@" (Played)");
                }
                result.AppendLine($@" - {player.Points} points</{(player.User == user ? "b" : "p")}>");
            }
            result.AppendLine(@"</div>");

            return result.ToString();
        }

        private static string ConstructHistoryList(Game game, User user)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine(@"<div class='history' height='100%'><h3>Last Rounds</h3>");
            foreach (RoundResult round in game.LastRounds.Take(3))
            {
                result.Append($@"<p><b>{round.BlackCard.Text}</b> - {round.WinningCard.Text} ({round.Winner.User.Name})</p>");
            }
            result.AppendLine(@"</div>");

            return result.ToString();
        }
    }
}