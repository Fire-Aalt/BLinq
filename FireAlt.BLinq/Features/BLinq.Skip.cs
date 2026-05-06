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
        /// Bypasses a specified number of elements and yields the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip.</param>
        /// <returns>A query that yields the elements after the skipped prefix.</returns>
        public Query<Skip<TEnumerator, T>, T> Skip(int count)
        {
            return new Query<Skip<TEnumerator, T>, T>(
                new Skip<TEnumerator, T>(GetEnumerator(), count));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Bypasses a specified number of elements and yields the remaining elements.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="count">The number of elements to skip.</param>
        /// <returns>A query that yields the elements after the skipped prefix.</returns>
        public static Query<Skip<TEnumerator, T>, T> Skip<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            int count)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Skip(count);
        }
    }

    public struct Skip<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private int _count;
        private int _remaining;
        private T _current;
        private bool _skipped;

        public Skip(TEnumerator source, int count)
        {
            _source = source;
            _count = count < 0 ? 0 : count;
            _remaining = count < 0 ? 0 : count;
            _current = default;
            _skipped = false;
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
            if (!_skipped)
            {
                while (_remaining > 0 && _source.MoveNext())
                {
                    _remaining--;
                }

                _skipped = true;
            }

            if (!_source.MoveNext())
            {
                return false;
            }

            _current = _source.Current;
            return true;
        }

        public void Reset()
        {
            _source.Reset();
            _remaining = _count;
            _current = default;
            _skipped = false;
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    }
}
