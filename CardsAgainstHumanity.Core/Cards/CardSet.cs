using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core.Cards
{
    public abstract class CardSet
    {
        protected abstract Dictionary<string, int> BlackCards { get; }
        protected abstract List<string> WhiteCards { get; }

        public IEnumerable<WhiteCard> GetWhiteCards()
        {
            foreach (string text in this.WhiteCards)
            {
                yield return new WhiteCard(text);
            }

            yield break;
        }

        public IEnumerable<BlackCard> GetBlackCards()
        {
            foreach (KeyValuePair<string, int> card in this.BlackCards)
            {
                yield return new BlackCard(card.Key, card.Value);
            }

            yield break;
        }
    }
}
