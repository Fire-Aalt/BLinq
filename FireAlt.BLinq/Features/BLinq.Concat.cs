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
        /// Concatenates this query with another query.
        /// </summary>
        /// <param name="second">The query whose elements are yielded after this query.</param>
        /// <returns>A query that yields all elements from this query followed by all elements from <paramref name="second"/>.</returns>
        public Query<Concat<TEnumerator, TSecondEnumerator, T>, T> Concat<TSecondEnumerator>(
            Query<TSecondEnumerator, T> second)
            where TSecondEnumerator : unmanaged, IEnumerator<T>
        {
            return new Query<Concat<TEnumerator, TSecondEnumerator, T>, T>(
                new Concat<TEnumerator, TSecondEnumerator, T>(GetEnumerator(), second.GetEnumerator()));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Concatenates two queries.
        /// </summary>
        /// <param name="source">The first query.</param>
        /// <param name="second">The query whose elements are yielded after <paramref name="source"/>.</param>
        /// <returns>A query that yields all elements from <paramref name="source"/> followed by all elements from <paramref name="second"/>.</returns>
        public static Query<Concat<TEnumerator, TSecondEnumerator, T>, T> Concat<T, TEnumerator, TSecondEnumerator>(
            this Query<TEnumerator, T> source,
            Query<TSecondEnumerator, T> second)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TSecondEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Concat(second);
        }
    }

    public struct Concat<TEnumerator, TSecondEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where TSecondEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private TSecondEnumerator _second;
        private T _current;
        private byte _state;

        public Concat(TEnumerator source, TSecondEnumerator second)
        {
            _source = source;
            _second = second;
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
                if (_source.MoveNext())
                {
                    _current = _source.Current;
                    return true;
                }

                _state = 1;
            }

            if (_state == 1 && _second.MoveNext())
            {
                _current = _second.Current;
                return true;
            }

            _state = 2;
            return false;
        }

        public void Reset()
        {
            _source.Reset();
            _second.Reset();
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _source.Dispose();
            _second.Dispose();
        }
    }
}
