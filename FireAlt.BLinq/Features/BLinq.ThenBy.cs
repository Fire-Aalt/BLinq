using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Adds a secondary ascending ordering to an already ordered query.
        /// </summary>
        /// <param name="source">The ordered source query.</param>
        /// <returns>An ordered query with secondary ascending ordering.</returns>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, AscendingComparer<T>>> ThenBy<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return source.ThenBy(new AscendingComparer<T>());
        }

        /// <summary>
        /// Adds a secondary descending ordering to an already ordered query.
        /// </summary>
        /// <param name="source">The ordered source query.</param>
        /// <returns>An ordered query with secondary descending ordering.</returns>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, DescendingComparer<T>>> ThenByDescending<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return source.ThenBy(new DescendingComparer<T>());
        }
        
                /// <summary>
        /// Adds a secondary ascending ordering to the query.
        /// </summary>
        /// <param name="source">The ordered source query.</param>
        /// <param name="comparer">Comparer used for the secondary ordering.</param>
        /// <returns>An ordered query that sorts by the existing ordering first and the new comparer second.</returns>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, TThenComparer>> ThenBy<TEnumerator, T, TComparer, TThenComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source, TThenComparer comparer)
            where TEnumerator : unmanaged, IEnumerator<T>
            where T : unmanaged, IComparable<T>
            where TComparer : unmanaged, IComparer<T>
            where TThenComparer : unmanaged, IComparer<T>
        {
            return new OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, TThenComparer>>(
                source._source,
                new ThenByComparer<T, TComparer, TThenComparer>(source._comparer, comparer));
        }

        /// <summary>
        /// Adds a secondary descending ordering to the query.
        /// </summary>
        /// <param name="source">The ordered source query.</param>
        /// <param name="comparer">Comparer used for the secondary ordering.</param>
        /// <returns>An ordered query that sorts by the existing ordering first and the new comparer second.</returns>
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, ReverseComparer<T, TThenComparer>>> ThenByDescending<TEnumerator, T, TComparer, TThenComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source, TThenComparer comparer)
            where TEnumerator : unmanaged, IEnumerator<T>
            where T : unmanaged, IComparable<T>
            where TComparer : unmanaged, IComparer<T>
            where TThenComparer : unmanaged, IComparer<T>
        {
            return source.ThenBy(new ReverseComparer<T, TThenComparer>(comparer));
        }
        
        /// <summary>
        /// Adds a secondary ascending ordering to the query.
        /// </summary>
        /// <param name="source">The ordered source query.</param>
        /// <param name="comparer">Comparer used for the secondary ordering.</param>
        /// <returns>An ordered query that sorts by the existing ordering first and the new comparer second.</returns>
        [NativeDelegateMethod(typeof(IComparer<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer>> ThenBy<TEnumerator, T, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source, Func<T, T, int> comparer)
            where TEnumerator : unmanaged, IEnumerator<T>
            where T : unmanaged, IComparable<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer>>>();
        }
        
        /// <summary>
        /// Adds a secondary descending ordering to the query.
        /// </summary>
        /// <param name="source">The ordered source query.</param>
        /// <param name="comparer">Comparer used for the secondary ordering.</param>
        /// <returns>An ordered query that sorts by the existing ordering first and the new comparer second.</returns>
        [NativeDelegateMethod(typeof(IComparer<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, ReverseComparer<T>>> ThenByDescending<TEnumerator, T, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source, Func<T, T, int> comparer)
            where TEnumerator : unmanaged, IEnumerator<T>
            where T : unmanaged, IComparable<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, ReverseComparer<T>>>>();
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

    public struct ThenByComparer<T, TPrimaryComparer> : IComparer<T>
        where T : unmanaged
        where TPrimaryComparer : unmanaged, IComparer<T>
    {
        public ThenByComparer(TPrimaryComparer primaryComparer)
        {
        }

        public int Compare(T x, T y)
        {
            return BLinqExtensions.ThrowCodeGen<int>();
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
    
    public struct ReverseComparer<T> : IComparer<T>
        where T : unmanaged
    {
        public int Compare(T x, T y)
        {
            return BLinqExtensions.ThrowCodeGen<int>();
        }
    }
}