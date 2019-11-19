using System.Collections;
using System.Collections.Generic;
using CardLibrary;
using Newtonsoft.Json;

namespace PokerLibrary
{
    public class Hand : IEnumerable<Card>
    {
        [JsonProperty]
        public List<Card> Cards { get; set; } = new List<Card>();
        [JsonIgnore]
        public Card this[int i]
        {
            get => Cards[i];
            set => Cards[i] = value;
        }
        IEnumerator IEnumerable.GetEnumerator() => Cards.GetEnumerator();
        public IEnumerator<Card> GetEnumerator() => Cards.GetEnumerator();
        internal void Add(Card card)
        {
            Cards.Add(card);
        }
        public override string ToString() => string.Join(", ", Cards);
        [JsonConstructor]
        public Hand(List<Card> cards)
        {
            Cards = cards;
        }
        public Hand()
        {

        }

    }
}