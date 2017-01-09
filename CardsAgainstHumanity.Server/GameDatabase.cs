using CardsAgainstHumanity.Core;
using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Server
{
    public class GameDatabase
    {
        public Dictionary<int, User> Users { get; set; } = new Dictionary<int, User>();
        public Dictionary<int, Game> Games { get; set; } = new Dictionary<int, Game>();

        public bool CheckUsernameAvailability(string name)
        {
            string preprocessed = name.Trim().ToLowerInvariant();
            return !this.Users.Values.Any(user => user.Name.ToLowerInvariant() == preprocessed);
        }

        public bool CheckGameNameAvailability(string name)
        {
            string preprocessed = name.Trim().ToLowerInvariant();
            return !this.Games.Values.Any(game => game.Name.ToLowerInvariant() == preprocessed);
        }

        public bool VerifyUser(int id, string token)
        {
            return this.Users.ContainsKey(id) && this.Users[id].Token == token;
        }



        public User CreateUser(string name)
        {
            if (!this.CheckUsernameAvailability(name))
            {
                throw new InvalidOperationException("It is not possible to create an user with an identical name as an already existing user.");
            }

            int id;
            for (id = this.Users.Count; this.Users.ContainsKey(id); id++) ;

            User user = new User()
            {
                Id = id,
                Token = Guid.NewGuid().ToString(),
                Name = name
            };
            this.Users.Add(id, user);
            return user;
        }

        public User GetUser(int id)
        {
            if(this.Users.ContainsKey(id))
            {
                return this.Users[id];
            }
            return null;
        }

        public Game CreateGame(string name, string password)
        {
            if (!this.CheckGameNameAvailability(name))
            {
                throw new InvalidOperationException("It is not possible to create a game with an identical name as an already existing game.");
            }

            int id;
            for (id = this.Games.Count; this.Games.ContainsKey(id); id++) ;

            Game game = new Game()
            {
                Id = id,
                Name = name,
                Password = password,
                Cards = CardDatabase.InitializeFromSet(CardDatabase.MainSet),
                State = GameState.Inactive
            };

            this.Games.Add(id, game);
            game.UpdateCounter++;
            return game;
        }



        public bool NeedsPasswordToJoin(int gameId, int userId)
        {
            if (!this.Games.ContainsKey(gameId))
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.Games[gameId].Password))
            {
                return false;
            }
            if (this.Games[gameId].Players.Any(player => player.User.Id == userId))
            {
                return false;
            }
            return true;
        }

        public bool JoinGame(int gameId, string password, int userId)
        {
            if (!this.NeedsPasswordToJoin(gameId, userId) || this.Games[gameId].Password == password)
            {
                if (this.Users.ContainsKey(userId))
                {
                    if (this.Games[gameId].Players.Any(player => player.User.Id == userId))
                    {
                        return true;
                    }
                    else
                    {
                        Game game = this.Games[gameId];
                        game.Players.Add(new Player()
                        {
                            User = this.GetUser(userId),
                            Points = 0,
                            WhiteCards = new List<WhiteCard>()
                        });
                        game.UpdateCounter++;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}