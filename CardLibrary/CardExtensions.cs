using System;
using System.Collections.Generic;
using System.Linq;

namespace CardLibrary
{
    public static class CardExtensions
    {
        public static void FormattedWrite(this Card card, ConsoleColor resetColor)
        {
            if (card.Suit == Suit.Diamonds || card.Suit == Suit.Hearts)
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.Black;
            Console.Write(card);

            Console.ForegroundColor = resetColor;
        }
        public static Card GetHighestCard(this IEnumerable<Card> cards)
        {
            var value = (int)cards.Select(x => x.Rank).OrderByDescending(x => (int)x).First();
            return cards.First(x => (int)x.Rank == value);
        }
    }
}
