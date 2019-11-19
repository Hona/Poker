using System;

namespace CardLibrary
{
    public static class CardService
    {
        public static Suit[] AllSuits => (Suit[])Enum.GetValues(typeof(Suit));
    }
}
