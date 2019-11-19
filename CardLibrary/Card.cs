using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CardLibrary
{
    public class Card
    {
        /// <summary>
        /// Converts shorthand card format into the model. An example string input is "5d"
        /// </summary>
        public static Card Parse(string shorthand)
        {
            shorthand = shorthand.Trim();

            if (shorthand.Length != 2)
            {
                throw new ArgumentOutOfRangeException(nameof(shorthand), "The shorthand should be two charcters long");
            }

            var (rankString, suitFirstChar) = (shorthand[0], shorthand[1]);


            var suit = CardService.AllSuits.First(x => x.ToString().ToLower()[0] == suitFirstChar.ToString().ToLower()[0]);


            // Try to parse '2' - '9'
            if (int.TryParse(rankString.ToString(), out var rankParsed) && rankParsed <= 9 && rankParsed >= 2)
            {
                return new Card((Rank)rankParsed, suit);
            }
            
            // Parse the remaining values
            switch (rankString.ToString().ToLower())
            {
                case "t":
                    return new Card(Rank.Ten, suit);
                    break;
                case "j":
                    return new Card(Rank.Jack, suit);
                    break;
                case "q":
                    return new Card(Rank.Queen, suit);
                    break;
                case "k":
                    return new Card(Rank.King, suit);
                    break;
                case "a":
                    return new Card(Rank.Ace, suit);
                    break;
                default:
                    throw new Exception("Could not parse the rank of that card");
                    break;
            }


        }
        public static Card[] ParseMultiple(string shorthandCards)
        {
            var cards = new List<Card>();

            foreach (var cardShorthand in shorthandCards.Split(' '))
            {
                cards.Add(Parse(cardShorthand));
            }

            return cards.ToArray();
        }
        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public Suit Suit { get; }
        public Rank Rank { get; }

        public override string ToString() => GetRankString(Rank) + GetSuitString(Suit);

        private char GetUnicodeChar()
        {
            var stringBuilder = new StringBuilder("1F0");

            switch (Suit)
            {
                case Suit.Hearts:
                    stringBuilder.Append("B");
                    break;
                case Suit.Diamonds:
                    stringBuilder.Append("C");
                    break;
                case Suit.Clubs:
                    stringBuilder.Append("D");
                    break;
                case Suit.Spades:
                    stringBuilder.Append("A");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if ((int) Rank <= 9 && (int) Rank >= 2)
                stringBuilder.Append((int) Rank);
            else
                switch (Rank)
                {
                    case Rank.Ace:
                        stringBuilder.Append("1");
                        break;
                    case Rank.King:
                        stringBuilder.Append("E");
                        break;
                    case Rank.Queen:
                        stringBuilder.Append("D");
                        break;
                    case Rank.Jack:
                        stringBuilder.Append("B");
                        break;
                    case Rank.Ten:
                        stringBuilder.Append("A");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            var intValue = int.Parse(stringBuilder.ToString(), NumberStyles.HexNumber);
            return (char) intValue;
        }

        private static string GetSuitString(Suit suit)
        {
            return suit switch
            {
                Suit.Hearts => "♥",
                Suit.Diamonds => "♦",
                Suit.Clubs => "♣",
                Suit.Spades => "♠",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private static string GetUglySuitString(Suit suit)
        {
            return suit switch
            {
                Suit.Hearts => "H",
                Suit.Diamonds => "D",
                Suit.Clubs => "C",
                Suit.Spades => "S",
                _ => throw new ArgumentOutOfRangeException(nameof(suit)),
            };
        }
        private static string GetRankString(Rank rank)
        {
            return rank switch
            {
                Rank.Ace => "A",
                Rank.King => "K",
                Rank.Queen => "Q",
                Rank.Jack => "J",
                Rank.Ten => "10",
                Rank.Nine => "9",
                Rank.Eight => "8",
                Rank.Seven => "7",
                Rank.Six => "6",
                Rank.Five => "5",
                Rank.Four => "4",
                Rank.Three => "3",
                Rank.Two => "2",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;
            var cardObj = (Card)obj;

            return cardObj.Rank == Rank && cardObj.Suit == Suit;
        }

        public static bool operator ==(Card a, Card b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(Card a, Card b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return int.Parse(((int)Suit).ToString() + ((int)Rank).ToString());
        }
    }
}