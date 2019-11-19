using System;
using System.Collections.Generic;
using System.Linq;
using CardLibrary;

namespace PokerLibrary
{
    public class Game
    {
        private readonly Deck _deck;
        private CommunityCards _communityCards = new CommunityCards();
        public List<Card> ShownCommunityCards => _communityCards.ShownCards;
        public int Pot { get; set; }
        public Game(List<Player> players)
        {
            Players = players;
            _deck = Deck.NewShuffled();
        }

        public Player CurrentDealer => Players[0];
        public List<Player> Players { get; }
        private List<Player> PlayersWithMoney => Players.Where(x => x.Money > 0).ToList();
        public List<Player> ActivePlayers { get; set; }

        public void DealAllPlayers()
        {
            // i = 1 to start left of dealer
            for (var i = 1; i <= PlayersWithMoney.Count * 2; i++)
            {
                if (!_deck.TryGetTopCard(out var card)) throw new Exception("No cards left");
                PlayersWithMoney[i % PlayersWithMoney.Count].GiveCard(card);
            }
            ActivePlayers = PlayersWithMoney;
        }

        /// <summary>
        /// Rotate dealer, shuffle deck, and reset community cards
        /// </summary>
        public void SetupNextRound()
        {
            var lastElement = Players[0];
            for (var i = 0; i < Players.Count; i++)
                if (i < Players.Count - 1)
                    Players[i] = Players[i + 1];
            Players[Players.Count - 1] = lastElement;
            ActivePlayers = PlayersWithMoney;

            _deck.Shuffle();
        }

        public Dictionary<Player, int> PostBlinds()
        {
            var bets = new Dictionary<Player, int>();

            // Small blind
            bets.Add(PlayersWithMoney[1], 1);
            PlayersWithMoney[1].SubtractBet(1);

            if (PlayersWithMoney.Count != 2)
            {
                // Big blind
                bets.Add(PlayersWithMoney[2], 2);
                PlayersWithMoney[2].SubtractBet(2);
            }
            else
            {
                bets.Add(PlayersWithMoney[0], 2);
                PlayersWithMoney[0].SubtractBet(2);
            }

            return bets;
        }

        public void Flop()
        {
            BurnOne();
            ShowThree();
        }
        public void Turn()
        {
            BurnOne();
            ShowOne();
        }

        public void River() =>
            Turn();
        private void BurnOne()
        {
            if (!_deck.TryGetTopCard(out var topCard)) return;
            _communityCards.BurntCards.Add(topCard);
        }

        private void ShowOne()
        {
            if (!_deck.TryGetTopCard(out var topCard)) return;
            _communityCards.ShownCards.Add(topCard);
        }
        private void ShowThree()
        {
            for (var i = 0; i < 3; i++)
            {
                ShowOne();
            }
        }
        public string GetWinnerText(List<Player> players)
        {
            var winner = HandEvaluator.GetWinner(players, ShownCommunityCards);
            var output = $"{winner.Key.Name} won ${Pot} with {winner.Value.HandType} | {string.Join<Card>(' ', winner.Value.Cards)}";

            winner.Key.GiveMoney(Pot);
            Pot = 0;


            // TODO: Move this to where the community cards are reset
            foreach (var player in Players)
            {
                player.ResetHand();
            }

            return output;
        }
    }
}
