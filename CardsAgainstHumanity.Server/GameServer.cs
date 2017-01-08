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



        public GameDatabase Game { get; set; } = new GameDatabase();

        protected internal CardDatabase TestCardDatabase { get; set; } = CardDatabase.InitializeFromSet(CardDatabase.MainSet);



        public override void ProcessRequest(HttpListenerContext context)
        {
            string requestTarget = context.Request.RawUrl.Trim();

            //Console.WriteLine($"{context.Request.UserHostAddress} requested '{requestTarget}'");

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
                path = requestTarget.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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

        protected internal bool ProcessRequestInternal(string[] path, HttpListenerContext context)
        {
            int length = path.Length;

            Regex parameterUrlRegex = new Regex(@"\A(?<Target>.*?)(?<Parameters>\?(?<Parameter>[\w-_]+=[\w-_]*)(\&(?<Parameter>[\w-_]+=[\w-_]*))*)\Z");
            Regex parameterRegex = new Regex(@"\A(?<Name>[\w-_]+)=(?<Value>[\w-_]*)\Z");

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            if (length > 0)
            {
                if (parameterUrlRegex.IsMatch(path.Last()))
                {
                    Match match = parameterUrlRegex.Match(path.Last());
                    foreach (Capture capture in match.Groups["Parameter"].Captures)
                    {
                        Match parameterMatch = parameterRegex.Match(capture.Value);
                        string name = parameterMatch.Groups["Name"].Value.ToLowerInvariant();
                        string value = parameterMatch.Groups["Value"].Value;
                        if (!parameters.ContainsKey(name))
                        {
                            parameters.Add(name, value);
                        }
                    }
                    path[length - 1] = match.Groups["Target"].Value;
                }
            }



            int gameId, userId;
            switch (path[0])
            {
                case "/" when parameters.Count == 0: //Default main page
                    this.ProcessHomeSiteRequest(context);
                    return true;

                case "create" when length == 1 && parameters.Count == 0: //Page for creating games, syntax: server/create/
                    this.ProcessCreateGameSiteRequest(context);
                    return true;

                case "create" when length == 1 && parameters.Count == 2 && parameters.ContainsKey("name") && parameters.ContainsKey("pass"):
                    this.ProcessCreateGameRequest(context, parameters["name"], parameters["pass"]);
                    return true;

                case "join" when length == 1 && parameters.Count == 0: //Join-site which provides an overview of all active games, syntax: server/join/
                    this.ProcessJoinGameSiteRequest(context);
                    return true;

                case "createuser" when length == 1 && parameters.ContainsKey("name"): //Interface for creating users, syntax: server/createuser?name=<Name>
                    this.ProcessCreateUserRequest(context, parameters["name"]);
                    return true;

                case "verify" when length == 1 && parameters.Count == 2 && parameters.ContainsKey("id") && parameters.ContainsKey("token"):
                    if (int.TryParse(parameters["id"], out userId))
                    {
                        this.ProcessVerifyUserRequest(context, userId, parameters["token"]);
                        return true;
                    }
                    return false;

                case "favicon.ico": //Favicon, server/favicon.ico
                    return true; //Just swallow for now so it doesn't spam the console (should not be necessary though as the pages block the request for the favicon automatically)

                default:
                    if (this.VerifyInternal(context, out userId))
                    {
                        switch (path[0])
                        {
                            case "game" when length == 2 && parameters.Count == 0: //Game-site, syntax: server/game/<GID>
                                if (int.TryParse(path[1], out gameId) && this.Game.Games.ContainsKey(gameId))
                                {
                                    if (this.Game.NeedsPasswordToJoin(gameId, userId))
                                    {
                                        this.ProcessJoinGameSiteRequest(context, gameId);
                                    }
                                    else
                                    {
                                        this.ProcessGameSiteRequest(context, gameId, this.Game.GetUser(userId));
                                    }
                                    return true;
                                }
                                context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/game.*", "/join"));
                                return true;
                            case "game" when length == 2 && parameters.Count == 1 && parameters.ContainsKey("pass"): //Internally used site for joining a game with correct credentials, syntax: server/game/<GID>?pass=<Password>
                                if (int.TryParse(path[1], out gameId) && this.Game.Games.ContainsKey(gameId))
                                {
                                    this.Game.JoinGame(gameId, parameters["pass"], userId);
                                }
                                context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), @"/game/(\d+).*", "/game/$1"));
                                return true;

                            case "join" when length == 2: //Join-game-site (authenticated), syntax: server/join/<GID>[?pass=<Password>]
                                string password = parameters.ContainsKey("pass") ? parameters["pass"] : string.Empty;
                                if (int.TryParse(path[1], out gameId) && this.Game.Games.ContainsKey(gameId))
                                {
                                    if (!this.Game.NeedsPasswordToJoin(gameId, userId) || this.Game.Games[gameId].Password == password)
                                    {
                                        this.Game.JoinGame(gameId, password, userId);
                                        context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/join/(.*)", "/game/$1"));
                                    }
                                    else
                                    {
                                        //context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), @"\?pass(.*)", ""));
                                        this.ProcessJoinGameSiteRequest(context, gameId);
                                    }
                                    return true;
                                }
                                context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/join.*", "/join"));
                                return true;
                            case "refreshgame" when length == 2 && parameters.Count == 0:

                                return true;
                        }
                    }
                    else
                    {
                        switch (path[0])
                        {
                            case "game" when length == 2 && parameters.Count == 0: //Game-site (unauthenticated), syntax: server/game/<GID>
                                if (int.TryParse(path[1], out gameId) && this.Game.Games.ContainsKey(gameId))
                                {
                                    if (this.Game.NeedsPasswordToJoin(gameId, -1))
                                    {
                                        context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/game/(.*)", "/join/$1"));
                                    }
                                    else
                                    {
                                        context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/game/(.*)", "/join/$1?pass="));
                                    }
                                    return true;
                                }
                                context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/game.*", "/join"));
                                return true;
                            case "join" when length == 2: //Join-game-site (unauthenticated), syntax: server/join/<GID>[?pass=<Password>]
                                string password = parameters.ContainsKey("pass") ? parameters["pass"] : string.Empty;
                                if (int.TryParse(path[1], out gameId) && this.Game.Games.ContainsKey(gameId))
                                {
                                    if (this.Game.Games[gameId].Password == password)
                                    {
                                        this.ProcessJoinGameCreateUserSiteRequest(context, gameId, password);
                                        return true;
                                    }
                                }
                                //context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/join.*", "/join"));
                                this.ProcessJoinGameSiteRequest(context, gameId);
                                return true;
                        }
                    }

                    return false;
            }
        }



        protected static internal string DefaultHeader { get; } = @"<link rel=""shortcut icon"" href=""data:image/x-icon;,"" type=""image/x-icon"">
