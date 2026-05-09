using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Projects each source element to an inner sequence and flattens the results into one query.
        /// </summary>
        /// <param name="selector">The function used to produce the inner enumerator for each source element.</param>
        /// <returns>
        /// A new query that yields all inner elements in source order, preserving the order within each inner sequence.
        /// </returns>
        public Query<SelectMany<TEnumerator, TInnerEnumerator, T, TResult, TSelector>, TResult> SelectMany<TResult, TInnerEnumerator, TSelector>(
            TSelector selector)
            where TResult : unmanaged
            where TInnerEnumerator : unmanaged, IEnumerator<TResult>
            where TSelector : unmanaged, ISelector<T, TInnerEnumerator>
        {
            return new Query<SelectMany<TEnumerator, TInnerEnumerator, T, TResult, TSelector>, TResult>(
                new SelectMany<TEnumerator, TInnerEnumerator, T, TResult, TSelector>(GetEnumerator(), selector));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Projects each source element to an inner sequence and flattens the results into one query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">The function used to produce the inner enumerator for each source element.</param>
        /// <returns>
        /// A new query that yields all inner elements in source order, preserving the order within each inner sequence.
        /// </returns>
        public static Query<SelectMany<TEnumerator, TInnerEnumerator, T, TResult, TSelector>, TResult> SelectMany<T, TResult, TEnumerator, TInnerEnumerator, TSelector>(
            this Query<TEnumerator, T> source,
            TSelector selector)
            where T : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TInnerEnumerator : unmanaged, IEnumerator<TResult>
            where TSelector : unmanaged, ISelector<T, TInnerEnumerator>
        {
            return source.SelectMany<TResult, TInnerEnumerator, TSelector>(selector);
        }
        
        /// <summary>
        /// Projects each source element to an inner sequence and flattens the results into one query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">The function used to produce the inner enumerator for each source element.</param>
        /// <returns>
        /// A new query that yields all inner elements in source order, preserving the order within each inner sequence.
        /// </returns>
        public static Query<SelectMany<TEnumerator, TInnerEnumerator, T, T, TSelector>, T> SelectMany<T, TEnumerator, TInnerEnumerator, TSelector>(
            this Query<TEnumerator, T> source,
            TSelector selector)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TInnerEnumerator : unmanaged, IEnumerator<T>
            where TSelector : unmanaged, ISelector<T, TInnerEnumerator>
        {
            return source.SelectMany<T, TInnerEnumerator, TSelector>(selector);
        }

        /// <summary>
        /// Projects each source element to an inner sequence and flattens the results into one query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">The function used to produce the inner enumerator for each source element.</param>
        /// <returns>
        /// A new query that yields all inner elements in source order, preserving the order within each inner sequence.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<SelectMany<TEnumerator, TInnerEnumerator, T, TResult>, TResult> SelectMany<T, TResult, TEnumerator, TInnerEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TInnerEnumerator> selector)
            where T : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TInnerEnumerator : unmanaged, IEnumerator<TResult>
        {
            return ThrowCodeGen<Query<SelectMany<TEnumerator, TInnerEnumerator, T, TResult>, TResult>>();
        }
        
        /// <summary>
        /// Projects each source element to an inner sequence and flattens the results into one query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">The function used to produce the inner enumerator for each source element.</param>
        /// <returns>
        /// A new query that yields all inner elements in source order, preserving the order within each inner sequence.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<SelectMany<TEnumerator, TInnerEnumerator, T, T>, T> SelectMany<T, TEnumerator, TInnerEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TInnerEnumerator> selector)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TInnerEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Query<SelectMany<TEnumerator, TInnerEnumerator, T, T>, T>>();
        }
    }

    public struct SelectMany<TSourceEnumerator, TInnerEnumerator, TSource, TResult, TSelector> : IQueryEnumerator<TResult>
        where TSourceEnumerator : unmanaged, IQueryEnumerator<TSource>
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

        public SelectMany(TSourceEnumerator source, TSelector selector)
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

        object IEnumerator.Current => Current;

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
    
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<TResult> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out TResult value)
        {
            value = default;
            return false;
        }
}

    public struct SelectMany<TSourceEnumerator, TInnerEnumerator, TSource, TResult> : IQueryEnumerator<TResult>
        where TSourceEnumerator : unmanaged, IQueryEnumerator<TSource>
        where TInnerEnumerator : unmanaged, IEnumerator<TResult>
        where TSource : unmanaged
        where TResult : unmanaged
    {
        public TResult Current => BLinqExtensions.ThrowCodeGen<TResult>();

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return BLinqExtensions.ThrowCodeGen<bool>();
        }

        public void Reset()
        {
            BLinqExtensions.ThrowCodeGen();
        }

        public void Dispose()
        {
        }
    
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<TResult> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out TResult value)
        {
            value = default;
            return false;
        }
}
}
