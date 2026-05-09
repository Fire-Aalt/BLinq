using System.Collections;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
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
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.TakeLast(count);
        }
    }

    public struct TakeLast<TEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private NativeArray<T> _buffer;
        private int _count;
        private int _index;
        private int _remaining;
        private int _skipRemaining;
        private int _sourceLength;
        private T _current;
        private byte _state;
        private bool _hasKnownLength;
        private int _resultLength;

        public TakeLast(TEnumerator source, int count)
        {
            _source = source;
            _buffer = default;
            _count = count < 0 ? 0 : count;
            _index = 0;
            _remaining = 0;
            _skipRemaining = 0;
            _sourceLength = 0;
            _current = default;
            _state = 0;
            _hasKnownLength = source.TryGetNonEnumeratedCount(out _sourceLength);
            _resultLength = _hasKnownLength ? _sourceLength < _count ? _sourceLength : _count : -1;
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

            if (_state == 3)
            {
                return MoveNextKnownLength();
            }

            if (_state == 4)
            {
                return MoveNextIndexed();
            }

            if (_remaining <= 0)
            {
                _state = 2;
                return false;
            }

            _current = _buffer[_index];
            _index++;
            if (_index == _count)
            {
                _index = 0;
            }

            _remaining--;
            return true;
        }

        public void Reset()
        {
            if (_buffer.IsCreated)
            {
                _buffer.Dispose();
            }

            _source.Reset();
            _buffer = default;
            _index = 0;
            _remaining = 0;
            _skipRemaining = 0;
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _source.Dispose();
            if (_buffer.IsCreated)
            {
                _buffer.Dispose();
                _buffer = default;
            }
        }

        private void InitializeBuffer()
        {
            if (_count <= 0)
            {
                _state = 2;
                return;
            }

            if (_hasKnownLength)
            {
                _remaining = _sourceLength < _count ? _sourceLength : _count;
                _skipRemaining = _sourceLength - _remaining;
                if (_source.TryGetElementAt(0, out _))
                {
                    _index = _skipRemaining;
                    _state = _remaining == 0 ? (byte)2 : (byte)4;
                }
                else
                {
                    _state = _remaining == 0 ? (byte)2 : (byte)3;
                }
                return;
            }

            _buffer = new NativeArray<T>(_count, Allocator.Temp);
            var writeIndex = 0;
            var total = 0;
            while (_source.MoveNext())
            {
                _buffer[writeIndex] = _source.Current;
                writeIndex++;
                if (writeIndex == _count)
                {
                    writeIndex = 0;
                }

                total++;
            }

            _remaining = total < _count ? total : _count;
            _index = total < _count ? 0 : writeIndex;
            _state = _remaining == 0 ? (byte)2 : (byte)1;
        }

        private bool MoveNextKnownLength()
        {
            while (_skipRemaining > 0 && _source.MoveNext())
            {
                _skipRemaining--;
            }

            if (_remaining <= 0 || !_source.MoveNext())
            {
                _state = 2;
                return false;
            }

            _current = _source.Current;
            _remaining--;
            return true;
        }

        private bool MoveNextIndexed()
        {
            if (_remaining <= 0)
            {
                _state = 2;
                return false;
            }

            if (!_source.TryGetElementAt(_index, out _current))
            {
                _state = 2;
                return false;
            }

            _index++;
            _remaining--;
            return true;
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            if (!_source.TryGetNonEnumeratedCount(out var sourceCount))
            {
                count = 0;
                return false;
            }

            count = sourceCount < _count ? sourceCount : _count;
            return true;
        }

        public bool TryGetSpan(out System.ReadOnlySpan<T> span)
        {
            if (!_source.TryGetSpan(out var sourceSpan))
            {
                span = default;
                return false;
            }

            var count = sourceSpan.Length < _count ? sourceSpan.Length : _count;
            span = sourceSpan.Slice(sourceSpan.Length - count, count);
            return true;
        }

        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            if (index < 0 ||
                _resultLength < 0 ||
                index >= _resultLength)
            {
                return false;
            }

            return _source.TryGetElementAt(_sourceLength - _resultLength + index, out value);
        }
    }

}
