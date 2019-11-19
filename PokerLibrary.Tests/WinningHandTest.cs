using CardLibrary;
using System.Linq;
using Xunit;

namespace PokerLibrary.Tests
{
    public class WinningHandTests
    {

        [Theory]
        [InlineData("Ad Kd Qd Jd Td", "Kd Qd Jd Td 9d")]
        [InlineData("Kd Qd Jd Td 9d", "Qd Jd Td 9d 8d")]
        [InlineData("Qd Jd Td 9d 8d", "Jd Td 9d 8d 7d")]
        [InlineData("Jd Td 9d 8d 7d", "Td 9d 8d 7d 6d")]
        [InlineData("Td 9d 8d 7d 6d", "9d 8d 7d 6d 5d")]
        [InlineData("5d 4d 3d 2d Ad", "2d 3d 4d 5d 7d")]

        public void StraightFlushes_Over_StraightFlushes(string expectedToWinString, string expectedToLoseString)
        {
            Assert.True(WinningHand_Over_LosingHand(expectedToWinString, expectedToLoseString));
        }
        private bool WinningHand_Over_LosingHand(string expectedToWinString, string expectedToLoseString)
        {
            var expectedToWinCards = Card.ParseMultiple(expectedToWinString);
            var expectedToLoseCards = Card.ParseMultiple(expectedToLoseString);

            var handEvaluatorWinner = HandEvaluator.GetWinningCards(new Card[][] { expectedToWinCards, expectedToLoseCards });

            return Enumerable.SequenceEqual(handEvaluatorWinner, expectedToWinCards);
        }
    }
}
