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
        /// Orders the query in ascending order by the selected key.
        /// </summary>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <returns>A query that yields the source elements in ascending key order.</returns>
        public Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, AscendingComparer<TKey>>>, T> OrderBy<TKey, TKeySelector>(
            TKeySelector keySelector)
            where TKey : unmanaged, IComparable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return OrderBy<TKey, TKeySelector, AscendingComparer<TKey>>(
                keySelector,
                new AscendingComparer<TKey>());
        }

        /// <summary>
        /// Orders the query in ascending order by the selected key and comparer.
        /// </summary>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <param name="comparer">Compares keys for ordering.</param>
        /// <returns>A query that yields the source elements in ascending key order.</returns>
        public Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>, T> OrderBy<TKey, TKeySelector, TKeyComparer>(
            TKeySelector keySelector,
            TKeyComparer comparer)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return new Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>, T>(
                new OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>(
                    this,
                    new KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>(keySelector, comparer)));
        }

        /// <summary>
        /// Orders the query in descending order by the selected key.
        /// </summary>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <returns>A query that yields the source elements in descending key order.</returns>
        public Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, DescendingComparer<TKey>>>, T> OrderByDescending<TKey, TKeySelector>(
            TKeySelector keySelector)
            where TKey : unmanaged, IComparable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return OrderBy<TKey, TKeySelector, DescendingComparer<TKey>>(
                keySelector,
                new DescendingComparer<TKey>());
        }

        /// <summary>
        /// Orders the query in descending order by the selected key and comparer.
        /// </summary>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <param name="comparer">Compares keys for ordering.</param>
        /// <returns>A query that yields the source elements in descending key order.</returns>
        public Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, ReverseComparer<TKey, TKeyComparer>>>, T> OrderByDescending<TKey, TKeySelector, TKeyComparer>(
            TKeySelector keySelector,
            TKeyComparer comparer)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return OrderBy<TKey, TKeySelector, ReverseComparer<TKey, TKeyComparer>>(
                keySelector,
                new ReverseComparer<TKey, TKeyComparer>(comparer));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Orders the query in ascending order using the element value as the key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>A query that yields the source elements in ascending value order.</returns>
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, T, IdentitySelector<T>, AscendingComparer<T>>>, T> OrderBy<T, TEnumerator>(
            this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.OrderBy<T, IdentitySelector<T>>(new IdentitySelector<T>());
        }

        /// <summary>
        /// Orders the query in descending order using the element value as the key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>A query that yields the source elements in descending value order.</returns>
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, T, IdentitySelector<T>, DescendingComparer<T>>>, T> OrderByDescending<T, TEnumerator>(
            this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.OrderByDescending<T, IdentitySelector<T>>(new IdentitySelector<T>());
        }

        /// <summary>
        /// Orders the query in ascending order by the selected key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <returns>A query that yields the source elements in ascending key order.</returns>
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, AscendingComparer<TKey>>>, T> OrderBy<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.OrderBy<TKey, TKeySelector>(keySelector);
        }

        /// <summary>
        /// Orders the query in ascending order by the selected key and comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <param name="comparer">Compares keys for ordering.</param>
        /// <returns>A query that yields the source elements in ascending key order.</returns>
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>, T> OrderBy<T, TKey, TEnumerator, TKeySelector, TKeyComparer>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return source.OrderBy<TKey, TKeySelector, TKeyComparer>(keySelector, comparer);
        }

        /// <summary>
        /// Orders the query in descending order by the selected key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <returns>A query that yields the source elements in descending key order.</returns>
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, DescendingComparer<TKey>>>, T> OrderByDescending<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.OrderByDescending<TKey, TKeySelector>(keySelector);
        }

        /// <summary>
        /// Orders the query in descending order by the selected key and comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selects the key for each element.</param>
        /// <param name="comparer">Compares keys for ordering.</param>
        /// <returns>A query that yields the source elements in descending key order.</returns>
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, ReverseComparer<TKey, TKeyComparer>>>, T> OrderByDescending<T, TKey, TEnumerator, TKeySelector, TKeyComparer>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return source.OrderByDescending<TKey, TKeySelector, TKeyComparer>(keySelector, comparer);
        }
        
        /// <summary>
        /// Orders the query in ascending order using a delegate key selector and the default key comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, AscendingComparer<TKey>>>, T> OrderBy<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, AscendingComparer<TKey>>>, T>>();
        }

        /// <summary>
        /// Orders the query in ascending order using a delegate key selector and comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeyComparer>>, T> OrderBy<T, TKey, TEnumerator, TKeyComparer>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThrowCodeGen<Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, TKeyComparer>>, T>>();
        }

        /// <summary>
        /// Orders the query in descending order using a delegate key selector and the default key comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, DescendingComparer<TKey>>>, T> OrderByDescending<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, DescendingComparer<TKey>>>, T>>();
        }

        /// <summary>
        /// Orders the query in descending order using a delegate key selector and comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, ReverseComparer<TKey, TKeyComparer>>>, T> OrderByDescending<T, TKey, TEnumerator, TKeyComparer>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThrowCodeGen<Query<OrderBy<TEnumerator, T, KeySelectorComparer<T, TKey, ReverseComparer<TKey, TKeyComparer>>>, T>>();
        }
    }

    public struct IdentitySelector<T> : ISelector<T, T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Select(in T value)
        {
            return value;
        }
    }

    public struct KeySelectorComparer<TSource, TKey, TKeySelector, TKeyComparer> : IComparer<TSource>
        where TSource : unmanaged
        where TKey : unmanaged
        where TKeySelector : unmanaged, ISelector<TSource, TKey>
        where TKeyComparer : unmanaged, IComparer<TKey>
    {
        private TKeySelector _keySelector;
        private TKeyComparer _comparer;

        public KeySelectorComparer(TKeySelector keySelector, TKeyComparer comparer)
        {
            _keySelector = keySelector;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(TSource x, TSource y)
        {
            var xKey = _keySelector.Select(in x);
            var yKey = _keySelector.Select(in y);
            return _comparer.Compare(xKey, yKey);
        }
    }

    public struct KeySelectorComparer<TSource, TKey, TKeyComparer> : IComparer<TSource>
        where TSource : unmanaged
        where TKey : unmanaged
        where TKeyComparer : unmanaged, IComparer<TKey>
    {
        public int Compare(TSource x, TSource y)
        {
            return BLinqExtensions.ThrowCodeGen<int>();
        }
    }
}
