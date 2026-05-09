using System;
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
        /// Splits the query into contiguous chunks of up to <paramref name="size"/> elements.
        /// </summary>
        /// <param name="size">The maximum number of elements in each chunk.</param>
        /// <returns>A query that yields chunk views backed by temporary native storage.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than one.</exception>
        public Query<Chunk<TEnumerator, T>, Chunk<T>> Chunk(int size)
        {
            if (size < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            return new Query<Chunk<TEnumerator, T>, Chunk<T>>(
                new Chunk<TEnumerator, T>(GetEnumerator(), size));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Splits a query into contiguous chunks of up to <paramref name="size"/> elements.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="size">The maximum number of elements in each chunk.</param>
        /// <returns>A query that yields chunk views backed by temporary native storage.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="size"/> is less than one.</exception>
        public static Query<Chunk<TEnumerator, T>, Chunk<T>> Chunk<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            int size)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Chunk(size);
        }
    }

    public struct Chunk<T>
        where T : unmanaged
    {
        private NativeArray<T> _values;

        public Chunk(NativeArray<T> values)
        {
            _values = values;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values.Length;
        }

        public NativeArray<T> Values
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values;
        }

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeArray<T>.Enumerator GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Query<NativeArrayQueryEnumerator<T>, T> AsQuery()
        {
            return new Query<NativeArrayQueryEnumerator<T>, T>(new NativeArrayQueryEnumerator<T>(_values));
        }
    }

    public struct Chunk<TEnumerator, T> : IQueryEnumerator<Chunk<T>>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private int _size;
        private Chunk<T> _current;
        private bool _finished;

        public Chunk(TEnumerator source, int size)
        {
            _source = source;
            _size = size;
            _current = default;
            _finished = false;
        }

        public Chunk<T> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_finished)
            {
                return false;
            }

            var values = new NativeList<T>(_size, Allocator.Temp);
            while (values.Length < _size && _source.MoveNext())
            {
                values.Add(_source.Current);
            }

            if (values.Length == 0)
            {
                _finished = true;
                return false;
            }

            _current = new Chunk<T>(values.AsArray());
            if (values.Length < _size)
            {
                _finished = true;
            }

            return true;
        }

        public void Reset()
        {
            _source.Reset();
            _current = default;
            _finished = false;
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            if (!_source.TryGetNonEnumeratedCount(out var sourceCount))
            {
                count = 0;
                return false;
            }

            count = sourceCount == 0 ? 0 : (sourceCount + _size - 1) / _size;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<Chunk<T>> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out Chunk<T> value)
        {
            value = default;
            return false;
        }
    }
}
