using System.Collections.Generic;
using System.Security.Cryptography;

namespace CardLibrary
{
    internal static class ListExtensions
    {
        private static void Shuffle<T>(this IList<T> list)
        {
            using var provider = new RNGCryptoServiceProvider();
            var n = list.Count;
            while (n > 1)
            {
                var box = new byte[1];
                do
                {
                    provider.GetBytes(box);
                } while (!(box[0] < n * (byte.MaxValue / n)));

                var k = box[0] % n;
                n--;
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        internal static Stack<T> Shuffle<T>(this Stack<T> stack)
        {
            var list = stack.ToArray();
            list.Shuffle();
            return list.ToStack();
        }

        private static Stack<T> ToStack<T>(this IEnumerable<T> list)
        {
            var stack = new Stack<T>();
            foreach (var t in list)
                stack.Push(t);

            return stack;
        }
    }
}