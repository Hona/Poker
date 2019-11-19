using CardLibrary;

namespace PokerLibrary
{
    public class HandRank
    {
        // HandType quicky distinguishes between hands such as RoyalFlush vs High Card
        // HandValue determines which hand is better inside of a hand type such as King high straight vs Queen high straight
        public HandType HandType { get; set; }
        public int HandValue { get; set; }
        public Card[] Cards { get; set; }
    }
}
