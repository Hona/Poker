using CardLibrary;
using System.Collections.Generic;

namespace PokerLibrary.Networking.Models
{
    public class GameState
    {
        public ClientGamePhase CurrentPhase { get; set; }
        public int StartingMoney { get; set; }
        public List<Card> ShownCommunityCards { get; set; }
        public Hand ClientHand { get; set; }
        public List<KeyValuePair<string, int>> Players { get; set; }
        public int PlayerCurrentSeatPosition { get; set; }
        public KeyValuePair<string, int>[] CurrentPlayerBets { get; set; }
    }
}
