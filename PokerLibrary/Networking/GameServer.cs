using PokerLibrary.Networking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PokerLibrary.Networking
{
    public class GameServer
    {
        private TcpListener _tcpListener;
        private bool _acceptingClients;
        private List<ClientHandler> _clients;
        private Game _game;
        private GameState _gameState;
        private Dictionary<Player, int> _bets;
        private IEnumerable<NetworkDataService> _networkDataServices => _clients.Select(x => x.NetworkDataService);
        public GameServer(int port = default)
        {
            _clients = new List<ClientHandler>();
            BuildTCPListener(port);
        }
        private void BuildTCPListener(int portInput = default)
        {
            int port;
            if (portInput == default)
            {
                ConsoleService.PrintTitle("Enter TCP Port to listen on (0 - 65535)");
                port = ConsoleService.TakeIntegerInput();
            }
            else
            {
                port = portInput;
            }

            _tcpListener = new TcpListener(IPAddress.Loopback, port);
            Console.Title = $"{NetworkService.GetLocalIPAddressString()}:{port}";
        }
        public void Start()
        {
            // Start listening for clients
            _tcpListener.Start();

            // Don't await, so we can configure the game
            AcceptClientsAsync();

            ConfigureGame();
            while (true)
            {
                SendClientsGameState(ClientGamePhase.ConfigureGame);
                StartGame();
                PreflopBetting();
                ShowFlop();
                FlopBetting();
                ShowTurn();
                TurnBetting();
                ShowRiver();
                RiverBetting();
                SendResults();
                SetupNextRound();
            }
        }
        private async Task AcceptClientsAsync()
        {
            _acceptingClients = true;
            await Task.Run(async () =>
            {
                while (_acceptingClients)
                {
                    Console.WriteLine("Waiting for connections...");
                    var client = await _tcpListener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected from: " + client.Client.RemoteEndPoint.ToString());

                    var clientHandler = new ClientHandler(client);
                    clientHandler.NegotiateUsername(_clients);
                    _clients.Add(clientHandler);


                    Console.WriteLine($"{_clients.Where(x => x.IsConnected).Count()} active connections");
                    Console.WriteLine();
                }
            });
        }
        private void SetupNextRound()
        {
            _game.SetupNextRound();
        }
        private void ConfigureGame()
        {
            Console.Clear();
            ConsoleService.PrintTitle("Configure Game");

            _gameState = new GameState();

            Console.WriteLine("Starting Money:");
            _gameState.StartingMoney = ConsoleService.TakeIntegerInput();

            Console.WriteLine("Write 'start' to begin the game");

            // Discard, as the function only continues on 'start'
            _ = ConsoleService.TakeSpecificInputs(caseSensitive: false, "start");
            
            _acceptingClients = false;

            var players = new List<Player>();
            foreach (var client in _clients)
            {
                // The player's password is the IP address - each player shouldn't know the other users IP easily, as they are only connecting to the server
                players.Add(new Player(client.Username, _gameState.StartingMoney, client.IPAddress.ToString()));
            }

            _game = new Game(players);

        }
        private void StartGame()
        { 
            _game.DealAllPlayers();
            _bets = _game.PostBlinds();
            _gameState.CurrentPlayerBets = _bets.Select(x => new KeyValuePair<string, int>(x.Key.Name, x.Value)).ToArray();
            SendClientsGameState(ClientGamePhase.PreflopBetting);
        }
        private void SendResults()
        {
            // _game.GetWinnerText pays out the winner
            // TODO: Refactor winner payout to another function
            var winnerText = _game.GetWinnerText(_game.Players);
            Console.WriteLine(winnerText);
            foreach (var networkDataService in _networkDataServices)
            {
                networkDataService.WriteString(winnerText);
            }
        }
        private void BettingRound(ClientGamePhase currentPhase)
        {
            if (currentPhase == ClientGamePhase.PreflopBetting)
            {
                SendClientsGameState(currentPhase);
            }

            var activePlayers = _game.ActivePlayers;
            TakeAllBets(ref _bets, ref activePlayers, currentPhase, preflop: currentPhase == ClientGamePhase.PreflopBetting ? true : false);
            _game.Pot += _bets.Values.Sum();
            _game.ActivePlayers = activePlayers;
        }
        private void PreflopBetting() => BettingRound(ClientGamePhase.PreflopBetting);
        private void ShowFlop() => ShowCards(ClientGamePhase.ShowFlop);
        private void ShowCards(ClientGamePhase currentRound)
        {
            switch (currentRound)
            {
                case ClientGamePhase.ShowFlop:
                    _game.Flop();
                    break;
                case ClientGamePhase.ShowTurn:
                    _game.Turn();
                    break;
                case ClientGamePhase.ShowRiver:
                    _game.River();
                    break;
                default:
                    throw new Exception($"Can't show cards on that game phase: ({currentRound.ToString()})");
            }
            // TODO: Refactor this, copy pasted from client code
            Console.WriteLine("Community Cards: " + string.Join(' ', _game.ShownCommunityCards));
            ResetBets();
            SendClientsGameState(currentRound);
        }
        private void ResetBets()
        {
            _bets = new Dictionary<Player, int>();
            _gameState.CurrentPlayerBets = _bets.Select(x => new KeyValuePair<string, int>(x.Key.Name, x.Value)).ToArray();
        }
        private void FlopBetting() => BettingRound(ClientGamePhase.FlopBetting);
        private void ShowTurn() => ShowCards(ClientGamePhase.ShowTurn);
        private void TurnBetting() => BettingRound(ClientGamePhase.TurnBetting);
        private void ShowRiver() => ShowCards(ClientGamePhase.ShowRiver);
        private void RiverBetting() => BettingRound(ClientGamePhase.RiverBetting);
        private void SendNextPhasePayload(ClientGamePhase currentPhase)
        {
            foreach (var item in _networkDataServices)
            {
                item.WriteString(NetworkConstants.NextPhasePayloadString);
            }
            _gameState.CurrentPlayerBets = _bets.Select(x => new KeyValuePair<string, int>(x.Key.Name, x.Value)).ToArray();
            SendClientsGameState((ClientGamePhase)((int)currentPhase + 1));
        }
        private void TakeAllBets(ref Dictionary<Player, int> bets, ref List<Player> players, ClientGamePhase currentPhase, bool preflop = false)
        {
            var startPlayer = preflop ? PokerConstants.UTGPosition : PokerConstants.SmallBlindPosition;

            if (players.Count == 2)
            {
                startPlayer = PokerConstants.SmallBlind;
            }

            do
            {
                for (var i = startPlayer; i < players.Count; i++)
                {
                    if (TakeBet(ref players, i, ref bets, currentPhase) && !bets.Any(x => x.Value == PokerConstants.YetToBet) && AllBetsMatchingValue(bets) && bets.Count() == _game.ActivePlayers.Count())
                    {
                        SendNextPhasePayload(currentPhase);

                        return;
                    }
                    _gameState.CurrentPlayerBets = _bets.Select(x => new KeyValuePair<string, int>(x.Key.Name, x.Value)).ToArray();
                    SendClientsGameState(currentPhase);
                }

                for (var i = 0; i < startPlayer && i < players.Count; i++)
                {
                    if (TakeBet(ref players, i, ref bets, currentPhase) && !bets.Any(x => x.Value == PokerConstants.YetToBet) && AllBetsMatchingValue(bets) && bets.Count() == _game.ActivePlayers.Count())
                    {
                        SendNextPhasePayload(currentPhase);
                        return;
                    }
                    _gameState.CurrentPlayerBets = _bets.Select(x => new KeyValuePair<string, int>(x.Key.Name, x.Value)).ToArray();
                    SendClientsGameState(currentPhase);
                }
            } while (true);
        }
        private bool TakeBet(ref List<Player> players, int i, ref Dictionary<Player, int> bets, ClientGamePhase currentGamePhase)
        {
            if(i >= players.Count)
            {
                i %= players.Count;
            }
            
            var currentPlayer = players[i];

            // TODO: Check this
            var isCurrentPlayerBigBlind = i == (2 < players.Count ? 2 : 0);

            bool alreadyBet = false;
            // Check if all bets are the same
            if (bets.ContainsKey(currentPlayer))
            {

                if (AllBetsMatchingValue(bets) && bets.Count() == _game.ActivePlayers.Count()) 
                {
                    if (currentGamePhase == ClientGamePhase.PreflopBetting) 
                    {
                        if (!isCurrentPlayerBigBlind || (isCurrentPlayerBigBlind && bets[currentPlayer] != 2))
                        {
                            return true;
                        }
                        
                    }
                    else
                    {
                        return true;
                    }
                }
                alreadyBet = true;
            }

            foreach (var networkDataService in _networkDataServices)
            {
                networkDataService.WriteString(currentPlayer.Name);
            }

            int bet = PokerConstants.YetToBet;

            if (alreadyBet)
            {
                bet = bets[currentPlayer];
            }
            
            
            var receivedBet = _clients.First(x => x.Username == currentPlayer.Name).NetworkDataService.ReadInteger();

            if (receivedBet > currentPlayer.Money)
            {
                // TODO: ERROR
                throw new Exception("A player tried to bet more than they have");
            }

            if (receivedBet != PokerConstants.Fold)
            {
                // Increase bet or set bet
                if (alreadyBet)
                {
                    if (bet != PokerConstants.Fold)
                    {
                        // Temporarily refund the player, to deduct the total later
                        currentPlayer.GiveMoney(bet);
                        bet += receivedBet;
                    }
                    else
                    {
                        throw new Exception("Cannot bet after folding");
                    }
                }
                else
                {
                    bet = receivedBet;
                }
            }
            else
            {
                Console.WriteLine($"{currentPlayer} folded");
                // Player folded
                players.Remove(currentPlayer);
                return false; 
            }

            if (alreadyBet)
                bets[currentPlayer] = bet;
            else
                bets.Add(currentPlayer, bet);

            if (bet != PokerConstants.YetToBet) 
            {
                Console.WriteLine($"{currentPlayer} bet ${bet}");
                currentPlayer.SubtractBet(bet); 
            }
            return false;
        }

        private IEnumerable<KeyValuePair<Player, int>> GetPlayedBets(ref Dictionary<Player, int> bets)
        {
            return bets.Where(x => x.Value >= 0);
        }

        /// <summary>
        /// Returns the first check or above bet
        /// </summary>
        private KeyValuePair<Player, int> GetFirstPlayedBet(ref Dictionary<Player, int> bets)
        {
            return GetPlayedBets(ref bets).First();
        }
        /// <summary>
        /// Check if the bets are all equal, and are not undecided
        /// </summary>
        /// <param name="bets"></param>
        /// <returns></returns>
        private bool AllBetsMatchingValue(IEnumerable<KeyValuePair<Player, int>> bets)
        {
            // Check if there are no bets yet
            if (bets == null || bets.Count() == 0)
            {
                return false;
            }


            var betDictionary = bets.ToDictionary(x => x.Key, y => y.Value);
            var firstBet = GetFirstPlayedBet(ref betDictionary);

            return bets.All(x => x.Value == firstBet.Value);
        }
        private void SendClientsGameState(ClientGamePhase currentPhase)
        {
            
            _gameState.CurrentPhase = currentPhase;
            if (_game == null)
            {
                _gameState.Players = null;
                _gameState.ShownCommunityCards = null;
            }
            else
            {
                _gameState.Players = _game.Players.Select(x => new KeyValuePair<string, int>(x.Name, x.Money)).ToList();
                _gameState.ShownCommunityCards = _game.ShownCommunityCards;
            }
            

            foreach (var clientHandler in _clients)
            {
                // Send the user their specific data
                if (_game != null)
                {
                    var currentPlayer = _game.Players.First(x => x.Name == clientHandler.Username);

                    _gameState.ClientHand = currentPlayer.Hand;
                    if (_game.ActivePlayers != null)
                    {
                        _gameState.PlayerCurrentSeatPosition = _game.ActivePlayers.IndexOf(currentPlayer);
                    }
                }
                else
                {
                    _gameState.ClientHand = null;
                    _gameState.PlayerCurrentSeatPosition = -1;
                }
                 clientHandler.NetworkDataService.WriteObject(_gameState);
            }
        }
    }
}
