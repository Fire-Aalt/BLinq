using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace FireAlt.BLinq
{
    internal unsafe struct UnsafeHashMapSlim<TKey, TValue> : IDisposable
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        private const int MINIMUM_CAPACITY = 16;
        private const float LOAD_FACTOR = 0.72f;

        private Entry* _entries;
        private int* _buckets;
        private int _capacity;
        private int _count;
        private int _resizeThreshold;
        private Allocator _allocator;

        public UnsafeHashMapSlim(int capacity, Allocator allocator)
        {
            _allocator = allocator;
            
            _capacity = math.ceilpow2(math.max(capacity, MINIMUM_CAPACITY));
            _count = 0;
            _resizeThreshold = (int)(_capacity * LOAD_FACTOR);

            _entries = (Entry*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<Entry>() * _capacity,
                UnsafeUtility.AlignOf<Entry>(),
                allocator);
            _buckets = (int*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<int>() * _capacity,
                UnsafeUtility.AlignOf<int>(),
                allocator);

            UnsafeUtility.MemClear(_buckets, UnsafeUtility.SizeOf<int>() * _capacity);
        }

        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count;
        }

        public ref TValue GetValueRefOrAddDefault(TKey key, out bool exists)
        {
            var hashCode = InternalGetHashCode(in key);
            ref var bucket = ref _buckets[GetBucketIndex(hashCode)];
            var index = bucket - 1;

            while (index >= 0)
            {
                ref var entry = ref _entries[index];
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                {
                    exists = true;
                    return ref entry.Value;
                }

                index = entry.Next;
            }

            exists = false;
            if (_count >= _resizeThreshold)
            {
                Resize();
                bucket = ref _buckets[GetBucketIndex(hashCode)];
            }

            ref var newEntry = ref _entries[_count];
            newEntry.HashCode = hashCode;
            newEntry.Key = key;
            newEntry.Value = default;
            newEntry.Next = bucket - 1;

            bucket = _count + 1;
            _count++;

            return ref newEntry.Value;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var hashCode = InternalGetHashCode(in key);
            var index = _buckets[GetBucketIndex(hashCode)] - 1;

            while (index >= 0)
            {
                ref var entry = ref _entries[index];
                if (entry.HashCode == hashCode && entry.Key.Equals(key))
                {
                    value = entry.Value;
                    return true;
                }

                index = entry.Next;
            }

            value = default;
            return false;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_entries, _count);
        }

        public void Dispose()
        {
            if (_buckets != null)
            {
                UnsafeUtility.Free(_buckets, _allocator);
                _buckets = null;
            }

            if (_entries != null)
            {
                UnsafeUtility.Free(_entries, _allocator);
                _entries = null;
            }

            _capacity = 0;
            _count = 0;
            _resizeThreshold = 0;
            _allocator = Allocator.Invalid;
        }

        private void Resize()
        {
            var newCapacity = _capacity * 2;
            var newEntries = (Entry*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<Entry>() * newCapacity,
                UnsafeUtility.AlignOf<Entry>(),
                _allocator);
            var newBuckets = (int*)UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<int>() * newCapacity,
                UnsafeUtility.AlignOf<int>(),
                _allocator);

            UnsafeUtility.MemCpy(newEntries, _entries, UnsafeUtility.SizeOf<Entry>() * _count);
            UnsafeUtility.MemClear(newBuckets, UnsafeUtility.SizeOf<int>() * newCapacity);

            var oldEntries = _entries;
            var oldBuckets = _buckets;
            _entries = newEntries;
            _buckets = newBuckets;
            _capacity = newCapacity;
            _resizeThreshold = (int)(_capacity * LOAD_FACTOR);

            for (var i = 0; i < _count; i++)
            {
                ref var entry = ref _entries[i];
                ref var bucket = ref _buckets[GetBucketIndex(entry.HashCode)];
                entry.Next = bucket - 1;
                bucket = i + 1;
            }

            UnsafeUtility.Free(oldEntries, _allocator);
            UnsafeUtility.Free(oldBuckets, _allocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetBucketIndex(uint hashCode)
        {
            return (int)(hashCode & (uint)(_capacity - 1));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint InternalGetHashCode(in TKey key)
        {
            return (uint)(key.GetHashCode() & 0x7FFFFFFF);
        }

        internal struct Entry
        {
            public uint HashCode;
            public int Next;
            public TKey Key;
            public TValue Value;
        }

        internal struct Enumerator
        {
            private readonly Entry* _entries;
            private readonly int _count;
            private int _index;

            internal Enumerator(Entry* entries, int count)
            {
                _entries = entries;
                _count = count;
                _index = 0;
            }

            public bool TryGetNext(out KeyValuePair<TKey, TValue> current)
            {
                if (_index < _count)
                {
                    ref var entry = ref _entries[_index];
                    _index++;
                    current = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
                    return true;
                }

                current = default;
                return false;
            }
        }
    }
}
