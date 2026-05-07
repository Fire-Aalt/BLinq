using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public struct Group<TKey, T>
        where TKey : unmanaged
        where T : unmanaged
    {
        private TKey _key;
        private NativeList<T> _values;

        public Group(TKey key, T value, AllocatorManager.AllocatorHandle allocator)
        {
            _key = key;
            _values = new NativeList<T>(1, allocator);
            _values.Add(value);
        }

        public Group(TKey key, AllocatorManager.AllocatorHandle allocator)
        {
            _key = key;
            _values = new NativeList<T>(0, allocator);
        }

        public TKey Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _key;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.Length;
        }

        public NativeArray<T> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.AsArray();
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Add(in T value)
        {
            _values.Add(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T>.Enumerator GetEnumerator()
        {
            return _values.AsArray().GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Query<NativeArrayQueryEnumerator<T>, T> AsQuery()
        {
            return new Query<NativeArrayQueryEnumerator<T>, T>(new NativeArrayQueryEnumerator<T>(_values.AsArray()), Length);
        }

        internal void Dispose()
        {
            if (_values.IsCreated)
            {
                _values.Dispose();
                _values = default;
            }
        }
    }

    public struct Lookup<TKey, T> : IDisposable
        where TKey : unmanaged
        where T : unmanaged
    {
        private NativeList<Group<TKey, T>> _groups;
        private int _valueCount;

        public Lookup(NativeList<Group<TKey, T>> groups, int valueCount)
        {
            _groups = groups;
            _valueCount = valueCount;
        }

        public bool IsCreated => _groups.IsCreated;

        public int GroupCount => _groups.Length;

        public int ValueCount => _valueCount;

        public NativeArray<Group<TKey, T>> Groups => _groups.AsArray();

        public Group<TKey, T> this[int index] => _groups[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Query<LookupEnumerator<TKey, T>, Group<TKey, T>> AsQuery()
        {
            return new Query<LookupEnumerator<TKey, T>, Group<TKey, T>>(GetEnumerator(), GroupCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LookupEnumerator<TKey, T> GetEnumerator()
        {
            return new LookupEnumerator<TKey, T>(_groups.AsArray());
        }

        public void Dispose()
        {
            if (!_groups.IsCreated)
            {
                return;
            }

            for (var i = 0; i < _groups.Length; i++)
            {
                ref var group = ref _groups.ElementAt(i);
                group.Dispose();
            }

            _groups.Dispose();
            _groups = default;
            _valueCount = 0;
        }
    }

    public struct LookupEnumerator<TKey, T> : IQueryEnumerator<Group<TKey, T>>
        where TKey : unmanaged
        where T : unmanaged
    {
        private NativeArray<Group<TKey, T>> _groups;
        private int _index;

        public LookupEnumerator(NativeArray<Group<TKey, T>> groups)
        {
            _groups = groups;
            _index = -1;
        }

        public Group<TKey, T> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _groups[_index];
        }

        object System.Collections.IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            _index++;
            return _index < _groups.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
        }

        public bool TryGetElementAt(int index, out Group<TKey, T> value)
        {
            if ((uint)index >= (uint)_groups.Length)
            {
                value = default;
                return false;
            }

            value = _groups[index];
            return true;
        }
    }
}
