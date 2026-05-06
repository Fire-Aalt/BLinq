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
        /// Yields a specified number of contiguous elements from the start of this query.
        /// </summary>
        /// <param name="count">The number of elements to yield.</param>
        /// <returns>A query that yields at most <paramref name="count"/> elements.</returns>
        public Query<Take<TEnumerator, T>, T> Take(int count)
        {
            return new Query<Take<TEnumerator, T>, T>(
                new Take<TEnumerator, T>(GetEnumerator(), count));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Yields a specified number of contiguous elements from the start of a query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="count">The number of elements to yield.</param>
        /// <returns>A query that yields at most <paramref name="count"/> elements.</returns>
        public static Query<Take<TEnumerator, T>, T> Take<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            int count)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Take(count);
        }
    }

    public struct Take<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private int _count;
        private int _remaining;
        private T _current;

        public Take(TEnumerator source, int count)
        {
            _source = source;
            _count = count < 0 ? 0 : count;
            _remaining = count < 0 ? 0 : count;
            _current = default;
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
            if (_remaining <= 0 || !_source.MoveNext())
            {
                return false;
            }

            _remaining--;
            _current = _source.Current;
            return true;
        }

        public void Reset()
        {
            _source.Reset();
            _remaining = _count;
            _current = default;
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    }
}
