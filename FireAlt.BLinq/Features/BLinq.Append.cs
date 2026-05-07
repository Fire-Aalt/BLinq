using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Appends a value to the end of this query.
        /// </summary>
        /// <param name="element">The value to yield after all source elements.</param>
        /// <returns>A query that yields the source elements followed by <paramref name="element"/>.</returns>
        public Query<Append<TEnumerator, T>, T> Append(T element)
        {
            var length = TryGetLength(out var sourceLength) ? checked(sourceLength + 1) : -1;
            return new Query<Append<TEnumerator, T>, T>(
                new Append<TEnumerator, T>(GetEnumerator(), element, TryGetLength(out sourceLength), sourceLength),
                length);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Appends a value to the end of a query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="element">The value to yield after all source elements.</param>
        /// <returns>A query that yields the source elements followed by <paramref name="element"/>.</returns>
        public static Query<Append<TEnumerator, T>, T> Append<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            T element)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Append(element);
        }
    }

    public struct Append<TEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private T _element;
        private T _current;
        private byte _state;
        private int _sourceLength;
        private bool _hasKnownLength;

        public Append(TEnumerator source, T element)
        {
            _source = source;
            _element = element;
            _current = default;
            _state = 0;
            _sourceLength = 0;
            _hasKnownLength = false;
        }

        public Append(TEnumerator source, T element, bool hasKnownLength, int sourceLength)
        {
            _source = source;
            _element = element;
            _current = default;
            _state = 0;
            _sourceLength = sourceLength;
            _hasKnownLength = hasKnownLength;
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
            if (_state == 0)
            {
                if (_source.MoveNext())
                {
                    _current = _source.Current;
                    return true;
                }

                _state = 1;
            }

            if (_state == 1)
            {
                _current = _element;
                _state = 2;
                return true;
            }

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

        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            if (index < 0 || !_hasKnownLength)
            {
                return false;
            }

            if (index == _sourceLength)
            {
                value = _element;
                return true;
            }

            if (index < _sourceLength)
            {
                return _source.TryGetElementAt(index, out value);
            }

            return false;
        }
    }
}
