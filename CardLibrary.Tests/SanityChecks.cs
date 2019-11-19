using Xunit;

namespace CardLibrary.Tests
{
    public class SanityChecks
    {
        [Fact]
        public void GetFullDeck_Count_Is52()
        {
            var fullDeck = Deck.GetFullDeck();
            var cardCount = fullDeck.Cards.Count;
            var expected = 52;
            Assert.Equal(expected, cardCount);
        }

        [Fact]
        public void GetEmptyDeck_TryDrawCard_IsFalse()
        {
            var fullDeck = Deck.NewShuffled();
            for (int i = 0; i < 52; i++)
            {
                fullDeck.TryGetTopCard(out _);

            }
            var result = fullDeck.TryGetTopCard(out _);
            Assert.False(result);
        }
    }
}