using CardLibrary;

namespace PokerLibrary
{
    public class Player
    {
        public Player(string name, int money, string password = "")
        {
            Name = name;
            Money = money;
            Password = password;
            Hand = new Hand();
        }

        public string Name { get; }
        public Hand Hand { get; private set; }
        public int Money { get; private set; }
        private string Password { get; }

        public void GiveCard(Card card)
        {
            Hand.Add(card);
        }

        public void SubtractBet(int bet)
        {
            Money -= bet;
        }

        public bool IsCorrectPassword(string password) => password == Password;

        internal void GiveMoney(int value)
        {
            Money += value;
        }
        internal void ResetHand()
        {
            Hand = new Hand();
        }
    }
}