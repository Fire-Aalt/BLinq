using System.Collections;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Concatenates this query with another query.
        /// </summary>
        /// <param name="second">The query whose elements are yielded after this query.</param>
        /// <returns>A query that yields all elements from this query followed by all elements from <paramref name="second"/>.</returns>
        public Query<Concat<TEnumerator, TSecondEnumerator, T>, T> Concat<TSecondEnumerator>(
            Query<TSecondEnumerator, T> second)
            where TSecondEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return new Query<Concat<TEnumerator, TSecondEnumerator, T>, T>(
                new Concat<TEnumerator, TSecondEnumerator, T>(
                    GetEnumerator(),
                    second.GetEnumerator()));
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
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TSecondEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Concat(second);
        }
    }

    public struct Concat<TEnumerator, TSecondEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where TSecondEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private TSecondEnumerator _second;
        private T _current;
        private byte _state;
        private int _firstLength;
        private bool _hasFirstLength;

        public Concat(TEnumerator source, TSecondEnumerator second)
        {
            _source = source;
            _second = second;
            _current = default;
            _state = 0;
            _hasFirstLength = source.TryGetNonEnumeratedCount(out _firstLength);
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

        public bool TryGetNonEnumeratedCount(out int count)
        {
            if (!_source.TryGetNonEnumeratedCount(out var firstCount) ||
                !_second.TryGetNonEnumeratedCount(out var secondCount))
            {
                count = 0;
                return false;
            }

            count = checked(firstCount + secondCount);
            return true;
        }

        public bool TryGetSpan(out System.ReadOnlySpan<T> span)
        {
            if (_source.TryGetNonEnumeratedCount(out var firstCount) && firstCount == 0)
            {
                return _second.TryGetSpan(out span);
            }

            if (_second.TryGetNonEnumeratedCount(out var secondCount) && secondCount == 0)
            {
                return _source.TryGetSpan(out span);
            }

            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            if (index < 0 || !_hasFirstLength)
            {
                return false;
            }

            if (index < _firstLength)
            {
                return _source.TryGetElementAt(index, out value);
            }

            return _second.TryGetElementAt(index - _firstLength, out value);
        }
    }
}
