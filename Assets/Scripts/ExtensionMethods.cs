using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns last n elements.
        /// </summary>
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int n) 
            => source.Skip(Math.Max(0, source.Count() - n));
    }
}
