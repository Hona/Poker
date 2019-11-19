using System;
using System.Collections.Generic;
using System.Linq;
using CardLibrary;

namespace PokerLibrary
{
    public static class HandEvaluator
    {
        public static HandRank GetHandRank(Hand hand, List<Card> communityCards)
        {
            var totalCards = new List<Card>();
            totalCards.AddRange(hand);
            totalCards.AddRange(communityCards);

            return GetHandRank(totalCards);
        }
        public static HandRank GetHandRank(List<Card> totalCards)
        {
            if (IsRoyalFlush(totalCards, out var usedCards))
            {
                return new HandRank 
                {
                    Cards = usedCards.ToArray(),
                    HandType = HandType.StraightFlush,
                    HandValue = (int)usedCards.GetHighestCard().Rank
                };
            }
            else if (IsStraightFlush(totalCards, out usedCards))
            {
                return new HandRank
                {
                    Cards = usedCards.ToArray(),
                    HandType = HandType.StraightFlush,
                    HandValue = (int)usedCards.GetHighestCard().Rank
                };
            }
            else if (IsFourOfAKind(totalCards, out usedCards))
            {
                var handCards = new List<Card>();
                handCards.AddRange(usedCards);
                var remainingHighCard = totalCards.Where(x => !usedCards.Contains(x)).GetHighestCard();
                handCards.Add(remainingHighCard);

                // Cube the quad cards as quad aces with a douce, verses quad kings ace high would be incorrect
                var quadsAloneValue = usedCards.Select(x => (int)x.Rank^3).Sum();
                var highCardValue = (int)remainingHighCard.Rank;

                return new HandRank
                {
                    Cards = handCards.ToArray(),
                    HandType = HandType.Quads,
                    HandValue = quadsAloneValue + highCardValue
                };
            }
            else if (IsFullHouse(totalCards, out usedCards))
            {
                var tripsValue = usedCards.Select(x => x.Rank).GroupBy(x => x).First(x => x.Count() == 3).Select(x => (int)x^3).Sum();
                var pairValue = usedCards.Select(x => x.Rank).GroupBy(x => x).First(x => x.Count() == 2).Select(x => (int)x).Sum();

                return new HandRank
                {
                    Cards = usedCards.ToArray(),
                    HandType = HandType.FullHouse,
                    HandValue = tripsValue + pairValue
                };
            }
            else if (IsFlush(totalCards, out usedCards))
            {
                return new HandRank
                {
                    Cards = usedCards.ToArray(),
                    HandType = HandType.Flush,
                    HandValue = usedCards.Select(x => (int)x.Rank).Sum()
                };
            }
            else if (IsStraight(totalCards, out usedCards))
            {
                return new HandRank
                {
                    Cards = usedCards.ToArray(),
                    HandType = HandType.Straight,
                    HandValue = (int)usedCards.GetHighestCard().Suit
                };
            }
            else if (IsThreeOfAKind(totalCards, out usedCards))
            {
                var handCards = new List<Card>();
                handCards.AddRange(usedCards);

                var remainingTwoHighCards = totalCards.Where(x => !usedCards.Contains(x)).OrderByDescending(x => x.Rank).Take(2);
                handCards.AddRange(remainingTwoHighCards);

                var tripsValue = ((int)usedCards[0].Rank ^ 3) * 3;
                var remainingTwoHighCardsRanks = remainingTwoHighCards.Select(x => (int)x.Rank).OrderByDescending(x => x).ToArray();
                var remainingTwoHighCardsValue = (remainingTwoHighCardsRanks[0] ^ 2) + remainingTwoHighCardsRanks[1];

                return new HandRank
                {
                    Cards = handCards.ToArray(),
                    HandType = HandType.Trips,
                    HandValue = tripsValue + remainingTwoHighCardsValue
                };
            }
            else if (IsTwoPair(totalCards, out usedCards))
            {
                var remainingHighCard = totalCards.Where(x => !usedCards.Contains(x)).GetHighestCard();

                var highPairValue = ((int)usedCards.OrderByDescending(x => x.Rank).First().Rank ^ 7) * 2;
                var lowPairValue = ((int)usedCards.OrderByDescending(x => x.Rank).First().Rank ^ 3) * 2;
                var remainingHighCardValue = (int)remainingHighCard.Rank;

                var handCards = new List<Card>();
                handCards.AddRange(usedCards);
                handCards.Add(remainingHighCard);

                return new HandRank
                {
                    Cards = handCards.ToArray(),
                    HandType = HandType.TwoPair,
                    HandValue = highPairValue + lowPairValue + remainingHighCardValue
                };
            }
            else if (IsPair(totalCards, out usedCards))
            {
                var handCards = new List<Card>();
                handCards.AddRange(usedCards);
                var remainingHighCards = totalCards.Where(x => !usedCards.Contains(x)).OrderByDescending(x => x.Rank).Take(3).Reverse().ToArray();
                handCards.AddRange(remainingHighCards);
                var pairValue = ((int)usedCards.First().Rank ^ 4) * 2;

                int highCardValues = 0;

                for (int i = 0; i < remainingHighCards.Length; i++)
                {
                    highCardValues += ((int)remainingHighCards[i].Rank ^ i) + 1;
                }

                return new HandRank
                {
                    Cards = handCards.ToArray(),
                    HandType = HandType.OnePair,
                    HandValue = pairValue + highCardValues
                };
            }
            else if (IsHighCard(totalCards, out usedCards))
            {
                var handCards = new List<Card>();
                handCards.AddRange(usedCards);
                
                var remainingHighCards = totalCards.Where(x => !usedCards.Contains(x)).OrderByDescending(x => x.Rank).Take(4).Reverse().ToArray();
                handCards.AddRange(remainingHighCards);

                handCards = handCards.OrderBy(x => x.Rank).ToList();

                int highCardValues = 0;
                for (int i = 0; i < handCards.Count; i++)
                {
                    highCardValues += ((int)handCards[i].Rank ^ i) + 1;
                }

                return new HandRank
                {
                    Cards = handCards.ToArray(),
                    HandType = HandType.HighCard,
                    HandValue = highCardValues
                };
            }
            else
            {
                throw new Exception("Something is very wrong... You somehow have no cards?");
            }
        }
        public static KeyValuePair<Player, HandRank> GetWinner(List<Player> players, List<Card> communityCards)
        {
            var playerHandRanks = new Dictionary<Player, HandRank>();

            foreach (var player in players)
            {
                playerHandRanks.Add(player, GetHandRank(player.Hand, communityCards));
            }

            var winningHandRank = GetWinningHandRank(playerHandRanks.Values.ToList());

            var winner = playerHandRanks.FirstOrDefault(x => x.Value == winningHandRank);
            if (winner.Key != null && winner.Value != null)
            {
                return winner;
            }
            throw new Exception("There had to be a winner here...");
        }

