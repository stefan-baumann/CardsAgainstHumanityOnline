using CardsAgainstHumanity.Core.Cards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core
{
    public class RoundResult
    {
        public Player Winner { get; set; }
        public WhiteCard WinningCard { get; set; }

        public BlackCard BlackCard { get; set; }
        public List<WhiteCard> WhiteCards { get; set; }
        public Player Judge { get; set; }
    }
}
