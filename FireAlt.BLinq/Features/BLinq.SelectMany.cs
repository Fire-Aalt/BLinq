using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public Query<SelectManyQuery<TEnumerator, TInnerEnumerator, T, TResult, TSelector>, TResult> SelectMany<TResult, TInnerEnumerator, TSelector>(
            TSelector selector)
            where TResult : unmanaged
            where TInnerEnumerator : unmanaged, IEnumerator<TResult>
            where TSelector : unmanaged, ISelector<T, TInnerEnumerator>
        {
            return new Query<SelectManyQuery<TEnumerator, TInnerEnumerator, T, TResult, TSelector>, TResult>(
                new SelectManyQuery<TEnumerator, TInnerEnumerator, T, TResult, TSelector>(GetEnumerator(), selector));
        }
    }

    public struct SelectManyQuery<TSourceEnumerator, TInnerEnumerator, TSource, TResult, TSelector> : IEnumerator<TResult>
        where TSourceEnumerator : unmanaged, IEnumerator<TSource>
        where TInnerEnumerator : unmanaged, IEnumerator<TResult>
        where TSource : unmanaged
        where TResult : unmanaged
        where TSelector : unmanaged, ISelector<TSource, TInnerEnumerator>
    {
        private TSourceEnumerator _source;
        private TInnerEnumerator _inner;
        private TSelector _selector;
        private TResult _current;
        private bool _hasInner;

        public SelectManyQuery(TSourceEnumerator source, TSelector selector)
        {
            _source = source;
            _inner = default;
            _selector = selector;
            _current = default;
            _hasInner = false;
        }

        public TResult Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        object System.Collections.IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (true)
            {
                if (_hasInner && _inner.MoveNext())
                {
                    _current = _inner.Current;
                    return true;
                }

                if (_hasInner)
                {
                    _inner.Dispose();
                }

                if (!_source.MoveNext())
                {
                    _hasInner = false;
                    return false;
                }

                var value = _source.Current;
                _inner = _selector.Select(in value);
                _hasInner = true;
            }
        }

        public void Reset()
        {
            if (_hasInner)
            {
                _inner.Dispose();
            }

            _source.Reset();
            _inner = default;
            _current = default;
            _hasInner = false;
        }

        public void Dispose()
        {
            if (_hasInner)
            {
                _inner.Dispose();
            }

            _source.Dispose();
        }
    }
}
