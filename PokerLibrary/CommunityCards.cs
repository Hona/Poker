using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CardLibrary;

namespace PokerLibrary
{
    public class CommunityCards : IEnumerable<Card>
    {
        public List<Card> BurntCards { get; } = new List<Card>(5);
        public List<Card> ShownCards { get; } = new List<Card>(5);
        [JsonIgnore]
        public Card this[int i]
        {
            get => ShownCards[i];
            set => ShownCards[i] = value;
        }

        IEnumerator IEnumerable.GetEnumerator() => ShownCards.GetEnumerator();

        public IEnumerator<Card> GetEnumerator() => ShownCards.GetEnumerator();
    }
}
