using System.Collections.Generic;
using Xunit;

namespace PokerLibrary.Tests
{
    public class PlayerTests
    {
        private static List<Player> GenerateSamplePlayers() =>
            new List<Player>
            {
                new Player("Dasham", 10),
                new Player("Neel", 4),
                new Player("Nisitha", 6),
                new Player("Abdullah", 1),
                new Player("Mason", 25),
                new Player("Luke", 12),
                new Player("Tyler", 21),
                new Player("Callum", 17)
            };

        [Fact]
        public void CheckDealerRotation()
        {
            var game = new Game(GenerateSamplePlayers());
            var rotationCount = game.Players.Count;

            for (var i = 0; i < rotationCount; i++)
            {
                var firstDealer = game.CurrentDealer;
                var expectedNextDealer = game.Players[1];

                game.SetupNextRound();

                Assert.NotEqual(firstDealer, game.CurrentDealer);
                Assert.Equal(expectedNextDealer, game.CurrentDealer);
            }

        }
    }
}