using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class Game
        : Utils.EquatableBase<Game>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }

        public List<Player> Players { get; set; } = new List<Player>();

        public GameState State { get; set; } = GameState.Inactive;

        protected int JudgeIndex { get; set; } = 0;
        public Player Judge { get => this.Players[this.JudgeIndex % this.Players.Count]; }

        public BlackCard CurrentBlackCard { get; set; }

        public Dictionary<Player, WhiteCard> PlayedWhiteCards { get; set; } = new Dictionary<Player, WhiteCard>();
        
        public Player RoundWinner { get; set; }

        public Stack<RoundResult> LastRounds { get; set; } = new Stack<RoundResult>();

        public CardDatabase Cards { get; set; }

        private object stateLockObject = new object();
        public void RefreshState()
        {
            lock (this.stateLockObject)
            {
                //Check whether eough players are there
                if (this.Players.Count < 3)
                {
                    this.State = GameState.Inactive;
                    if (this.CurrentBlackCard != null)
                    {
                        this.Cards.Recycle(this.CurrentBlackCard);
                        this.CurrentBlackCard = null;
                    }
                }

                //Remove played cards of players who left
                foreach (var playedCard in this.PlayedWhiteCards)
                {
                    if (!this.Players.Contains(playedCard.Key))
                    {
                        this.PlayedWhiteCards.Remove(playedCard.Key);
                    }
                }
                
                switch (this.State)
                {
                    case GameState.Inactive:
                        if (this.Players.Count >= 3)
                        {
                            //Enough players, start round

                            if (this.CurrentBlackCard == null)
                            {
                                //Select new black card
                                this.CurrentBlackCard = this.Cards.DrawBlackCard();
                            }
                            foreach (Player player in this.Players)
                            {
                                //Fill up the cards of each player
                                for (; player.WhiteCards.Count < 10; player.WhiteCards.Add(this.Cards.DrawWhiteCard())) ;
                            }

                            this.State = GameState.PlayingCards;
                            this.RefreshState();
                        }
                        return;

                    case GameState.PlayingCards:
                        if (this.Players.All(player => this.Judge == player || this.PlayedWhiteCards.ContainsKey(player)))
                        {
                            //All players have selected a card, start judging

                            this.RoundWinner = null;

                            this.State = GameState.Judging;
                            this.RefreshState();
                        }
                        return;

                    case GameState.Judging:
                        if (this.RoundWinner != null)
                        {
                            //Judging has ended, refresh scoreboard and return to game
                            this.LastRounds.Push(new RoundResult()
                            {
                                Winner = this.RoundWinner,
                                WinningCard = this.PlayedWhiteCards[this.RoundWinner],
                                BlackCard = this.CurrentBlackCard,
                                WhiteCards = this.PlayedWhiteCards.Values.ToList(),
                                Judge = this.Judge
                            });

                            this.RoundWinner.Points++;
                            this.RoundWinner = null;
                            this.Cards.Recycle(this.CurrentBlackCard);
                            this.CurrentBlackCard = this.Cards.DrawBlackCard();
                            this.JudgeIndex++;
                            foreach(WhiteCard card in this.PlayedWhiteCards.Values)
                            {
                                this.Cards.Recycle(card);
                            }
                            this.PlayedWhiteCards.Clear();

                            this.State = GameState.PlayingCards;
                            this.RefreshState();
                        }

                        return;
                }
            }
        }



        public void ExecuteGameCommand(Player player, string command, Dictionary<string, string> parameters)
        {
            int index;
            switch (command)
            {
                case "playcard" when parameters.Count == 1 && parameters.ContainsKey("index"):
                    if (int.TryParse(parameters["index"], out index) && index < player.WhiteCards.Count)
                    {
                        if (!this.PlayedWhiteCards.ContainsKey(player))
                        {
                            Console.WriteLine($"[{this.Name} / #{this.Id}] {player.User.Name} played card #{index}.");

                            WhiteCard card = player.WhiteCards[index];
                            this.PlayedWhiteCards.Add(player, card);
                            player.WhiteCards.Remove(card);
                        }
                        else
                        {
                            Console.WriteLine($"[{this.Name} / #{this.Id}] {player.User.Name} tried playing a card although he has already done so.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{this.Name} / #{this.Id}] {player.User.Name} tried playing a card with an invalid index ({index}).");
                    }
                    return;
                
                case "judge" when parameters.Count == 1 && parameters.ContainsKey("index"):
                    if (int.TryParse(parameters["index"], out index) && index < this.PlayedWhiteCards.Count)
                    {
                        if (this.Judge == player)
                        {
                            if (this.RoundWinner == null)
                            {
                                Console.WriteLine($"[{this.Name} / #{this.Id}] Judge {player.User.Name} chose card #{index}.");
                                this.RoundWinner = this.PlayedWhiteCards.Keys.ElementAt(index);
                            }
                            else
                            {
                                Console.WriteLine($"[{this.Name} / #{this.Id}] {player.User.Name} tried judging although he has already chosen.");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[{this.Name} / #{this.Id}] {player.User.Name} tried judging although he is not a judge.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[{this.Name} / #{this.Id}] {player.User.Name} tried judging a card with an invalid index ({index}).");
                    }
                    return;
            }
        }



        protected override bool IsEqualTo(Game other)
        {
            return this.Id == other.Id && this.Name == other.Name && this.Password == other.Password && this.Judge == other.Judge && this.State == other.State && this.Players.SequenceEqual(other.Players);
        }
    }

    public enum GameState
    {
        Inactive,
        PlayingCards,
        Judging
    }
}
