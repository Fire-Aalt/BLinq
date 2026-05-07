using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the query elements, or a singleton <paramref name="defaultValue"/> when the query is empty.
        /// </summary>
        /// <param name="defaultValue">The value yielded when the source query is empty.</param>
        /// <returns>A query that yields the source elements, or <paramref name="defaultValue"/> when the source is empty.</returns>
        public Query<DefaultIfEmpty<TEnumerator, T>, T> DefaultIfEmpty(T defaultValue = default)
        {
            var hasKnownLength = TryGetLength(out var sourceLength);
            var length = hasKnownLength
                ? sourceLength == 0 ? 1 : sourceLength
                : -1;

            return new Query<DefaultIfEmpty<TEnumerator, T>, T>(
                new DefaultIfEmpty<TEnumerator, T>(GetEnumerator(), defaultValue, hasKnownLength, sourceLength),
                length);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the query elements, or a singleton default value when the query is empty.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>A query that yields the source elements, or default when the source is empty.</returns>
        public static Query<DefaultIfEmpty<TEnumerator, T>, T> DefaultIfEmpty<T, TEnumerator>(
            this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.DefaultIfEmpty();
        }

        /// <summary>
        /// Returns the query elements, or a singleton <paramref name="defaultValue"/> when the query is empty.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="defaultValue">The value yielded when the source query is empty.</param>
        /// <returns>A query that yields the source elements, or <paramref name="defaultValue"/> when the source is empty.</returns>
        public static Query<DefaultIfEmpty<TEnumerator, T>, T> DefaultIfEmpty<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            T defaultValue)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.DefaultIfEmpty(defaultValue);
        }
    }

    public struct DefaultIfEmpty<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private T _defaultValue;
        private T _current;
        private byte _state;

        public DefaultIfEmpty(TEnumerator source, T defaultValue)
            : this(source, defaultValue, false, 0)
        {
        }

        public DefaultIfEmpty(TEnumerator source, T defaultValue, bool hasKnownLength, int length)
        {
            _source = source;
            _defaultValue = defaultValue;
            _current = default;
            _state = hasKnownLength && length > 0 ? (byte)1 : (byte)0;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (_state == 3)
            {
                return false;
            }

            if (_state == 0)
            {
                if (_source.MoveNext())
                {
                    _current = _source.Current;
                    _state = 1;
                    return true;
                }

                _current = _defaultValue;
                _state = 2;
                return true;
            }

            if (_state == 1 && _source.MoveNext())
            {
                _current = _source.Current;
                return true;
            }

            _state = 3;
            return false;
        }

        public void Reset()
        {
            _source.Reset();
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    }
}
