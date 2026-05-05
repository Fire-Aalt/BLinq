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
        /// Filters the query so that only elements matching <paramref name="predicate"/> are yielded.
        /// </summary>
        /// <param name="predicate">The predicate used to decide whether each element should be kept.</param>
        /// <returns>
        /// A new query that streams only the elements for which <paramref name="predicate"/> returns <c>true</c>.
        /// </returns>
        public Query<Where<TEnumerator, T, TPredicate>, T> Where<TPredicate>(TPredicate predicate)
            where TPredicate : unmanaged, IPredicate<T>
        {
            return new Query<Where<TEnumerator, T, TPredicate>, T>(
                new Where<TEnumerator, T, TPredicate>(GetEnumerator(), predicate));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Filters the query so that only elements matching <paramref name="predicate"/> are yielded.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to decide whether each element should be kept.</param>
        /// <returns>
        /// A new query that streams only the elements for which <paramref name="predicate"/> returns <c>true</c>.
        /// </returns>
        public static Query<Where<TEnumerator, T, TPredicate>, T> Where<T, TEnumerator, TPredicate>(
            this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return source.Where(predicate);
        }
        
        /// <summary>
        /// Filters the query so that only elements matching <paramref name="predicate"/> are yielded.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to decide whether each element should be kept.</param>
        /// <returns>
        /// A new query that streams only the elements for which <paramref name="predicate"/> returns <c>true</c>.
        /// </returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<Where<TEnumerator, T>, T> Where<T, TEnumerator>(
            this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Query<Where<TEnumerator, T>, T>>();
        }
    }

    public struct Where<TEnumerator, T, TPredicate> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
        where TPredicate : unmanaged, IPredicate<T>
    {
        private TEnumerator _source;
        private TPredicate _predicate;
        private T _current;

        public Where(TEnumerator source, TPredicate predicate)
        {
            _source = source;
            _predicate = predicate;
            _current = default;
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
            while (_source.MoveNext())
            {
                var value = _source.Current;
                if (!_predicate.Match(in value))
                {
                    continue;
                }

                _current = value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _source.Reset();
            _current = default;
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    }
    
    public struct Where<TEnumerator, T> : IEnumerator<T>
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
