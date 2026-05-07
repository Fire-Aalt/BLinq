using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Produces the set difference of two queries using default equality.
        /// </summary>
        /// <param name="source">The query whose elements are returned.</param>
        /// <param name="other">The query whose values are excluded from <paramref name="source"/>.</param>
        /// <returns>A query that yields distinct values from <paramref name="source"/> that do not appear in <paramref name="other"/>.</returns>
        public static Query<Except<TEnumerator, TOtherEnumerator, T>, T> Except<T, TEnumerator, TOtherEnumerator>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, T> other)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TOtherEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return new Query<Except<TEnumerator, TOtherEnumerator, T>, T>(
                new Except<TEnumerator, TOtherEnumerator, T>(source.GetEnumerator(), other.GetEnumerator()));
        }
    }

    public struct Except<TEnumerator, TOtherEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where TOtherEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged, IEquatable<T>
    {
        private TEnumerator _source;
        private TOtherEnumerator _other;
        private UnsafeHashMapSlim<T, byte> _set;
        private T _current;
        private bool _initialized;

        public Except(TEnumerator source, TOtherEnumerator other)
        {
            _source = source;
            _other = other;
            _set = default;
            _current = default;
            _initialized = false;
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_initialized)
            {
                _set = new UnsafeHashMapSlim<T, byte>(64, Allocator.Temp);
                while (_other.MoveNext())
                {
                    ref var marker = ref _set.GetValueRefOrAddDefault(_other.Current, out var exists);
                    if (!exists)
                    {
                        marker = 1;
                    }
                }

                _initialized = true;
            }

            while (_source.MoveNext())
            {
                var value = _source.Current;
                ref var marker = ref _set.GetValueRefOrAddDefault(value, out var exists);
                if (exists)
                {
                    continue;
                }

                marker = 2;
                _current = value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            if (_initialized)
            {
                _set.Dispose();
            }

            _source.Reset();
            _other.Reset();
            _set = default;
            _current = default;
            _initialized = false;
        }

        public void Dispose()
        {
            _source.Dispose();
            _other.Dispose();
            if (_initialized)
            {
                _set.Dispose();
                _set = default;
                _initialized = false;
            }
        }
    
        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            return false;
        }
}
}
