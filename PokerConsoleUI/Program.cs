//#define SHOW_DEBUG_LOG
using System;
using PokerLibrary;
using System.Threading.Tasks;
using PokerLibrary.Networking;

namespace PokerConsoleUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if SHOW_DEBUG_LOG
            ConsoleService.ShowDebugLog = true;
#else
            ConsoleService.ShowDebugLog = false;
#endif

            PlayOnLAN(args).GetAwaiter().GetResult();
        }

        private static async Task PlayOnLAN(string[] args)
        {
            string server = null;
            int port = default;
            string option = string.Empty;
            if (args != null && args.Length <= 2 && args.Length > 0)
            {
                if (args.Length == 2)
                {
                    option = args[1].ToLower();
                }
                var parts = args[0].Split(":", 2);
                server = parts[0];
                port = int.Parse(parts[1]);
            }

            ConsoleService.PrintTitle("Console Poker on LAN");
            if (option == string.Empty)
            {
                // TODO: Temporary, should clients be able to host their own servers?
                option = "c";
                
                // Console.WriteLine("(S)erver or (C)lient");
                // option = ConsoleService.TakeSpecificInputs(caseSensitive: false, "s", "c", "server", "client");
            }

            Console.Clear();
            switch (option)
            {
                case "s":
                case "server":
                    ConsoleService.PrintTitle("Console Poker Server");
                    var gameServer = new GameServer(port);
                    gameServer.Start();
                    Console.ReadLine();
                    break;
                case "c":
                case "client":
                    ConsoleService.PrintTitle("Console Poker Client");
                    var gameClient = new GameClient();
                    await gameClient.RunAsync(server, port);
                    break;
            }
        }
    }
}