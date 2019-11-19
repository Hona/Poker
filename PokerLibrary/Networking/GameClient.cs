using PokerLibrary.Networking.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PokerLibrary.Networking
{
    public class GameClient
    {
        private TcpClient _tcpClient = new TcpClient();
        private NetworkDataService _networkDataService;
        private GameState _gameState;
        private GameState _previousGameState;
        private string _username;
        private string _winningText;
        public async Task RunAsync(string server = null, int port = default)
        {
            await ConnectTCPClientAsync(server, port);
            SendUsername();
            while (true)
            {
                ReceiveGameConfiguration();
                //receive preflop
                ReceiveCards(ClientGamePhase.PreflopBetting);
                //send preflop bet
                SendBet(ClientGamePhase.ShowFlop);
                //get flop
                ReceiveCards(ClientGamePhase.ShowFlop);
                SendBet(ClientGamePhase.ShowTurn);
                ReceiveCards(ClientGamePhase.ShowTurn);
                SendBet(ClientGamePhase.ShowRiver);
                ReceiveCards(ClientGamePhase.ShowRiver);
                SendBet(ClientGamePhase.RoundResults);
                ShowResults();
            }
        }
        private async Task ConnectTCPClientAsync(string server = null, int portInput = default)
        {
            IPEndPoint ipEndPoint;
            if (server != null && portInput != default)
            {
                if (IPAddress.TryParse(server, out var ipAddress) && ushort.TryParse(portInput.ToString(), out var port))
                {
                    ipEndPoint = new IPEndPoint(ipAddress, port);
                }
                else
                {
                    throw new Exception("Could not parse command line connection string");
                }
            }
            else
            {
                ConsoleService.PrintTitle("Enter IP Address and Port of the server (xxx.xxx.xxx.xxx:xxxxxx)");
                ipEndPoint = ConsoleService.TakeIPEndPointInput();
            }

            await _tcpClient.ConnectAsync(ipEndPoint.Address, ipEndPoint.Port);
            Console.WriteLine("Connected to server");
            _networkDataService = new NetworkDataService(_tcpClient.GetStream());
        }
        private void SendUsername()
        {
            do
            {
                Console.WriteLine("Enter your username:");
                _username = ConsoleService.TakeInput();
                _networkDataService.WriteString(_username);

                var response = _networkDataService.Read();
                if (response == ClientGamePhase.Error)
                {
                    Console.WriteLine("That username is already taken, please try another.");
                }
                else if (response == ClientGamePhase.GatherPlayers)
                {
                    Console.WriteLine("The server is still accepting players, wait for the server to start the game.");
                    break;
                }
                else
                {
                    throw new Exception("Expected another response");
                }
            } while (true);
        }
        private void UpdateGameState()
        {
            _previousGameState = _gameState;
            _gameState = _networkDataService.ReadObject<GameState>();
        }
        private void ReceiveGameConfiguration()
        {
            UpdateGameState();
            if (_gameState.CurrentPhase != ClientGamePhase.ConfigureGame)
            {
                throw new Exception("Expected ConfigureGame state, received " + _gameState.CurrentPhase.ToString());
            }
        }
        private void ReceiveCards(ClientGamePhase currentPhase)
        {
            UpdateGameState();
            if (_gameState.CurrentPhase != currentPhase)
            {
                throw new Exception("Wrong phase?");
            }
        }
        private void SendBet(ClientGamePhase nextPhase)
        {
            UpdateGameState();
            PrintGameState();
            do
            {
                var nextPlayerToBet = _networkDataService.ReadString();
                if (nextPlayerToBet == _username)
                {
                    var bet = GetUserBet();
                    _networkDataService.WriteInteger(bet);
                }
                else if (nextPlayerToBet == NetworkConstants.NextPhasePayloadString)
                {
                    Console.WriteLine("Going to the " + nextPhase);
                    return;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine($"Waiting on {nextPlayerToBet} to bet");
                    Console.WriteLine();
                }
                UpdateGameState();
                if (_gameState.CurrentPhase == nextPhase)
                {
                    Console.WriteLine("Going to the " + nextPhase);
                    return;
                }
                if (_gameState.CurrentPlayerBets.Count() == 0 || _previousGameState.CurrentPlayerBets.Select(x => x.Value).Sum() != _gameState.CurrentPlayerBets.Select(x => x.Value).Sum() || _previousGameState.CurrentPlayerBets.Count() != _gameState.CurrentPlayerBets.Count())
                {
                    PrintGameState();
                }
                
            } while (true);
        }
        private void ShowResults()
        {
            UpdateGameState();
            _winningText = _networkDataService.ReadString();
        }
        private int GetUserBet()
        {
            var alreadyBet = false;
            // Check if all bets are the same
            if (_gameState.CurrentPlayerBets.Any(x => x.Key == _username))
            {
                alreadyBet = true;
            }

            int bet = PokerConstants.YetToBet;

            if (alreadyBet)
            {
                bet = _gameState.CurrentPlayerBets.First(x => x.Key == _username).Value;
                if (bet != PokerConstants.YetToBet) Console.Write($"Current bet ${bet}, increase it by value or 'call': ");
            }
            else
            {
                Console.WriteLine("Enter your bet value, 'call', 'fold' or 'check':");
            }

            do
            {
                var betString = ConsoleService.TakeInput().ToLower();
                if (betString == "fold")
                {
                    bet = PokerConstants.Fold;
                }
                else if (betString == "check" && UserCanCheck())
                {
                    bet = 0;
                }
                else if (betString == "call")
                {
                    var highestBet = _gameState.CurrentPlayerBets.Select(x => x.Value).Max();
                    var betToCall = highestBet - (bet < 0 ? 0 : bet);
                    return betToCall;
                }
                else
                {
                    // Make sure the bet is greater than a check, to check use the string command
                    if (int.TryParse(betString, out var betSize) && betSize > PokerConstants.Check)
                    {
                        // Increase bet or set bet
                        bet = betSize;
                    }
                    else
                    {
                        continue;
                    }

                }
                if (!( bet == PokerConstants.YetToBet || !UserCanCheck() && bet == PokerConstants.Check))
                {
                    break;
                }
                Console.WriteLine("Invalid input, please try again");
            } while (true);
            return bet;
        }
        private bool UserCanCheck()
        {
            var playerCount = _gameState.CurrentPlayerBets.Length;
            if (playerCount == 0)
            {
                return true;
            }
            if (_gameState.CurrentPlayerBets.Select(x=>x.Value).Sum() == 0)
            {
                return true;
            }
            var betsMatchingValue = _gameState.CurrentPlayerBets.Select(x => x.Value).GroupBy(x => x).First().Count();
            return betsMatchingValue == playerCount;
        }
        private void PrintGameState(bool clearConsole = true)
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            if (_winningText != null)
            {
                Console.WriteLine(_winningText);
                Console.WriteLine();
                _winningText = null;
            }

            ConsoleService.PrintTitle(_gameState.CurrentPhase.ToString());
            Console.WriteLine("Hand: " + string.Join(' ', _gameState.ClientHand));
            Console.WriteLine();
            if (_gameState.ShownCommunityCards != null && _gameState.ShownCommunityCards.Count() != 0)
            {
                Console.WriteLine("Community Cards: " + string.Join(' ', _gameState.ShownCommunityCards));
                Console.WriteLine();
            }
            

            if (_gameState.CurrentPlayerBets != null && _gameState.CurrentPlayerBets.Count() >= 1)
            {
                Console.WriteLine("Current Bets:");
                foreach (var (key, value) in _gameState.CurrentPlayerBets)
                {
                    Console.WriteLine($"{key} - {(value == 0 ? "Check" : "$" + value.ToString())}");
                }
            }
            string seatPosition;

            // Convert the player position index to a player-friendly string
            switch (_gameState.PlayerCurrentSeatPosition)
            {
                case PokerConstants.DealerPosition:
                    seatPosition = "Dealer";
                    break;
                case PokerConstants.SmallBlindPosition:
                    seatPosition = "SB";
                    break;
                case PokerConstants.BigBlindPosition:
                    seatPosition = "BB";
                    break;
                case PokerConstants.UTGPosition:
                    seatPosition = "UTG";
                    break;
                default:
                    seatPosition = _gameState.PlayerCurrentSeatPosition.ToString();
                    break;
            }

            Console.WriteLine("Position: " + seatPosition);
            Console.WriteLine(_gameState.Players.Aggregate("Players", (currentString, nextItem) => currentString + nextItem.Key + $"(${nextItem.Value}), " ).TrimEnd(',', ' '));
            Console.WriteLine();
        }
    }
}
