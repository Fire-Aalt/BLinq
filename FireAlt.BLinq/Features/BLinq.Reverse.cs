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
        /// Yields the elements of this query in reverse order.
        /// </summary>
        /// <returns>A query that yields the source elements in reverse order.</returns>
        public Query<Reverse<TEnumerator, T>, T> Reverse()
        {
            return new Query<Reverse<TEnumerator, T>, T>(new Reverse<TEnumerator, T>(GetEnumerator()));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Yields the elements of a query in reverse order.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>A query that yields the source elements in reverse order.</returns>
        public static Query<Reverse<TEnumerator, T>, T> Reverse<T, TEnumerator>(
            this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.Reverse();
        }
    }

    public struct Reverse<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private NativeList<T> _values;
        private int _index;
        private T _current;
        private byte _state;

        public Reverse(TEnumerator source)
        {
            _source = source;
            _values = default;
            _index = -1;
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
                _values = BLinqUtilities.ToNativeList<T, TEnumerator>(_source, Allocator.Temp);
                _index = _values.Length - 1;
                _state = 1;
            }

            if (_index < 0)
            {
                return false;
            }

            _current = _values[_index];
            _index--;
            return true;
        }

        public void Reset()
        {
            if (_values.IsCreated)
            {
                _values.Dispose();
            }

            _source.Reset();
            _values = default;
            _index = -1;
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _source.Dispose();
            if (_values.IsCreated)
            {
                _values.Dispose();
                _values = default;
            }
        }
    }
}
