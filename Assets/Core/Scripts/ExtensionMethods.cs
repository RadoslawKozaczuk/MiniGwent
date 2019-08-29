using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Core
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns last n elements.
        /// </summary>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int n) 
            => source.Skip(Math.Max(0, source.Count() - n));

        public static void AllOnTheRight<T>(this List<T> source, int slotNumber, Action<T> action) 
            => source.TakeLast(source.Count - slotNumber).ToList().ForEach(action);
    }
}
