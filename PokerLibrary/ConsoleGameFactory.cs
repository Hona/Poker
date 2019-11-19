using System;
using System.Collections.Generic;

namespace PokerLibrary
{
    public static class ConsoleGameFactory
    {
        public static Game StartNewGame()
        {
            Console.WriteLine("*** Starting New Game ***");
            throw new Exception("obsolete");
            //var startingMoney = GenerateStartingMoney();

            //var players = GeneratePlayers(startingMoney);

            //while (true)
            //{
            //    var game = new Game(players);

            //    game.DealAllPlayers();

            //    var currentBets = game.PostBlinds();
            //    currentBets = game.Flop(currentBets);
            //    currentBets = game.Turn(currentBets);
            //    currentBets = game.River(currentBets);

            //    var winner = HandEvaluator.GetWinner(players, game.ShownCommunityCards);
            //    winner.Key.GiveMoney(game.Pot);

            //    Console.WriteLine($"{winner.Key.Name} won ${game.Pot} with {winner.Value.HandType} | {string.Join<Card>(' ', winner.Value.Cards)}");
            //    Console.ReadLine();

            //    return game;
            //}

            
        }
        private static int GenerateStartingMoney()
        {
            Console.Write("Enter the starting money: $");
            var startingMoney = -1;
            do
            {
                var startingMoneyString = Console.ReadLine();
                if (!int.TryParse(startingMoneyString, out startingMoney))
                    Console.Write("Invalid input, Enter the starting money: ");
            } while (startingMoney <= 0);

            Console.WriteLine();
            return startingMoney;
        }
        private static List<Player> GeneratePlayers(int startingMoney)
        {
            Console.Clear();

            Console.Write("Create all the players. Enter the number of players: ");

            var playerCount = -1;

            do
            {
                var playerCountString = Console.ReadLine();
                if (!int.TryParse(playerCountString, out playerCount))
                    Console.Write("Invalid input, Enter the number of players: ");
            } while (playerCount <= 1);

            Console.WriteLine();

            var playerList = new List<Player>();
            Console.WriteLine("Creating Individual Players");
            for (var i = 0; i < playerCount; i++)
            {
                Console.Write($"Enter player #{i}'s name: ");
                var name = Console.ReadLine();

                Console.WriteLine($"Enter player #{i}'s password: ");
                var password = Console.ReadLine();

                var player = new Player(name, startingMoney, password);
                playerList.Add(player);
            }

            Console.WriteLine();
            return playerList;
        }
    }
}