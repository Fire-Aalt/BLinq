using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Orders the query in ascending order using the provided comparer.
        /// </summary>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <returns>An ordered query in ascending comparer order.</returns>
        public OrderedQuery<TEnumerator, T, TComparer> OrderBy<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            return new OrderedQuery<TEnumerator, T, TComparer>(this, comparer);
        }

        /// <summary>
        /// Orders the query in descending order using the provided comparer.
        /// </summary>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <returns>An ordered query in descending comparer order.</returns>
        public OrderedQuery<TEnumerator, T, ReverseComparer<T, TComparer>> OrderByDescending<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            return new OrderedQuery<TEnumerator, T, ReverseComparer<T, TComparer>>(this, new ReverseComparer<T, TComparer>(comparer));
        }

        /// <summary>
        /// Materializes the query into a sorted list using the provided comparer.
        /// </summary>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list using the provided comparer.</returns>
        public NativeList<T> ToOrderedBy<TComparer>(TComparer comparer, AllocatorManager.AllocatorHandle allocator)
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToNativeList(allocator);
            list.Sort(comparer);
            return list;
        }

        /// <summary>
        /// Materializes the query into a sorted list in reverse order using the provided comparer.
        /// </summary>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list using the reversed comparer order.</returns>
        public NativeList<T> ToOrderedByDescending<TComparer>(TComparer comparer, AllocatorManager.AllocatorHandle allocator)
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToNativeList(allocator);
            list.Sort(new ReverseComparer<T, TComparer>(comparer));
            return list;
        }
    }
        
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Orders the query in ascending order using the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>An ordered query in ascending element order.</returns>
        public static OrderedQuery<TEnumerator, T, AscendingComparer<T>> OrderBy<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.OrderBy(new AscendingComparer<T>());
        }

        /// <summary>
        /// Orders the query in descending order using the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>An ordered query in descending element order.</returns>
        public static OrderedQuery<TEnumerator, T, DescendingComparer<T>> OrderByDescending<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.OrderBy(new DescendingComparer<T>());
        }

        /// <summary>
        /// Materializes the query into a sorted list in ascending order using the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list in ascending element order.</returns>
        public static NativeList<T> ToOrderedBy<T, TEnumerator>(this Query<TEnumerator, T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ToOrderedBy(new AscendingComparer<T>(), allocator);
        }

        /// <summary>
        /// Materializes the query into a sorted list in descending order using the default comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list in descending element order.</returns>
        public static NativeList<T> ToOrderedByDescending<T, TEnumerator>(this Query<TEnumerator, T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ToOrderedBy(new DescendingComparer<T>(), allocator);
        }

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
        /// Materializes the query into a sorted list using the provided comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list using the provided comparer.</returns>
        public static NativeList<T> ToOrderedBy<T, TEnumerator, TComparer>(
            this Query<TEnumerator, T> source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, global::System.Collections.Generic.IComparer<T>
        {
            return source.ToOrderedBy(comparer, allocator);
        }

        /// <summary>
        /// Materializes the query into a sorted list in reverse order using the provided comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list using the reversed comparer order.</returns>
        public static NativeList<T> ToOrderedByDescending<T, TEnumerator, TComparer>(
            this Query<TEnumerator, T> source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, global::System.Collections.Generic.IComparer<T>
        {
            return source.ToOrderedByDescending(comparer, allocator);
        }

        /// <summary>
        /// Materializes the query into a sorted list using the provided comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list using the provided comparer.</returns>
        [NativeDelegateMethod(typeof(IComparer<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeList<T> ToOrderedBy<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, T, int> comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<NativeList<T>>();
        }

        /// <summary>
        /// Materializes the query into a sorted list in reverse order using the provided comparer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="comparer">Comparer used to order elements.</param>
        /// <param name="allocator">Allocator used for the resulting list.</param>
        /// <returns>A sorted list using the reversed comparer order.</returns>
        [NativeDelegateMethod(typeof(IComparer<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeList<T> ToOrderedByDescending<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, T, int> comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<NativeList<T>>();
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
