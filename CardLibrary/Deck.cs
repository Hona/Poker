using System;
using System.Collections.Generic;

namespace CardLibrary
{
    public class Deck
    {
        public Stack<Card> Cards { get; private set; }

        public static Deck GetFullDeck()
        {
            var cardsOutput = new Stack<Card>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                cardsOutput.Push(new Card(rank, suit));

            return new Deck {Cards = cardsOutput};
        }

        public static Deck NewShuffled()
        {
            var fullDeck = GetFullDeck();
            fullDeck.Cards = fullDeck.Cards.Shuffle();
            return fullDeck;
        }
        public void Shuffle()
        {
            Cards = Cards.Shuffle();
        }
        public bool TryGetTopCard(out Card card) => Cards.TryPop(out card);
    }
}