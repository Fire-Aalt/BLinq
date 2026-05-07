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
        /// Prepends a value to the beginning of this query.
        /// </summary>
        /// <param name="element">The value to yield before all source elements.</param>
        /// <returns>A query that yields <paramref name="element"/> followed by the source elements.</returns>
        public Query<Prepend<TEnumerator, T>, T> Prepend(T element)
        {
            var length = TryGetLength(out var sourceLength) ? checked(sourceLength + 1) : -1;
            return new Query<Prepend<TEnumerator, T>, T>(
                new Prepend<TEnumerator, T>(GetEnumerator(), element),
                length);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Prepends a value to the beginning of a query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="element">The value to yield before all source elements.</param>
        /// <returns>A query that yields <paramref name="element"/> followed by the source elements.</returns>
        public static Query<Prepend<TEnumerator, T>, T> Prepend<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            T element)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Prepend(element);
        }
    }

    public struct Prepend<TEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private T _element;
        private T _current;
        private byte _state;

        public Prepend(TEnumerator source, T element)
        {
            _source = source;
            _element = element;
            _current = default;
            _state = 0;
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
                _current = _element;
                _state = 1;
                return true;
            }

            if (_state == 1 && _source.MoveNext())
            {
                _current = _source.Current;
                return true;
            }

            _state = 2;
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
            if (index < 0)
            {
                return false;
            }

            if (index == 0)
            {
                value = _element;
                return true;
            }

            return _source.TryGetElementAt(index - 1, out value);
        }
    }
}