<meta charset='UTF-8'>
<script language=""Javascript"">
function getCookie(name) {
    var value = ""; "" + document.cookie;
    var parts = value.split(""; "" + name + ""="");
    if (parts.length == 2) return parts.pop().split("";"").shift();
}

function verifyCookie() {
    var cookie = getCookie('authenticatedUser');
    if (cookie != null && cookie != """") {
        var parts = cookie.split(""|"");
        localStorage['authenticatedUser'] = JSON.stringify({ id: parts[0], token: parts[1] });
        verify(false);
    } else {
        verify(true);
    }
}

function verify(reload) {
    if (localStorage['authenticatedUser'] !== """") {
        var user = JSON.parse(localStorage['authenticatedUser']);
        var request = new XMLHttpRequest();
        request.onreadystatechange = function() {
            if (request.readyState == 4 && request.status == 200) {
                if (request.responseText != 'ok') {
                    localStorage['authenticatedUser'] = """";
                } else {
                    if (reload && window.location.href.indexOf(""join"") != -1) {
                        location.reload();
                    }
                }
            }
        }
        request.open(""GET"", ""/verify?id="" + user.id + ""&token="" + user.token, true);
        request.send(null);
    }
}

(function () {
    //Verify existing user authentication if present
    verifyCookie();
})();
</script>";

        protected internal void ProcessHomeSiteRequest(HttpListenerContext context)
        {
            Console.WriteLine($"Delivering main page to {context.Request.UserHostAddress}...");
            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online</title>
        {GameServer.DefaultHeader}
    </head>
    <body>
        <h1>Cards Against Humanity Online</h1>
        <p>Welcome to Cards Against Humanity Online - a small webserver which allows you to play the game 'Cards Against Humanity' together with your friends online - have fun!</p>
        <p>
            <button onclick='window.location.href=""join"";'>Join a Game</button>
            <button onclick='window.location.href=""create"";'>Create a Game</button>
        </p>
        <br>
        <p>Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)</p>
        <p>This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
    </body>
</html>";
            context.WriteString(response);
        }

        protected internal void ProcessGameSiteRequest(HttpListenerContext context, int id, User user)
        {
            Console.WriteLine($"Delivering game-page of game #{id} to {context.Request.UserHostAddress}...");

            if (!this.Game.Games[id].Players.Any(p => p.User == user))
            {
                context.Response.Redirect(Regex.Replace(context.Request.Url.ToString(), "/game/(.*)", "/join/$1"));
                return;
            }

            Game game = this.Game.Games[id];
            context.WriteString(GamePageConstructor.ConstructGamePage(game, user));
        }

        protected internal void ProcessJoinGameSiteRequest(HttpListenerContext context)
        {
            Console.WriteLine($"Delivering 'join game'-page to {context.Request.UserHostAddress}...");

            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online - Join Game</title>
        {GameServer.DefaultHeader}
    </head>
    <body>
        <h1>Cards Against Humanity Online - Join Game</h1>
        <p>Click on the name of a game to join it.</p>
        <p>
            {string.Join(Environment.NewLine, this.Game.Games.Values.Select(game => $@"<p><a href='/../join/{game.Id}'>{game.Name}</a> ({game.Players.Count} players)</p>"))}
        </p>
        <br>
        <p>Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)</p>
        <p>This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
    </body>
</html>";
            context.WriteString(response);
        }

        protected internal void ProcessJoinGameSiteRequest(HttpListenerContext context, int id)
        {
            Console.WriteLine($"Delivering 'join game'-password-enter-page to {context.Request.UserHostAddress}...");

            Game game = this.Game.Games[id];
            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online - Join Game {game.Name}</title>
        {GameServer.DefaultHeader}
        <script language='JavaScript'>
            function joinGame() {{
                if (new RegExp(""^([\\w-_]+|)$"").test(passwordbox.value)) {{
                    window.location.href = ""/../join/{id}?pass="" + passwordbox.value;
                }} else {{
                    alert(""The password can only contain alphanumeric characters and dashes."");
                }}
            }}
        </script>
    </head>
    <body onload='checkExistingAuthentication();'>
        <h1>Cards Against Humanity Online - Join Game {game.Name}</h1>
        <p>A password is needed to join this game.</p>
        <p>Password: <input id='passwordbox'></input></p>
        <p><button onclick='joinGame()'>Join Game</button></p>       
        <br>
        <p>Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)</p>
        <p>This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
    </body>
</html>";
            context.WriteString(response);
        }

        protected internal void ProcessJoinGameCreateUserSiteRequest(HttpListenerContext context, int gameId, string password)
        {
            Console.WriteLine($"Delivering user-creation-page to {context.Request.UserHostAddress}...");
            Game game = this.Game.Games[gameId];
            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online - Join Game {game.Name}</title>
        {GameServer.DefaultHeader}
        <script language='JavaScript'>
            function joinGame() {{
                if (new RegExp(""^[\\w-_]+$"").test(namebox.value)) {{
                    if (!(localStorage['authenticatedUser'] === null)) {{
                        var request = new XMLHttpRequest();
                        request.onreadystatechange = function() {{
                            if (request.readyState == 4 && request.status == 200) {{
                                if (request.responseText !== 'username taken') {{
                                    localStorage['authenticatedUser'] = request.responseText;
                                    location.reload();
                                }} else {{
                                    alert(""The username you entered is already used by another user."");
                                }}
                            }}
                        }}
                        request.open(""GET"", ""/createuser?name="" + namebox.value, true);
                        request.send(null);
                    }}
                }} else {{
                    alert(""The username can only contain alphanumeric characters and dashes."");
                }}
            }}
        </script>
    </head>
    <body onload='checkExistingAuthentication();'>
        <h1>Cards Against Humanity Online - Join Game {game.Name}</h1>
        <p>You have to choose an username to enter the game.</p>
        <p>Username: <input id='namebox'></input></p>
        <p><button onclick='joinGame()'>Join Game</button></p>       
        <br>
        <p>Copyright 2016 © Stefan Baumann (<a href='https://github.com/stefan-baumann'>GitHub</a>)</p>
        <p>This web game is based off the card game <a href='https://www.cardsagainsthumanity.com/'>Cards Against Humanity</a> which is available for free under the <a href='https://www.cardsagainsthumanity.com/'>Creative Commons BY-NC-SA 2.0 license</a>.
    </body>
</html>";
            context.WriteString(response);
        }

        protected internal void ProcessCreateGameSiteRequest(HttpListenerContext context)
        {
            Console.WriteLine($"Delivering 'create game'-page to {context.Request.UserHostAddress}...");
            string response = $@"<html>
    <head>
        <title>Cards Against Humanity Online - Create Game</title>
        {GameServer.DefaultHeader}
        <script language='JavaScript'>
            function createGame() {{
                if (new RegExp(""^[\\w_-]+$"").test(namebox.value)) {{
                    if (new RegExp(""^([\\w_-]+|)$"").test(passwordbox.value)) {{
                        var request = new XMLHttpRequest();
                        request.onreadystatechange = function() {{
                            if (request.readyState == 4 && request.status == 200) {{
                                //TODO: Error handling
                                window.location.href = ""/../join/"" + request.responseText + ""?pass="" + passwordbox.value;
                            }}
                        }}
                        request.open(""GET"", ""/create?name="" + namebox.value + ""&pass="" + passwordbox.value, true);
                        request.send(null);
                    }} else {{
                        alert(""The password may only contain alphanumeric characters and dashes."");
                    }}
                }} else {{
                    alert(""The name can only contain alphanumeric characters and dashes."");
                }}
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



        protected internal void ProcessCreateGameRequest(HttpListenerContext context, string name, string password)
        {
            Console.WriteLine($"{context.Request.UserHostAddress} created a new game with name '{name}' and password '{password}'.");

            Game game = this.Game.CreateGame(name, password);

            context.WriteString(game.Id.ToString());
        }

        protected internal void ProcessCreateUserRequest(HttpListenerContext context, string name)
        {
            if (!this.Game.CheckUsernameAvailability(name))
            {
                context.WriteString("username taken");
                return;
            }

            User user = this.Game.CreateUser(name);
            Console.WriteLine($"{context.Request.UserHostAddress} created a new user with name '{user.Name}' and id '{user.Id}'.");
            context.Response.SetCookie(new Cookie("authenticatedUser", $"{user.Id}|{user.Token}", "/"));
            context.WriteString($@"{{""id"":""{user.Id}"",""token"":""{user.Token}""}}");
        }

        protected internal void ProcessChooseCardRequest(HttpListenerContext context, int gameId, int userId, string userToken, int cardIndex)
        {
            Console.WriteLine($"{context.Request.UserHostAddress} chose card #{cardIndex} in game #{gameId}");
            if (this.Game.VerifyUser(userId, userToken))
            {
                User user = this.Game.GetUser(userId);
                if (this.Game.Games.ContainsKey(gameId) && this.Game.Games[gameId].Players.Any(p => p.User == user))
                {
                    Player player = this.Game.Games[gameId].Players.First(p => p.User == user);
                    if (player.ChosenCardIndex < 0)
                    {
                        player.ChosenCardIndex = cardIndex;
                    }
                    context.WriteString("ok");
                }
                else
                {
                    Console.WriteLine("Invalid Game!");
                    context.WriteString("invalid game");
                }
            }
            else
            {
                Console.WriteLine("Verification failed!");
                context.WriteString("invalid credentials");
            }
        }

        protected internal void ProcessVerifyUserRequest(HttpListenerContext context, int id, string token)
        {
            //Console.Write($"{context.Request.UserHostAddress} tries to verify an user with id '{id}' and token '{token}': ");

            bool verified = this.Game.VerifyUser(id, token);
            if (verified)
            {
                context.Response.SetCookie(new Cookie("authenticatedUser", $"{id}|{token}", "/"));
            }
            else
            {
                context.Response.SetCookie(new Cookie("authenticatedUser", ""));
            }

            //Console.WriteLine(verified ? "Ok" : "Invalid credentials");
            context.WriteString(verified ? "ok" : "invalid credentials");
        }

        /*protected internal void ProcessCheckPasswordRequiredRequest(HttpListenerContext context, int gameId, int userId, string token)
        {
            Console.WriteLine($"{context.Request.UserHostAddress} checks whether it needs to enter the password to be able to join game #{gameId}.");
            context.WriteString(this.Game.NeedsPasswordToJoin(gameId, userId) ? "true" : "false");
        }*/

        protected internal bool VerifyInternal(HttpListenerContext context, out int userId)
        {
            string authenticationCookieData = context.Request.Cookies["authenticatedUser"]?.Value;
            if (authenticationCookieData != null && authenticationCookieData.Contains('|'))
            {
                //Console.WriteLine($"Checking authentication-cookie by {context.Request.UserHostAddress}...");
                string[] data = authenticationCookieData.Split('|');
                if (int.TryParse(data[0], out userId))
                {
                    string token = data[1];
                    if (this.Game.VerifyUser(userId, token))
                    {
                        //Console.WriteLine($"Authentication-cookie by {context.Request.UserHostAddress} is valid.");
                        return true;
                    }
                }
            }
            //Console.WriteLine($"Authentication-cookie by {context.Request.UserHostAddress} is invalid.");
            context.Response.SetCookie(new Cookie("authenticatedUser", "", "/"));
            userId = -1;
            return false;
        }
    }
}