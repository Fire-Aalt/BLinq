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
        /// Returns the element with the maximum key according to the default comparer.
        /// </summary>
        /// <param name="keySelector">Selector used to compute a key for each element.</param>
        /// <returns>The element with the maximum selected key.</returns>
        public T MaxBy<TKey, TKeySelector>(TKeySelector keySelector)
            where TKey : unmanaged, IComparable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return MaxBy<TKey, TKeySelector, AscendingComparer<TKey>>(keySelector, new AscendingComparer<TKey>());
        }

        /// <summary>
        /// Returns the element with the maximum key according to <paramref name="comparer"/>.
        /// </summary>
        /// <param name="keySelector">Selector used to compute a key for each element.</param>
        /// <param name="comparer">The comparer used to order keys.</param>
        /// <returns>The element with the maximum selected key.</returns>
        public T MaxBy<TKey, TKeySelector, TKeyComparer>(TKeySelector keySelector, TKeyComparer comparer)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return BLinqUtilities.MaxBy<T, TKey, TEnumerator, TKeySelector, TKeyComparer>(
                GetEnumerator(),
                keySelector,
                comparer);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the element with the maximum key according to the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selector used to compute a key for each element.</param>
        /// <returns>The element with the maximum selected key.</returns>
        public static T MaxBy<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.MaxBy<TKey, TKeySelector, AscendingComparer<TKey>>(keySelector, new AscendingComparer<TKey>());
        }

        /// <summary>
        /// Returns the element with the maximum key according to <paramref name="comparer"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selector used to compute a key for each element.</param>
        /// <param name="comparer">The comparer used to order keys.</param>
        /// <returns>The element with the maximum selected key.</returns>
        public static T MaxBy<T, TKey, TEnumerator, TKeySelector, TKeyComparer>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return source.MaxBy<TKey, TKeySelector, TKeyComparer>(keySelector, comparer);
        }

        /// <summary>
        /// Returns the element with the maximum key according to the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selector used to compute a key for each element.</param>
        /// <returns>The element with the maximum selected key.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T MaxBy<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }

        /// <summary>
        /// Returns the element with the maximum key according to <paramref name="comparer"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selector used to compute a key for each element.</param>
        /// <param name="comparer">The comparer used to order keys.</param>
        /// <returns>The element with the maximum selected key.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T MaxBy<T, TKey, TEnumerator, TKeyComparer>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThrowCodeGen<T>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static T MaxBy<T, TKey, TEnumerator, TKeySelector, TKeyComparer>(
            TEnumerator enumerator,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains no elements.");
            }

            var best = enumerator.Current;
            var bestKey = keySelector.Select(in best);
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                var key = keySelector.Select(in value);
                if (comparer.Compare(key, bestKey) > 0)
                {
                    best = value;
                    bestKey = key;
                }
            }

            enumerator.Dispose();
            return best;
        }
    }
}
