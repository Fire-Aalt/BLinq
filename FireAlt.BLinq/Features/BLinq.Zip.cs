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
        /// Merges this query with another query into pairs, stopping when either query is exhausted.
        /// </summary>
        /// <param name="second">The query to merge with this query.</param>
        /// <returns>A query that yields pairs from both queries in matching order.</returns>
        public Query<Zip<TEnumerator, TSecondEnumerator, T, TSecond>, ValueTuple<T, TSecond>> Zip<TSecond, TSecondEnumerator>(
            Query<TSecondEnumerator, TSecond> second)
            where TSecond : unmanaged
            where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
        {
            return new Query<Zip<TEnumerator, TSecondEnumerator, T, TSecond>, ValueTuple<T, TSecond>>(
                new Zip<TEnumerator, TSecondEnumerator, T, TSecond>(GetEnumerator(), second.GetEnumerator()));
        }

        /// <summary>
        /// Merges this query with another query using <paramref name="resultSelector"/>, stopping when either query is exhausted.
        /// </summary>
        /// <param name="second">The query to merge with this query.</param>
        /// <param name="resultSelector">The selector used to combine matching elements.</param>
        /// <returns>A query that yields selected results from both queries in matching order.</returns>
        public Query<Zip<TEnumerator, TSecondEnumerator, T, TSecond, TResult, TResultSelector>, TResult> Zip<TSecond, TResult, TSecondEnumerator, TResultSelector>(
            Query<TSecondEnumerator, TSecond> second,
            TResultSelector resultSelector)
            where TSecond : unmanaged
            where TResult : unmanaged
            where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
            where TResultSelector : unmanaged, IZipResultSelector<T, TSecond, TResult>
        {
            return new Query<Zip<TEnumerator, TSecondEnumerator, T, TSecond, TResult, TResultSelector>, TResult>(
                new Zip<TEnumerator, TSecondEnumerator, T, TSecond, TResult, TResultSelector>(
                    GetEnumerator(),
                    second.GetEnumerator(),
                    resultSelector));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Merges two queries into pairs, stopping when either query is exhausted.
        /// </summary>
        /// <param name="first">The first query.</param>
        /// <param name="second">The second query.</param>
        /// <returns>A query that yields pairs from both queries in matching order.</returns>
        public static Query<Zip<TFirstEnumerator, TSecondEnumerator, TFirst, TSecond>, ValueTuple<TFirst, TSecond>> Zip<TFirst, TSecond, TFirstEnumerator, TSecondEnumerator>(
            this Query<TFirstEnumerator, TFirst> first,
            Query<TSecondEnumerator, TSecond> second)
            where TFirst : unmanaged
            where TSecond : unmanaged
            where TFirstEnumerator : unmanaged, IQueryEnumerator<TFirst>
            where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
        {
            return first.Zip<TSecond, TSecondEnumerator>(second);
        }

        /// <summary>
        /// Merges two queries using <paramref name="resultSelector"/>, stopping when either query is exhausted.
        /// </summary>
        /// <param name="first">The first query.</param>
        /// <param name="second">The second query.</param>
        /// <param name="resultSelector">The selector used to combine matching elements.</param>
        /// <returns>A query that yields selected results from both queries in matching order.</returns>
        public static Query<Zip<TFirstEnumerator, TSecondEnumerator, TFirst, TSecond, TResult, TResultSelector>, TResult> Zip<TFirst, TSecond, TResult, TFirstEnumerator, TSecondEnumerator, TResultSelector>(
            this Query<TFirstEnumerator, TFirst> first,
            Query<TSecondEnumerator, TSecond> second,
            TResultSelector resultSelector)
            where TFirst : unmanaged
            where TSecond : unmanaged
            where TResult : unmanaged
            where TFirstEnumerator : unmanaged, IQueryEnumerator<TFirst>
            where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
            where TResultSelector : unmanaged, IZipResultSelector<TFirst, TSecond, TResult>
        {
            return first.Zip<TSecond, TResult, TSecondEnumerator, TResultSelector>(second, resultSelector);
        }

        /// <summary>
        /// Merges two queries using <paramref name="resultSelector"/>, stopping when either query is exhausted.
        /// </summary>
        /// <param name="first">The first query.</param>
        /// <param name="second">The second query.</param>
        /// <param name="resultSelector">The selector used to combine matching elements.</param>
        /// <returns>A query that yields selected results from both queries in matching order.</returns>
        [NativeDelegateMethod(typeof(IZipResultSelector<,,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<Zip<TFirstEnumerator, TSecondEnumerator, TFirst, TSecond, TResult>, TResult> Zip<TFirst, TSecond, TResult, TFirstEnumerator, TSecondEnumerator>(
            this Query<TFirstEnumerator, TFirst> first,
            Query<TSecondEnumerator, TSecond> second,
            Func<TFirst, TSecond, TResult> resultSelector)
            where TFirst : unmanaged
            where TSecond : unmanaged
            where TResult : unmanaged
            where TFirstEnumerator : unmanaged, IQueryEnumerator<TFirst>
            where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
        {
            return ThrowCodeGen<Query<Zip<TFirstEnumerator, TSecondEnumerator, TFirst, TSecond, TResult>, TResult>>();
        }
    }

    public struct Zip<TFirstEnumerator, TSecondEnumerator, TFirst, TSecond> : IQueryEnumerator<ValueTuple<TFirst, TSecond>>
        where TFirstEnumerator : unmanaged, IQueryEnumerator<TFirst>
        where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
        where TFirst : unmanaged
        where TSecond : unmanaged
    {
        private TFirstEnumerator _first;
        private TSecondEnumerator _second;
        private ValueTuple<TFirst, TSecond> _current;

        public Zip(TFirstEnumerator first, TSecondEnumerator second)
        {
            _first = first;
            _second = second;
            _current = default;
        }

        public ValueTuple<TFirst, TSecond> Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            if (!_first.MoveNext() || !_second.MoveNext())
            {
                return false;
            }

            _current = new ValueTuple<TFirst, TSecond>(_first.Current, _second.Current);
            return true;
        }

        public void Reset()
        {
            _first.Reset();
            _second.Reset();
            _current = default;
        }

        public void Dispose()
        {
            _first.Dispose();
            _second.Dispose();
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            if (!_first.TryGetNonEnumeratedCount(out var firstCount) ||
                !_second.TryGetNonEnumeratedCount(out var secondCount))
            {
                count = 0;
                return false;
            }

            count = firstCount < secondCount ? firstCount : secondCount;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<ValueTuple<TFirst, TSecond>> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out ValueTuple<TFirst, TSecond> value)
        {
            value = default;
            if (index < 0 ||
                !_first.TryGetElementAt(index, out var first) ||
                !_second.TryGetElementAt(index, out var second))
            {
                return false;
            }

            value = new ValueTuple<TFirst, TSecond>(first, second);
            return true;
        }
    }

    public struct Zip<TFirstEnumerator, TSecondEnumerator, TFirst, TSecond, TResult, TResultSelector> : IQueryEnumerator<TResult>
        where TFirstEnumerator : unmanaged, IQueryEnumerator<TFirst>
        where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
        where TFirst : unmanaged
        where TSecond : unmanaged
        where TResult : unmanaged
        where TResultSelector : unmanaged, IZipResultSelector<TFirst, TSecond, TResult>
    {
        private TFirstEnumerator _first;
        private TSecondEnumerator _second;
        private TResultSelector _resultSelector;
        private TResult _current;

        public Zip(TFirstEnumerator first, TSecondEnumerator second, TResultSelector resultSelector)
        {
            _first = first;
            _second = second;
            _resultSelector = resultSelector;
            _current = default;
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
            if (!_first.MoveNext() || !_second.MoveNext())
            {
                return false;
            }

            var first = _first.Current;
            var second = _second.Current;
            _current = _resultSelector.Select(in first, in second);
            return true;
        }

        public void Reset()
        {
            _first.Reset();
            _second.Reset();
            _current = default;
        }

        public void Dispose()
        {
            _first.Dispose();
            _second.Dispose();
        }

        public bool TryGetNonEnumeratedCount(out int count)
        {
            if (!_first.TryGetNonEnumeratedCount(out var firstCount) ||
                !_second.TryGetNonEnumeratedCount(out var secondCount))
            {
                count = 0;
                return false;
            }

            count = firstCount < secondCount ? firstCount : secondCount;
            return true;
        }

        public bool TryGetSpan(out ReadOnlySpan<TResult> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out TResult value)
        {
            value = default;
            if (index < 0 ||
                !_first.TryGetElementAt(index, out var first) ||
                !_second.TryGetElementAt(index, out var second))
            {
                return false;
            }

            value = _resultSelector.Select(in first, in second);
            return true;
        }
    }

    public struct Zip<TFirstEnumerator, TSecondEnumerator, TFirst, TSecond, TResult> : IQueryEnumerator<TResult>
        where TFirstEnumerator : unmanaged, IQueryEnumerator<TFirst>
        where TSecondEnumerator : unmanaged, IQueryEnumerator<TSecond>
        where TFirst : unmanaged
        where TSecond : unmanaged
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
