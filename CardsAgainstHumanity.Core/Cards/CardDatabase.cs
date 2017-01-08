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



        public BlackCard DrawBlackCard()
        {
            return this.BlackCards.Dequeue();
        }

        public WhiteCard DrawWhiteCard()
        {
            return this.WhiteCards.Dequeue();
        }

        public void Recycle(BlackCard card)
        {
            this.BlackCards.Enqueue(card);
        }

        public void Recycle(WhiteCard card)
        {
            this.WhiteCards.Enqueue(card);
        }
    }
}
