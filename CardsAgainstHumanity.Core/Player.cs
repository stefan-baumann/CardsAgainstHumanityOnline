using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class Player
    {
        public User User { get; private set; }

        public List<WhiteCard> WhiteCards { get; } = new List<WhiteCard>();
    }
}
