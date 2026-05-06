using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns distinct elements from the query using default equality.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>A query that yields each distinct value the first time it appears.</returns>
        public static Query<Distinct<TEnumerator, T>, T> Distinct<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return new Query<Distinct<TEnumerator, T>, T>(new Distinct<TEnumerator, T>(source.GetEnumerator()));
        }
    }

    public struct Distinct<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged, IEquatable<T>
    {
        private TEnumerator _source;
        private UnsafeHashMapSlim<T, byte> _seen;
        private T _current;
        private bool _initialized;

        public Distinct(TEnumerator source)
        {
            _source = source;
            _seen = default;
            _current = default;
            _initialized = false;
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_initialized)
            {
                _seen = new UnsafeHashMapSlim<T, byte>(64, Allocator.Temp);
                _initialized = true;
            }

            while (_source.MoveNext())
            {
                var value = _source.Current;
                ref var marker = ref _seen.GetValueRefOrAddDefault(value, out var exists);
                if (exists)
                {
                    continue;
                }

                marker = 1;
                _current = value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            if (_initialized)
            {
                _seen.Dispose();
            }

            _source.Reset();
            _seen = default;
            _current = default;
            _initialized = false;
        }

        public void Dispose()
        {
            _source.Dispose();
            if (_initialized)
            {
                _seen.Dispose();
                _seen = default;
                _initialized = false;
            }
        }
    }
}
