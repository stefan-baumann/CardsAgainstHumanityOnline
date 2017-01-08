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

        public Player Judge { get; set; }

        public BlackCard CurrentBlackCard { get; set; }

        public CardDatabase Cards { get; set; }

        public void RefreshState()
        {

        }

        public void FinishRound(Player winner)
        {
            winner.Points += 1;
            this.FinishRound();
        }

        public void FinishRound()
        {
            this.State = GameState.PlayingCards;
            this.Cards.RecycleBlackCard(this.CurrentBlackCard);
            this.CurrentBlackCard = this.Cards.GetBlackCard();
            foreach(Player player in this.Players)
            {
                if (player.ChosenCardIndex > -1)
                {
                    this.Cards.RecycleWhiteCard(player.WhiteCards[player.ChosenCardIndex]);
                    player.WhiteCards.RemoveAt(player.ChosenCardIndex);
                }

                while (player.WhiteCards.Count < 10)
                {
                    player.WhiteCards.Add(this.Cards.GetWhiteCard());
                }
            }

            this.Judge = this.Players[(this.Players.IndexOf(this.Judge) + 1) % this.Players.Count];
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
