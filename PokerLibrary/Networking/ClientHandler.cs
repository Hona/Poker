using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace PokerLibrary.Networking.Models
{
    public class ClientHandler
    {
        public string Username { get; set; }
        private TcpClient _tcpClient;
        internal NetworkDataService NetworkDataService;
        public IPAddress IPAddress => ((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address;
        public ClientHandler(TcpClient client)
        {
            _tcpClient = client;
            NetworkDataService = new NetworkDataService(_tcpClient.GetStream());
        }
        public void NegotiateUsername(List<ClientHandler> clients)
        {
            do
            {
                Username = NetworkDataService.ReadString();
                if (clients.Any(x => x.Username == Username))
                {
                    NetworkDataService.Write(ClientGamePhase.Error);
                    Console.WriteLine($"A user with username: '{Username}' already exists");
                }
                else
                {
                    NetworkDataService.Write(ClientGamePhase.GatherPlayers);
                    Console.WriteLine($"A user with username: '{Username}' connected");
                    break;
                }
            } while (true);
        }
        public bool IsConnected => _tcpClient.Connected;
    }
}
