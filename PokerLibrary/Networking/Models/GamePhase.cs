namespace PokerLibrary.Networking.Models
{
    public enum ClientGamePhase
    {
        Error = 0,
        GatherPlayers = 1,
        ConfigureGame = 2,
        DealCardsAndInfo = 3,
        PreflopBetting = 4,
        ShowFlop = 5,
        FlopBetting = 6,
        ShowTurn = 7,
        TurnBetting = 8,
        ShowRiver = 9,
        RiverBetting = 11,
        RoundResults = 12,
    }
}
