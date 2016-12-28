using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class Player
        : Utils.EquatableBase<Player>
    {
        public User User { get; set; }

        public int Points { get; set; }

        public List<WhiteCard> WhiteCards { get; set; } 

        public int ChosenCardIndex { get; set; }

        protected override bool IsEqualTo(Player other)
        {
            return this.User == other.User && this.Points == other.Points && this.ChosenCardIndex == other.ChosenCardIndex && this.WhiteCards.SequenceEqual(other.WhiteCards);
        }
    }
}
