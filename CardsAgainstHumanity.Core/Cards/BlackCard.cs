using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core.Cards
{
    public class BlackCard
    {
        public BlackCard(string text, int answerCount)
        {
            this.Text = text;
            this.AnswerCount = answerCount;
        }

        public string Text { get; set; }
        public int AnswerCount { get; set; }

        public override string ToString()
        {
            return $"Black Card [ '{this.Text}' ]";
        }
    }
}
