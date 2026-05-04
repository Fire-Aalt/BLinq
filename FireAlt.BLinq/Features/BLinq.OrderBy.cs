using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        public static OrderedQuery<TEnumerator, T, AscendingComparer<T>> OrderBy<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.OrderBy(new AscendingComparer<T>());
        }

        public static OrderedQuery<TEnumerator, T, DescendingComparer<T>> OrderByDescending<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.OrderBy(new DescendingComparer<T>());
        }

        public static NativeList<T> ToOrderedBy<T, TEnumerator>(this Query<TEnumerator, T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ToOrderedBy(new AscendingComparer<T>(), allocator);
        }

        public static NativeList<T> ToOrderedByDescending<T, TEnumerator>(this Query<TEnumerator, T> source, AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.ToOrderedBy(new DescendingComparer<T>(), allocator);
        }

        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, AscendingComparer<T>>> ThenBy<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return source.ThenBy(new AscendingComparer<T>());
        }

        public static OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, DescendingComparer<T>>> ThenByDescending<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged, IComparable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return source.ThenBy(new DescendingComparer<T>());
        }

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

    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public OrderedQuery<TEnumerator, T, TComparer> OrderBy<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            return new OrderedQuery<TEnumerator, T, TComparer>(this, comparer);
        }

        public OrderedQuery<TEnumerator, T, ReverseComparer<T, TComparer>> OrderByDescending<TComparer>(TComparer comparer)
            where TComparer : unmanaged, IComparer<T>
        {
            return new OrderedQuery<TEnumerator, T, ReverseComparer<T, TComparer>>(this, new ReverseComparer<T, TComparer>(comparer));
        }

        public NativeList<T> ToOrderedBy<TComparer>(TComparer comparer, AllocatorManager.AllocatorHandle allocator)
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToNativeList(allocator);
            list.Sort(comparer);
            return list;
        }

        public NativeList<T> ToOrderedByDescending<TComparer>(TComparer comparer, AllocatorManager.AllocatorHandle allocator)
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToNativeList(allocator);
            list.Sort(new ReverseComparer<T, TComparer>(comparer));
            return list;
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