        private static HandRank GetWinningHandRank(List<HandRank> handRanks)
        {
            for (int i = (int)HandType.StraightFlush; i > (int)HandType.HighCard; i--)
            {
                var currentHandTypeCount = handRanks.Count(x => x.HandType == (HandType)i);
                if (currentHandTypeCount == 0)
                {
                    continue;
                }
                else if (currentHandTypeCount == 1)
                {
                    return handRanks.First(x => x.HandType == (HandType)i);
                }
                else
                {
                    var hands = handRanks.Where(x => x.HandType == (HandType)i);
                    return handRanks.Aggregate((l, r) => l.HandValue > r.HandValue ? l : r);
                }
            }

            throw new Exception("There had to be a winner here...");
        }
        public static List<Card> GetWinningCards(IEnumerable<IEnumerable<Card>> handArray) 
        {
            var handRanks = handArray.Select(x => GetHandRank(x.ToList())).ToList();
            var winningHandRank = GetWinningHandRank(handRanks);
            return handArray.First(x => Enumerable.SequenceEqual(x.ToArray(), winningHandRank.Cards)).ToList();
        }
        private static bool IsRoyalFlush(List<Card> totalCards, out List<Card> usedCards)
        {
            if (IsStraightFlush(totalCards, out var usedStraightFlushCards))
            {
                if (usedStraightFlushCards.GetHighestCard().Rank == Rank.Ace)
                {
                    usedCards = usedStraightFlushCards;
                    return true;
                }
            }

            usedCards = null;
            return false;
        }

        private static bool IsStraightFlush(List<Card> totalCards, out List<Card> usedCards)
        {
            // TODO: This is incorrect, as it will get the best straight, and the best flush - what happens when its 2-6 straight with ace high flush?
            if (IsStraight(totalCards, out var usedStraightCards) && IsFlush(totalCards, out var usedFlushCards))
            {
                if (usedStraightCards.SequenceEqual(usedFlushCards))
                {
                    usedCards = usedStraightCards;
                    return true;
                }
            }

            usedCards = null;
            return false;
        }

        private static bool IsFourOfAKind(List<Card> totalCards, out List<Card> usedCards)
        {
            var groupedCardRanks = totalCards.Select(x => x.Rank).GroupBy(x => x);
            if (groupedCardRanks.Any(x => x.Count() == 4))
            {
                usedCards = totalCards.Where(x => x.Rank == groupedCardRanks.First(z => z.Count() == 4).First()).ToList();
                return true;
            }

            usedCards = null;
            return false;
        }

