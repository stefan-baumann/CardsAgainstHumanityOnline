using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core.Cards
{
    public class WhiteCard
    {
        public WhiteCard(string text)
        {
            this.Text = text;
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return $"White Card [ '{this.Text}' ]";
        }
    }
}
