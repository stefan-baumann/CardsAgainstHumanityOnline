using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardsAgainstHumanity.Core.Cards
{
    public class CardDatabase
    {
        public static CardSet MainSet { get; } = new MainSet();

        public static CardDatabase InitializeFromSet(CardSet set)
        {
            Random random = new Random();
            CardDatabase result = new CardDatabase()
            {
                BlackCards = new Queue<BlackCard>(set.GetBlackCards().OrderBy(c => random.Next())),
                WhiteCards = new Queue<WhiteCard>(set.GetWhiteCards().OrderBy(c => random.Next()))
            };
            return result;
        }



        public Queue<BlackCard> BlackCards { get; internal protected set; }
        public Queue<WhiteCard> WhiteCards { get; internal protected set; }



        public BlackCard GetBlackCard()
        {
            return this.BlackCards.Dequeue();
        }

        public WhiteCard GetWhiteCard()
        {
            return this.WhiteCards.Dequeue();
        }

        public void RecycleBlackCard(BlackCard card)
        {
            this.BlackCards.Enqueue(card);
        }

        public void RecycleWhiteCard(WhiteCard card)
        {
            this.WhiteCards.Enqueue(card);
        }
    }
}
