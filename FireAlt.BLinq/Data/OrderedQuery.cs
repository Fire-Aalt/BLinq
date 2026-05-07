using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace FireAlt.BLinq
{
    public struct OrderedQuery<TEnumerator, T, TComparer>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
        where TComparer : unmanaged, IComparer<T>
    {
        internal Query<TEnumerator, T> _source;
        internal TComparer _comparer;

        public OrderedQuery(Query<TEnumerator, T> source, TComparer comparer)
        {
            _source = source;
            _comparer = comparer;
        }
        
        public NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
        {
            return BLinqUtilities.ToSortedNativeArray<T, TEnumerator, TComparer>(
                _source.GetEnumerator(),
                _comparer,
                allocator);
        }

        public UnsafeList<T> ToUnsafeList(AllocatorManager.AllocatorHandle allocator)
        {
            return BLinqUtilities.ToSortedUnsafeList<T, TEnumerator, TComparer>(
                _source.GetEnumerator(),
                _comparer,
                allocator);
        }

        public NativeList<T> ToNativeList(AllocatorManager.AllocatorHandle allocator)
        {
            return BLinqUtilities.ToSortedNativeList<T, TEnumerator, TComparer>(
                _source.GetEnumerator(),
                _comparer,
                allocator);
        }

        public T[] ToManagedArray()
        {
            return BLinqUtilities.ToSortedManagedArray<T, TEnumerator, TComparer>(
                _source.GetEnumerator(),
                _comparer);
        }

        public List<T> ToManagedList()
        {
            return BLinqUtilities.ToSortedManagedList<T, TEnumerator, TComparer>(
                _source.GetEnumerator(),
                _comparer);
        }

        public Dictionary<TKey, T> ToManagedDictionary<TKey, TKeySelector>(TKeySelector keySelector)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToSortedManagedDictionary<T, TKey, T, TEnumerator, TComparer, TKeySelector, IdentitySelector<T>>(
                _source.GetEnumerator(),
                _comparer,
                keySelector,
                new IdentitySelector<T>(),
                null);
        }

        public Dictionary<TKey, T> ToManagedDictionary<TKey, TKeySelector>(
            TKeySelector keySelector,
            IEqualityComparer<TKey> comparer)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToSortedManagedDictionary<T, TKey, T, TEnumerator, TComparer, TKeySelector, IdentitySelector<T>>(
                _source.GetEnumerator(),
                _comparer,
                keySelector,
                new IdentitySelector<T>(),
                comparer);
        }

        public Dictionary<TKey, TValue> ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(
            TKeySelector keySelector,
            TValueSelector valueSelector)
            where TKey : unmanaged
            where TValue : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return BLinqUtilities.ToSortedManagedDictionary<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
                _source.GetEnumerator(),
                _comparer,
                keySelector,
                valueSelector,
                null);
        }

        public Dictionary<TKey, TValue> ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(
            TKeySelector keySelector,
            TValueSelector valueSelector,
            IEqualityComparer<TKey> comparer)
            where TKey : unmanaged
            where TValue : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return BLinqUtilities.ToSortedManagedDictionary<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
                _source.GetEnumerator(),
                _comparer,
                keySelector,
                valueSelector,
                comparer);
        }

        public HashSet<T> ToManagedHashSet()
        {
            return BLinqUtilities.ToSortedManagedHashSet<T, TEnumerator, TComparer>(
                _source.GetEnumerator(),
                _comparer,
                null);
        }

        public HashSet<T> ToManagedHashSet(IEqualityComparer<T> comparer)
        {
            return BLinqUtilities.ToSortedManagedHashSet<T, TEnumerator, TComparer>(
                _source.GetEnumerator(),
                _comparer,
                comparer);
        }

        public NativeHashMap<TKey, T> ToNativeHashMap<TKey, TKeySelector>(
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToSortedNativeHashMap<T, TKey, T, TEnumerator, TComparer, TKeySelector, IdentitySelector<T>>(
                _source.GetEnumerator(),
                _comparer,
                keySelector,
                new IdentitySelector<T>(),
                allocator);
        }

        public NativeHashMap<TKey, TValue> ToNativeHashMap<TKey, TValue, TKeySelector, TValueSelector>(
            TKeySelector keySelector,
            TValueSelector valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return BLinqUtilities.ToSortedNativeHashMap<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
                _source.GetEnumerator(),
                _comparer,
                keySelector,
                valueSelector,
                allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OrderedQueryEnumerator<T, TEnumerator, TComparer> GetEnumerator()
        {
            return new OrderedQueryEnumerator<T, TEnumerator, TComparer>(_source, _comparer);
        }
    }
    
    public struct OrderedQueryEnumerator<T, TEnumerator, TComparer> : IEnumerator<T>
        where T : unmanaged
        where TEnumerator : unmanaged, IEnumerator<T>
        where TComparer : unmanaged, IComparer<T>
    {
        private Query<TEnumerator, T> _source;
        private TComparer _comparer;
        private NativeList<T> _list;
        private int _index;
        private bool _initialized;

        public OrderedQueryEnumerator(Query<TEnumerator, T> source, TComparer comparer)
        {
            _source = source;
            _comparer = comparer;
            _list = default;
            _index = -1;
            _initialized = false;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list[_index];
        }

        object System.Collections.IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (!_initialized)
            {
                _list = _source.ToNativeList(Allocator.Temp);
                BLinqUtilities.StableSort(_list, _comparer);
                _initialized = true;
            }

            _index++;
            return _index < _list.Length;
        }

        public void Reset()
        {
            if (_list.IsCreated)
            {
                _list.Dispose();
            }

            _list = default;
            _index = -1;
            _initialized = false;
        }

        public void Dispose()
        {
            if (_list.IsCreated)
            {
                _list.Dispose();
                _list = default;
            }
        }
    }
}
