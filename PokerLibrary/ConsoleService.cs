using System;
using System.Linq;
using System.Net;

namespace PokerLibrary
{
    public static class ConsoleService
    {
        public static bool ShowDebugLog { get; set; }
        public static void PrintTitle(string title)
        {
            Console.WriteLine($"-= {title} =-");
        }
        public static string TakeInput()
        {
            Console.Write(" > ");
            return Console.ReadLine();
        }
        public static int TakeIntegerInput()
        {
            do
            {
                var stringInput = TakeInput();
                if (int.TryParse(stringInput, out var parsedInput))
                {
                    return parsedInput;
                }

                Console.WriteLine("You did not input a number");
            } while (true);
        }
        public static IPEndPoint TakeIPEndPointInput()
        {
            do
            {
                var stringInput = TakeInput();

                if (stringInput.Contains(':'))
                {
                    var stringInputParts = stringInput.Split(':');

                    if (stringInputParts.Length == 2)
                    {
                        if (IPAddress.TryParse(stringInputParts[0], out var ipAddress) && ushort.TryParse(stringInputParts[1], out var port))
                        {
                            return new IPEndPoint(ipAddress, port);
                        }
                    }
                }

                Console.WriteLine("You did not input a valid IP:Port");
               
            } while (true);
        }
        public static string TakeSpecificInputs(bool caseSensitive, params string[] options)
        {
            string output = string.Empty;
            string currentInput = string.Empty;

            bool inputInvalid = false;

            do
            {
                currentInput = TakeInput();

                if (!options.Any(x => string.Equals(currentInput, x, caseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)))
                {
                    Console.WriteLine("That was not a valid input, please input one of the following: " + options.Aggregate("", (currentString, nextItem) => currentString + $"'{nextItem}' ").Trim());
                    inputInvalid = true;
                }
                else
                {
                    inputInvalid = false;
                }
            } while (inputInvalid);

            return currentInput;
        }
        public static void LogDebug(string input)
        {
            if (ShowDebugLog)
            {
                Console.WriteLine("[DEBUG] : " + input);
            }
        }
    }
}
