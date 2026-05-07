using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Adds a secondary ascending ordering using the element as the key.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, T, IdentitySelector<T>, AscendingComparer<T>>>> ThenBy<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThenBy<T, T, TEnumerator, TComparer, IdentitySelector<T>>(
                source,
                new IdentitySelector<T>());
        }

        /// <summary>
        /// Adds a secondary descending ordering using the element as the key.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, T, IdentitySelector<T>, DescendingComparer<T>>>> ThenByDescending<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThenByDescending<T, T, TEnumerator, TComparer, IdentitySelector<T>>(
                source,
                new IdentitySelector<T>());
        }

        /// <summary>
        /// Adds a secondary ascending ordering using a key selector and the default key comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeySelector, AscendingComparer<TKey>>>> ThenBy<T, TKey, TEnumerator, TComparer, TKeySelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return ThenBy<T, TKey, TEnumerator, TComparer, TKeySelector, AscendingComparer<TKey>>(
                source,
                keySelector,
                new AscendingComparer<TKey>());
        }

        /// <summary>
        /// Adds a secondary ascending ordering using a key selector and comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>> ThenBy<T, TKey, TEnumerator, TComparer, TKeySelector, TKeyComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return new OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>>(
                source._source,
                new ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>(
                    source._comparer,
                    new KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>(keySelector, comparer)));
        }

        /// <summary>
        /// Adds a secondary descending ordering using a key selector and the default key comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeySelector, DescendingComparer<TKey>>>> ThenByDescending<T, TKey, TEnumerator, TComparer, TKeySelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return ThenBy<T, TKey, TEnumerator, TComparer, TKeySelector, DescendingComparer<TKey>>(
                source,
                keySelector,
                new DescendingComparer<TKey>());
        }

        /// <summary>
        /// Adds a secondary descending ordering using a key selector and comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeySelector, ReverseComparer<TKey, TKeyComparer>>>> ThenByDescending<T, TKey, TEnumerator, TComparer, TKeySelector, TKeyComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThenBy<T, TKey, TEnumerator, TComparer, TKeySelector, ReverseComparer<TKey, TKeyComparer>>(
                source,
                keySelector,
                new ReverseComparer<TKey, TKeyComparer>(comparer));
        }

        /// <summary>
        /// Adds a secondary ascending ordering using a delegate key selector and the default key comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, AscendingComparer<TKey>>>> ThenBy<T, TKey, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, AscendingComparer<TKey>>>>>();
        }

        /// <summary>
        /// Adds a secondary ascending ordering using a delegate key selector and comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeyComparer>>> ThenBy<T, TKey, TEnumerator, TComparer, TKeyComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, TKeyComparer>>>>();
        }

        /// <summary>
        /// Adds a secondary descending ordering using a delegate key selector and the default key comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, DescendingComparer<TKey>>>> ThenByDescending<T, TKey, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, DescendingComparer<TKey>>>>>();
        }

        /// <summary>
        /// Adds a secondary descending ordering using a delegate key selector and comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, ReverseComparer<TKey, TKeyComparer>>>> ThenByDescending<T, TKey, TEnumerator, TComparer, TKeyComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, KeySelectorComparer<T, TKey, ReverseComparer<TKey, TKeyComparer>>>>>();
        }
    }

    public struct ThenByComparer<T, TPrimaryComparer, TSecondaryComparer> : IComparer<T>
        where T : unmanaged
        where TPrimaryComparer : unmanaged, IComparer<T>
        where TSecondaryComparer : unmanaged, IComparer<T>
    {
        private TPrimaryComparer _primaryComparer;
        private TSecondaryComparer _secondaryComparer;

        public ThenByComparer(TPrimaryComparer primaryComparer, TSecondaryComparer secondaryComparer)
        {
            _primaryComparer = primaryComparer;
            _secondaryComparer = secondaryComparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T x, T y)
        {
            var result = _primaryComparer.Compare(x, y);
            return result != 0 ? result : _secondaryComparer.Compare(x, y);
        }
    }

    public struct ReverseComparer<T, TComparer> : IComparer<T>
        where T : unmanaged
        where TComparer : unmanaged, IComparer<T>
    {
        private TComparer _comparer;

        public ReverseComparer(TComparer comparer)
        {
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Compare(T x, T y)
        {
            return _comparer.Compare(y, x);
        }
    }
}
