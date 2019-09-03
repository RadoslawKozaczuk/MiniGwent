using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Core
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns last n elements. 
        /// When n is greater or equal the collection's size returns entire collection.
        /// When n = 0 returns empty collection.
        /// </summary>
        public static IEnumerable<T> GetLast<T>(this IEnumerable<T> source, int n)
        {
#if UNITY_EDITOR
            if (n < 0)
                throw new ArgumentOutOfRangeException("except", $"Except argument cannot be lower than 0");
#endif

            return source.Skip(Math.Max(0, source.Count() - n));
        }

        /// <summary>
        /// Executes given action on all elements starting from the slotNumber-element (inclusive).
        /// For example if slotNumber = 1, the method will execute on all elements except the first one.
        /// </summary>
        public static void AllOnTheRight<T>(this IEnumerable<T> source, int slotNumber, Action<T> action) 
            where T : class
            => source.GetLast(source.Count() - slotNumber).ToList().ForEach(action);

        public static IEnumerable<T> GetAllExceptOne<T>(this IEnumerable<T> source, int except)
        {
#if UNITY_EDITOR
            if (except < 0 || except >= source.Count())
                throw new ArgumentOutOfRangeException(
                    "except",
                    $"Except argument cannot be lower than 0 or greater or equal than the collection's size {source.Count()}");
#endif
            var list = new List<T>();
            for(int i = 0; i < source.Count(); i++)
                if (i != except)
                    list.Add(source.ElementAt(i));

            return list;
        }

        public static IEnumerable<T> GetLeftNeighbor<T>(this IEnumerable<T> source, int slotNumber)
        {
#if UNITY_EDITOR
            if (slotNumber < 0 || slotNumber >= source.Count())
                throw new ArgumentOutOfRangeException(
                    "slotNumber", 
                    "SlotNumber argument should not be negative or greater than source collection's size");
#endif

            return slotNumber == 0 
                ? new List<T>(0) 
                : new List<T>(1) { source.ElementAt(slotNumber - 1) };
        }

        public static IEnumerable<T> GetRightNeighbor<T>(this IEnumerable<T> source, int slotNumber)
        {
#if UNITY_EDITOR
            if (slotNumber < 0 || slotNumber >= source.Count())
                throw new ArgumentOutOfRangeException("slotNumber", "SlotNumber argument should not be negative or greater than source collection's size");
#endif

            return slotNumber == source.Count() - 1
                ? new List<T>(0)
                : new List<T>(1) { source.ElementAt(slotNumber + 1) };
        }

        public static IEnumerable<T> GetBothNeighbors<T>(this IEnumerable<T> source, int slotNumber)
        {
#if UNITY_EDITOR
            if (slotNumber < 0 || slotNumber >= source.Count())
                throw new ArgumentOutOfRangeException(
                    "slotNumber", 
                    "SlotNumber argument should not be negative or greater than source collection's size");
#endif

            var list = new List<T>(2);
            if (slotNumber < source.Count() - 1)
                list.Add(source.ElementAt(slotNumber + 1));
            if(slotNumber > 0)
                list.Add(source.ElementAt(slotNumber - 1));
            return list;
        }

        public static PlayerIndicator Opposite(this PlayerIndicator indicator) 
            => indicator == PlayerIndicator.Top
                ? PlayerIndicator.Bot
                : PlayerIndicator.Top;

        public static LineIndicator Opposite(this LineIndicator line) => (LineIndicator)(5 - (int)line);
    }
}
