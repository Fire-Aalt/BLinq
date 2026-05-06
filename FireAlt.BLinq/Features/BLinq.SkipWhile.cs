using System;
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
        /// Bypasses elements while they match a predicate, then yields the remaining elements.
        /// </summary>
        /// <param name="predicate">The predicate used to decide whether each prefix element should be skipped.</param>
        /// <returns>A query that yields the source after the first element that does not match <paramref name="predicate"/>.</returns>
        public Query<SkipWhile<TEnumerator, T, TPredicate>, T> SkipWhile<TPredicate>(TPredicate predicate)
            where TPredicate : unmanaged, IPredicate<T>
        {
            return new Query<SkipWhile<TEnumerator, T, TPredicate>, T>(
                new SkipWhile<TEnumerator, T, TPredicate>(GetEnumerator(), predicate));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Bypasses elements while they match a predicate, then yields the remaining elements.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to decide whether each prefix element should be skipped.</param>
        /// <returns>A query that yields the source after the first element that does not match <paramref name="predicate"/>.</returns>
        public static Query<SkipWhile<TEnumerator, T, TPredicate>, T> SkipWhile<T, TEnumerator, TPredicate>(
            this Query<TEnumerator, T> source,
            TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return source.SkipWhile(predicate);
        }

        /// <summary>
        /// Bypasses elements while they match a predicate, then yields the remaining elements.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to decide whether each prefix element should be skipped.</param>
        /// <returns>A query that yields the source after the first element that does not match <paramref name="predicate"/>.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<SkipWhile<TEnumerator, T>, T> SkipWhile<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Query<SkipWhile<TEnumerator, T>, T>>();
        }
    }

    public struct SkipWhile<TEnumerator, T, TPredicate> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
        where TPredicate : unmanaged, IPredicate<T>
    {
        private TEnumerator _source;
        private TPredicate _predicate;
        private T _current;
        private bool _skipped;

        public SkipWhile(TEnumerator source, TPredicate predicate)
        {
            _source = source;
            _predicate = predicate;
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
                while (_source.MoveNext())
                {
                    var value = _source.Current;
                    if (_predicate.Match(in value))
                    {
                        continue;
                    }

                    _current = value;
                    _skipped = true;
                    return true;
                }

                _skipped = true;
                return false;
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
            _current = default;
            _skipped = false;
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    }

    public struct SkipWhile<TEnumerator, T> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
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
    }
}
