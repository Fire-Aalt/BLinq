using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Yields a specified number of contiguous elements from the end of this query.
        /// </summary>
        /// <param name="count">The number of elements to yield from the end of the query.</param>
        /// <returns>A query that yields at most <paramref name="count"/> elements from the tail of the source.</returns>
        public Query<TakeLast<TEnumerator, T>, T> TakeLast(int count)
        {
            return new Query<TakeLast<TEnumerator, T>, T>(
                new TakeLast<TEnumerator, T>(GetEnumerator(), count));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Yields a specified number of contiguous elements from the end of a query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="count">The number of elements to yield from the end of the query.</param>
        /// <returns>A query that yields at most <paramref name="count"/> elements from the tail of the source.</returns>
        public static Query<TakeLast<TEnumerator, T>, T> TakeLast<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            int count)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.TakeLast(count);
        }
    }

    public struct TakeLast<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private NativeArray<T> _buffer;
        private int _count;
        private int _start;
        private int _length;
        private int _index;
        private T _current;
        private byte _state;

        public TakeLast(TEnumerator source, int count)
        {
            _source = source;
            _buffer = default;
            _count = count < 0 ? 0 : count;
            _start = 0;
            _length = 0;
            _index = 0;
            _current = default;
            _state = 0;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_state == 0)
            {
                InitializeBuffer();
            }

            if (_state == 2)
            {
                return false;
            }

            if (_index >= _length)
            {
                _state = 2;
                return false;
            }

            _current = _buffer[(_start + _index) % _count];
            _index++;
            return true;
        }

        public void Reset()
        {
            _source.Reset();
            _buffer = default;
            _start = 0;
            _length = 0;
            _index = 0;
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        private void InitializeBuffer()
        {
            if (_count <= 0)
            {
                _state = 2;
                return;
            }

            _buffer = new NativeArray<T>(_count, Allocator.Temp);
            var total = 0;
            while (_source.MoveNext())
            {
                _buffer[total % _count] = _source.Current;
                total++;
            }

            _length = total < _count ? total : _count;
            _start = total < _count ? 0 : total % _count;
            _index = 0;
            _state = _length == 0 ? (byte)2 : (byte)1;
        }
    }
}
