using System;
using System.Collections.Generic;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns whether the query contains <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparer">The equality comparer used to compare elements.</param>
        /// <returns><c>true</c> when the value is present; otherwise <c>false</c>.</returns>
        public bool Contains<TEqualityComparer>(T value, TEqualityComparer comparer)
            where TEqualityComparer : unmanaged, INativeEqualityComparer<T>
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (comparer.Equals(in current, in value))
                {
                    enumerator.Dispose();
                    return true;
                }
            }

            enumerator.Dispose();
            return false;
        }
    }
    
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns whether the query contains <paramref name="value"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="value">The value to search for.</param>
        /// <returns><c>true</c> when the value is present; otherwise <c>false</c>.</returns>
        public static bool Contains<T, TEnumerator>(this Query<TEnumerator, T> source, T value)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Contains(value, new NativeEqualityComparer<T>());
        }
    }
}
