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
        /// Orders the query in ascending order using a key selector and the default key comparer.
        /// </summary>
        public OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, AscendingComparer<TKey>>> OrderBy<TKey, TKeySelector>(
            TKeySelector keySelector)
            where TKey : unmanaged, IComparable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return OrderBy<TKey, TKeySelector, AscendingComparer<TKey>>(
                keySelector,
                new AscendingComparer<TKey>());
        }

        /// <summary>
        /// Orders the query in ascending order using a key selector and comparer.
        /// </summary>
        public OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>> OrderBy<TKey, TKeySelector, TKeyComparer>(
            TKeySelector keySelector,
            TKeyComparer comparer)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return new OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>>(
                this,
                new KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>(keySelector, comparer));
        }

        /// <summary>
        /// Orders the query in descending order using a key selector and the default key comparer.
        /// </summary>
        public OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, DescendingComparer<TKey>>> OrderByDescending<TKey, TKeySelector>(
            TKeySelector keySelector)
            where TKey : unmanaged, IComparable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return OrderBy<TKey, TKeySelector, DescendingComparer<TKey>>(
                keySelector,
                new DescendingComparer<TKey>());
        }

        /// <summary>
        /// Orders the query in descending order using a key selector and comparer.
        /// </summary>
        public OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, ReverseComparer<TKey, TKeyComparer>>> OrderByDescending<TKey, TKeySelector, TKeyComparer>(
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
        /// Orders the query in ascending order using the element as the key.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, T, IdentitySelector<T>, AscendingComparer<T>>> OrderBy<T, TEnumerator>(
            this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.OrderBy<T, IdentitySelector<T>>(new IdentitySelector<T>());
        }

        /// <summary>
        /// Orders the query in descending order using the element as the key.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, T, IdentitySelector<T>, DescendingComparer<T>>> OrderByDescending<T, TEnumerator>(
            this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.OrderByDescending<T, IdentitySelector<T>>(new IdentitySelector<T>());
        }

        /// <summary>
        /// Orders the query in ascending order using a key selector and the default key comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, AscendingComparer<TKey>>> OrderBy<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.OrderBy<TKey, TKeySelector>(keySelector);
        }

        /// <summary>
        /// Orders the query in ascending order using a key selector and comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, TKeyComparer>> OrderBy<T, TKey, TEnumerator, TKeySelector, TKeyComparer>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return source.OrderBy<TKey, TKeySelector, TKeyComparer>(keySelector, comparer);
        }

        /// <summary>
        /// Orders the query in descending order using a key selector and the default key comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, DescendingComparer<TKey>>> OrderByDescending<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.OrderByDescending<TKey, TKeySelector>(keySelector);
        }

        /// <summary>
        /// Orders the query in descending order using a key selector and comparer.
        /// </summary>
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeySelector, ReverseComparer<TKey, TKeyComparer>>> OrderByDescending<T, TKey, TEnumerator, TKeySelector, TKeyComparer>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return source.OrderByDescending<TKey, TKeySelector, TKeyComparer>(keySelector, comparer);
        }

        /// <summary>
        /// Materializes the query into a sorted list in ascending element order.
        /// </summary>
        public static NativeList<T> ToOrderedBy<T, TEnumerator>(this Query<TEnumerator, T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ToOrderedBy(new AscendingComparer<T>(), allocator);
        }

        /// <summary>
        /// Materializes the query into a sorted list in descending element order.
        /// </summary>
        public static NativeList<T> ToOrderedByDescending<T, TEnumerator>(this Query<TEnumerator, T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ToOrderedBy(new DescendingComparer<T>(), allocator);
        }

        /// <summary>
        /// Materializes the query into a sorted list using the provided comparer.
        /// </summary>
        public static NativeList<T> ToOrderedBy<T, TEnumerator, TComparer>(
            this Query<TEnumerator, T> source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return source.ToOrderedBy(comparer, allocator);
        }

        /// <summary>
        /// Materializes the query into a sorted list in reverse order using the provided comparer.
        /// </summary>
        public static NativeList<T> ToOrderedByDescending<T, TEnumerator, TComparer>(
            this Query<TEnumerator, T> source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return source.ToOrderedByDescending(comparer, allocator);
        }
        
        /// <summary>
        /// Orders the query in ascending order using a delegate key selector and the default key comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, AscendingComparer<TKey>>> OrderBy<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, AscendingComparer<TKey>>>>();
        }

        /// <summary>
        /// Orders the query in ascending order using a delegate key selector and comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeyComparer>> OrderBy<T, TKey, TEnumerator, TKeyComparer>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, TKeyComparer>>>();
        }

        /// <summary>
        /// Orders the query in descending order using a delegate key selector and the default key comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, DescendingComparer<TKey>>> OrderByDescending<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IComparable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, DescendingComparer<TKey>>>>();
        }

        /// <summary>
        /// Orders the query in descending order using a delegate key selector and comparer.
        /// </summary>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, ReverseComparer<TKey, TKeyComparer>>> OrderByDescending<T, TKey, TEnumerator, TKeyComparer>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            TKeyComparer comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeyComparer : unmanaged, IComparer<TKey>
        {
            return ThrowCodeGen<OrderedQuery<TEnumerator, T, KeySelectorComparer<T, TKey, ReverseComparer<TKey, TKeyComparer>>>>();
        }
        
        // /// <summary>
        // /// Materializes the query into a sorted list using the provided comparer.
        // /// </summary>
        // public static NativeList<T> ToOrderedBy<T, TKey, TEnumerator>(
        //     this Query<TEnumerator, T> source,
        //     Func<T, TKey> keySelector,
        //     AllocatorManager.AllocatorHandle allocator)
        //     where T : unmanaged
        //     where TKey : unmanaged
        //     where TEnumerator : unmanaged, IEnumerator<T>
        // {
        //     return source.ToOrderedBy(comparer, allocator);
        // }
        //
        // /// <summary>
        // /// Materializes the query into a sorted list in reverse order using the provided comparer.
        // /// </summary>
        // public static NativeList<T> ToOrderedByDescending<T, TKey, TEnumerator>(
        //     this Query<TEnumerator, T> source,
        //     Func<T, TKey> keySelector,
        //     AllocatorManager.AllocatorHandle allocator)
        //     where T : unmanaged
        //     where TKey : unmanaged
        //     where TEnumerator : unmanaged, IEnumerator<T>
        // {
        //     return source.ToOrderedByDescending(comparer, allocator);
        // }
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
