using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using KrasCore;

namespace FireAlt.BLinq
{
    public struct OrderedQuery<TEnumerator, T, TComparer>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
        where TComparer : unmanaged, IComparer<T>
    {
        private Query<TEnumerator, T> _source;
        private TComparer _comparer;

        public OrderedQuery(Query<TEnumerator, T> source, TComparer comparer)
        {
            _source = source;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, TThenComparer>> ThenBy<TThenComparer>(TThenComparer comparer)
            where TThenComparer : unmanaged, IComparer<T>
        {
            return new OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, TThenComparer>>(
                _source,
                new ThenByComparer<T, TComparer, TThenComparer>(_comparer, comparer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public OrderedQuery<TEnumerator, T, ThenByComparer<T, TComparer, ReverseComparer<T, TThenComparer>>> ThenByDescending<TThenComparer>(TThenComparer comparer)
            where TThenComparer : unmanaged, IComparer<T>
        {
            return ThenBy(new ReverseComparer<T, TThenComparer>(comparer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
        {
            var array = _source.ToNativeArray(allocator);
            array.Sort(_comparer);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe UnsafeArray<T> ToUnsafeArray(Allocator allocator)
        {
            var array = _source.ToUnsafeArray(allocator);
            NativeSortExtension.Sort((T*)array.GetUnsafePtr(), array.Length, _comparer);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnsafeList<T> ToUnsafeList(AllocatorManager.AllocatorHandle allocator)
        {
            var list = _source.ToUnsafeList(allocator);
            list.Sort(_comparer);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList<T> ToNativeList(AllocatorManager.AllocatorHandle allocator)
        {
            var list = _source.ToNativeList(allocator);
            list.Sort(_comparer);
            return list;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T[] ToManagedArray()
        {
            var list = ToNativeList(Allocator.Temp);
            return list.ToManagedArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public List<T> ToManagedList()
        {
            var array = ToManagedArray();
            return new List<T>(array);
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
                _list.Sort(_comparer);
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