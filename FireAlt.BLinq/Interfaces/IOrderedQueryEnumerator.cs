using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public interface IOrderedQueryEnumerator<TEnumerator, T, TComparer> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
        where TComparer : unmanaged, IComparer<T>
    {
        Query<TEnumerator, T> Source { get; }
        TComparer Comparer { get; }
    }

    public struct OrderBy<TEnumerator, T, TComparer> : IOrderedQueryEnumerator<TEnumerator, T, TComparer>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
        where TComparer : unmanaged, IComparer<T>
    {
        private Query<TEnumerator, T> _source;
        private TComparer _comparer;
        private NativeList<T> _list;
        private int _index;
        private bool _initialized;

        public OrderBy(Query<TEnumerator, T> source, TComparer comparer)
        {
            _source = source;
            _comparer = comparer;
            _list = default;
            _index = -1;
            _initialized = false;
        }

        public Query<TEnumerator, T> Source
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source;
        }

        public TComparer Comparer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _comparer;
        }

        public T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _list[_index];
        }

        object System.Collections.IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (!_initialized)
            {
                _list = _source.ToNativeList(Allocator.Temp);
                BLinqUtilities.StableSort(_list, _comparer);
                _initialized = true;
            }

            _index++;
            return _index < _list.Length;
        }

        public void Reset()
        {
            if (_list.IsCreated)
            {
                _list.Dispose();
            }

            _list = default;
            _index = -1;
            _initialized = false;
        }

        public void Dispose()
        {
            if (_list.IsCreated)
            {
                _list.Dispose();
                _list = default;
            }
        }

        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            return false;
        }
    }
}
