using CardsAgainstHumanity.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Server
{
    public static class GamePageConstructor
    {
        private static string Stylesheet { get; } = $@"h3 {{
    font-size: 1.5em;
    margin-bottom: .25em;
}}

span, p {{
    font-size: 1em;
}}

.playerlist {{
    vertical-align: top;
}}

.card-container {{
    display: flex;
    flex-wrap: nowrap;
    -webkit-flex-wrap: wrap;
}}

.card {{
    padding: 1em 1em 1em 1em;
    margin: 0 1em 1em 0;
    width: 8em;
    min-height: 10em;
    display: flex;
}}

.card > span {{
    display: inline;
}}

.black-card {{
    background: #111111;
    color: white;
}}

.white-card {{
    background: #efefef;
    cursor: pointer;
    cursor: hand;
}}

.white-card:hover {{
    background: #dfdfdf;
}}";

        public static string ConstructGamePage(Game game, User user)
        {
            return $@"<html>
    <head>
        <meta charset='UTF-8'>
        <title>Cards Against Humanity Online - {game.Name}</title>
        <style>
            {GamePageConstructor.Stylesheet}
        </style>
        <script language='Javascript'>
            function chooseCard(index) {{
                alert('You chose card #' + index);
            }}
        </script>
    </head>
    <body>
        <h1>Cards Against Humanity Online - {game.Name}</h1>
        <table width='100%'>
            <tr>
                <td>
                    {GamePageConstructor.ConstructPlayerList(game, user)}
                </td>
                <td>
                    {GamePageConstructor.ConstructField(game, user)}
                    {GamePageConstructor.ConstructWhiteCardCollection(game, user)}
                </td>
            </tr>
            <tr>
                <td>
                    Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)
                </td>
                <td>
                    This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
                </td>
            </tr>
        </table>
    </body>
</html>";
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
                else if (player.ChosenCardIndex > -1)
                {
                    result.Append(@" (Played)");
                }
                result.AppendLine($@" - {player.Points} points</{(player.User == user ? "b" : "p")}>");
            }
            result.AppendLine(@"</div>");

            return result.ToString();
        }

        private static string ConstructField(Game game, User user)
        {
            StringBuilder result = new StringBuilder();

            result.AppendLine($@"<h3>Field</h3><div class='card-container'><div class='card black-card'><span>{game.CurrentBlackCard.Text}</span></div>");
            foreach (Player player in game.Players)
            {
                if (player.ChosenCardIndex > -1)
                {
                    if (game.State == GameState.Judging)
                    {
                        result.AppendLine($@"<div class='card white-card'><span>{player.WhiteCards[player.ChosenCardIndex].Text}</span></div>");
                    }
                    else
                    {
                        result.AppendLine($@"<div class='card white-card'><span></span></div>");
                    }
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
                for (int i = 0; i < player.WhiteCards.Count; i++)
                {
                    if (player.ChosenCardIndex > -1)
                    {
                        if (player.ChosenCardIndex != i)
                        {
                            result.AppendLine($@"<div class='card white-card'><span>{player.WhiteCards[i].Text}</span></div>");
                        }
                    }
                    else
                    {
                        result.AppendLine($@"<div class='card white-card' onclick='chooseCard({i})'><span>{player.WhiteCards[i].Text}</span></div>");
                    }
                }
                result.AppendLine(@"</div>");

                return result.ToString();
            }
            else
            {
                return @"You're the judge - wait for the other players to select their cards.";
            }
        }
    }
}