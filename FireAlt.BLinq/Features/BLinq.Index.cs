using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Incorporates the element's index into a value tuple.
        /// </summary>
        /// <returns>A query that incorporates the element's index into a tuple.</returns>
        public Query<Index<TEnumerator, T>, IndexTuple<T>> Index()
        {
            return new Query<Index<TEnumerator, T>, IndexTuple<T>>(
                new Index<TEnumerator, T>(GetEnumerator()));
        }
    }

    public struct IndexTuple<T> 
        where T : unmanaged
    {
        public int Index;
        public T Item;
        
        public IndexTuple(int index, T item)
        {
            Index = index;
            Item = item;
        }
    }

    public struct Index<TEnumerator, T> : IQueryEnumerator<IndexTuple<T>>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private TEnumerator _source;
        private IndexTuple<T> _current;
        private int _index;

        public Index(TEnumerator source)
        {
            _source = source;
            _current = default;
            _index = 0;
        }

        public IndexTuple<T> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_source.MoveNext())
            {
                _current = new IndexTuple<T>(_index, _source.Current);
                _index++;
                return true;
            }
            return false;
        }

        public void Reset()
        {
            _source.Reset();
            _current = default;
            _index = 0;
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
            count = sourceCount;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<IndexTuple<T>> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out IndexTuple<T> value)
        {
            if (_source.TryGetElementAt(index, out var element))
            {
                value = new IndexTuple<T>(index, element);
                return false;
            }
            value = default;
            return false;
        }
    }
}