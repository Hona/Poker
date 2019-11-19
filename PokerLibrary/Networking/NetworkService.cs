using System;
using System.Net;
using System.Net.Sockets;

namespace PokerLibrary.Networking
{
    public static class NetworkService
    {
        public static string GetLocalIPAddressString()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with IPv4 capabilities.");
        }
    }
}
