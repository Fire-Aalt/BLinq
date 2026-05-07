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
        /// Yields elements while they match a predicate.
        /// </summary>
        /// <param name="predicate">The predicate used to decide whether each prefix element should be yielded.</param>
        /// <returns>A query that yields the source prefix where every element matches <paramref name="predicate"/>.</returns>
        public Query<TakeWhile<TEnumerator, T, TPredicate>, T> TakeWhile<TPredicate>(TPredicate predicate)
            where TPredicate : unmanaged, IPredicate<T>
        {
            return new Query<TakeWhile<TEnumerator, T, TPredicate>, T>(
                new TakeWhile<TEnumerator, T, TPredicate>(GetEnumerator(), predicate));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Yields elements while they match a predicate.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to decide whether each prefix element should be yielded.</param>
        /// <returns>A query that yields the source prefix where every element matches <paramref name="predicate"/>.</returns>
        public static Query<TakeWhile<TEnumerator, T, TPredicate>, T> TakeWhile<T, TEnumerator, TPredicate>(
            this Query<TEnumerator, T> source,
            TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return source.TakeWhile(predicate);
        }

        /// <summary>
        /// Yields elements while they match a predicate.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to decide whether each prefix element should be yielded.</param>
        /// <returns>A query that yields the source prefix where every element matches <paramref name="predicate"/>.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<TakeWhile<TEnumerator, T>, T> TakeWhile<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<Query<TakeWhile<TEnumerator, T>, T>>();
        }
    }

    public struct TakeWhile<TEnumerator, T, TPredicate> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
        where TPredicate : unmanaged, IPredicate<T>
    {
        private TEnumerator _source;
        private TPredicate _predicate;
        private T _current;
        private bool _done;

        public TakeWhile(TEnumerator source, TPredicate predicate)
        {
            _source = source;
            _predicate = predicate;
            _current = default;
            _done = false;
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
            if (_done || !_source.MoveNext())
            {
                _done = true;
                return false;
            }

            var value = _source.Current;
            if (!_predicate.Match(in value))
            {
                _done = true;
                return false;
            }

            _current = value;
            return true;
        }

        public void Reset()
        {
            _source.Reset();
            _current = default;
            _done = false;
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    
        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            return false;
        }
}

    public struct TakeWhile<TEnumerator, T> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        public T Current => BLinqExtensions.ThrowCodeGen<T>();

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
    
        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            return false;
        }
}
}