        private static bool IsFullHouse(List<Card> totalCards, out List<Card> usedCards)
        {
            if (IsThreeOfAKind(totalCards, out var usedTripsCards))
            {
                if (IsPair(totalCards.Where(x => !usedTripsCards.Contains(x)).ToList(), out var usedPairCards))
                {
                    usedTripsCards.AddRange(usedPairCards);
                    usedCards = usedTripsCards;
                }
            }

            usedCards = null;
            return false;
        }

        private static bool IsFlush(List<Card> totalCards, out List<Card> usedCards)
        {
            var groupedSuits = totalCards.Select(x => x.Suit).GroupBy(x => x);
            if (groupedSuits.Any(x => x.Count() >= 5))
            {
                var suit = groupedSuits.First(x => x.Count() >= 5).First();
                var allFlushSuitedCards = totalCards.Where(x => x.Suit == suit).OrderByDescending(x => x.Rank);
                usedCards = allFlushSuitedCards.Take(5).ToList();
                return true;
            }

            usedCards = null;
            return false;
        }

        private static bool IsStraight(List<Card> totalCards, out List<Card> usedCards)
        {
            var orderedCards = totalCards.OrderBy(x => (int)x.Rank).ToList();
            foreach (var card in orderedCards)
            {
                var lowerBound = (int)card.Rank;
                var upperBound = lowerBound + 5;
                var orderedCardsLowerBoundIndex = orderedCards.IndexOf(card);

                if (!Enum.IsDefined(typeof(Rank), upperBound))
                {
                    // Any card to iterate past this will be increasingly out of bounds
                    break;
                }

                var isStraight = true;

                for (int i = lowerBound; i < upperBound - 1; i++)
                {

                    var arrayUpperBound = orderedCardsLowerBoundIndex + 4;
                    if (arrayUpperBound >= orderedCards.Count)
                    {
                        isStraight = false;
                        break;
                    }

                    if (orderedCards[orderedCardsLowerBoundIndex + (i - lowerBound)].Rank + 1 != orderedCards[orderedCardsLowerBoundIndex + (i - lowerBound) + 1].Rank)
                    {
                        if (card.Rank == Rank.Two && i == upperBound && orderedCards.Any(x => x.Rank == Rank.Ace))
                        {
                            continue;
                        }
                        isStraight = false;
                        break;
                    }


                }

                if (isStraight)
                {
                    // Prefer the same suit
                    if (orderedCards.Skip(orderedCardsLowerBoundIndex).ToArray()[0].Rank == Rank.Two)
                    {
                        usedCards = orderedCards.Skip(orderedCardsLowerBoundIndex).Take(4).ToList();
                        usedCards.Add(orderedCards.First(x => x.Rank == Rank.Ace));

                    }
                    usedCards = orderedCards.Skip(orderedCardsLowerBoundIndex).Take(5).ToList();
                    return true;
                }
            }

            usedCards = null;
            return false;
        }

        private static bool IsThreeOfAKind(List<Card> totalCards, out List<Card> usedCards)
        {
            var groupedCardRanks = totalCards.Select(x => x.Rank).GroupBy(x => x).OrderByDescending(x => (int)x.First());
            if (groupedCardRanks.Any(x => x.Count() == 3))
            {
                usedCards = totalCards.Where(x => x.Rank == groupedCardRanks.First(z => z.Count() == 3).First()).ToList();
                return true;
            }

            usedCards = null;
            return false;
        }

        private static bool IsTwoPair(List<Card> totalCards, out List<Card> usedCards)
        {
            if (IsPair(totalCards, out var firstPairCards))
            {
                if (IsPair(totalCards.Where(x => !firstPairCards.Contains(x)).ToList(), out var secondPairCards))
                {
                    firstPairCards.AddRange(secondPairCards);
                    usedCards = firstPairCards;
                    return true;
                }
            }

            usedCards = null;
            return false;
        }

        private static bool IsPair(List<Card> totalCards, out List<Card> usedCards)
        {
            var groupedCardRanks = totalCards.Select(x => x.Rank).GroupBy(x => x).OrderByDescending(x => (int)x.First());
            if (groupedCardRanks.Any(x => x.Count() == 2))
            {
                usedCards = totalCards.Where(x => x.Rank == groupedCardRanks.First(z => z.Count() == 2).First()).ToList();
                return true;
            }

            usedCards = null;
            return false;
        }
        private static bool IsHighCard(List<Card> totalCards, out List<Card> usedCards)
        {
            var groupedCardRanks = totalCards.Select(x => x.Rank).GroupBy(x => x).OrderByDescending(x => (int)x.First());
            if (groupedCardRanks.Any(x => x.Count() == 1))
            {
                usedCards = totalCards.Where(x => x.Rank == groupedCardRanks.First(z => z.Count() == 1).First()).ToList();
                return true;
            }

            usedCards = null;
            return false;
        }
    }
}
