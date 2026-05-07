using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Produces the set union of two queries using default equality.
        /// </summary>
        /// <param name="source">The first query.</param>
        /// <param name="other">The query whose distinct elements are appended after <paramref name="source"/>.</param>
        /// <returns>A query that yields distinct values from both input queries in first occurrence order.</returns>
        public static Query<Union<TEnumerator, TOtherEnumerator, T>, T> Union<T, TEnumerator, TOtherEnumerator>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, T> other)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TOtherEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return new Query<Union<TEnumerator, TOtherEnumerator, T>, T>(
                new Union<TEnumerator, TOtherEnumerator, T>(source.GetEnumerator(), other.GetEnumerator()));
        }
    }

    public struct Union<TEnumerator, TOtherEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where TOtherEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged, IEquatable<T>
    {
        private TEnumerator _source;
        private TOtherEnumerator _other;
        private UnsafeHashMapSlim<T, byte> _seen;
        private T _current;
        private byte _state;

        public Union(TEnumerator source, TOtherEnumerator other)
        {
            _source = source;
            _other = other;
            _seen = default;
            _current = default;
            _state = 0;
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_state == 0)
            {
                _seen = new UnsafeHashMapSlim<T, byte>(64, Allocator.Temp);
                _state = 1;
            }

            while (_state == 1 && _source.MoveNext())
            {
                if (TryYield(_source.Current))
                {
                    return true;
                }
            }

            _state = 2;
            while (_other.MoveNext())
            {
                if (TryYield(_other.Current))
                {
                    return true;
                }
            }

            _state = 3;
            return false;
        }

        public void Reset()
        {
            if (_state != 0)
            {
                _seen.Dispose();
            }

            _source.Reset();
            _other.Reset();
            _seen = default;
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _source.Dispose();
            _other.Dispose();
            if (_state != 0)
            {
                _seen.Dispose();
                _seen = default;
                _state = 0;
            }
        }

        private bool TryYield(T value)
        {
            ref var marker = ref _seen.GetValueRefOrAddDefault(value, out var exists);
            if (exists)
            {
                return false;
            }

            marker = 1;
            _current = value;
            return true;
        }
    
        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            return false;
        }
}
}
