using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class Player
    {
        public User User { get; set; }

        public int Points { get; set; }

        public List<WhiteCard> WhiteCards { get; set; } 
    }
}
