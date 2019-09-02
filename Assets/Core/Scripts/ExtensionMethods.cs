﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Core
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns last n elements.
        /// </summary>
        public static IEnumerable<T> GetLast<T>(this IEnumerable<T> source, int n) 
            => source.Skip(Math.Max(0, source.Count() - n));

        /// <summary>
        /// Executes given action on all elements starting from the slotNumber-element (exclusive).
        /// For example if slotNumber = 1, the method will execute on all elements except the first two.
        /// </summary>
        public static void AllOnTheRight<T>(this IEnumerable<T> source, int slotNumber, Action<T> action) 
            => source.GetLast(source.Count() - slotNumber).ToList().ForEach(action);

        public static IEnumerable<T> GetAllExceptOne<T>(this IEnumerable<T> source, int except)
        {
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
                throw new ArgumentOutOfRangeException("slotNumber", "SlotNumber argument should not be negative or greater than source collection's size");
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
                throw new ArgumentOutOfRangeException("slotNumber", "SlotNumber argument should not be negative or greater than source collection's size");
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
    }
}
