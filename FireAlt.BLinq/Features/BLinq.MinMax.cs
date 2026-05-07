using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the smallest element in the query according to <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">The comparer used to order values.</param>
        /// <returns>The minimum element in the sequence.</returns>
        public T Min<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            var enumerator = GetEnumerator();
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains no elements.");
            }

            var best = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (comparer.Compare(value, best) < 0)
                {
                    best = value;
                }
            }

            enumerator.Dispose();
            return best;
        }

        /// <summary>
        /// Returns the largest element in the query according to <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">The comparer used to order values.</param>
        /// <returns>The maximum element in the sequence.</returns>
        public T Max<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            var enumerator = GetEnumerator();
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains no elements.");
            }

            var best = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (comparer.Compare(value, best) > 0)
                {
                    best = value;
                }
            }

            enumerator.Dispose();
            return best;
        }
    }
    
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the smallest element in the query according to the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The minimum element in the sequence.</returns>
        public static T Min<TEnumerator, T>(this Query<TEnumerator, T> source)
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where T : unmanaged, IComparable<T>
        {
            return source.Min(new AscendingComparer<T>());
        }

        /// <summary>
        /// Returns the largest element in the query according to the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The maximum element in the sequence.</returns>
        public static T Max<TEnumerator, T>(this Query<TEnumerator, T> source)
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where T : unmanaged, IComparable<T>
        {
            return source.Max(new AscendingComparer<T>());
        }
    }
}
