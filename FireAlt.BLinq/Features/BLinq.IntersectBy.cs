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
        /// Produces the set intersection of this query and a query of keys according to a key selector.
        /// </summary>
        /// <param name="other">The query whose keys are used to filter this query.</param>
        /// <param name="keySelector">Selector used to compute each source element key.</param>
        /// <returns>A query that yields the first source element for each matching key.</returns>
        public Query<IntersectBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>, T> IntersectBy<TOtherEnumerator, TKey, TKeySelector>(
            Query<TOtherEnumerator, TKey> other,
            TKeySelector keySelector)
            where TOtherEnumerator : unmanaged, IQueryEnumerator<TKey>
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return new Query<IntersectBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>, T>(
                new IntersectBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>(
                    GetEnumerator(),
                    other.GetEnumerator(),
                    keySelector));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Produces the set intersection of a query and a query of keys according to a key selector.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="other">The query whose keys are used to filter <paramref name="source"/>.</param>
        /// <param name="keySelector">Selector used to compute each source element key.</param>
        /// <returns>A query that yields the first source element for each matching key.</returns>
        public static Query<IntersectBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>, T> IntersectBy<T, TKey, TEnumerator, TOtherEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, TKey> other,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TOtherEnumerator : unmanaged, IQueryEnumerator<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.IntersectBy<TOtherEnumerator, TKey, TKeySelector>(other, keySelector);
        }

        /// <summary>
        /// Produces the set intersection of a query and a query of keys according to a delegate key selector.
        /// </summary>
        /// <param name="source">The source query.</param>
        /// <param name="other">The query whose keys are used to filter <paramref name="source"/>.</param>
        /// <param name="keySelector">Selector used to compute each source element key.</param>
        /// <returns>A query that yields the first source element for each matching key.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<IntersectBy<TEnumerator, TOtherEnumerator, T, TKey>, T> IntersectBy<T, TKey, TEnumerator, TOtherEnumerator>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, TKey> other,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TOtherEnumerator : unmanaged, IQueryEnumerator<TKey>
        {
            return ThrowCodeGen<Query<IntersectBy<TEnumerator, TOtherEnumerator, T, TKey>, T>>();
        }
    }

    public struct IntersectBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where TOtherEnumerator : unmanaged, IQueryEnumerator<TKey>
        where T : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
        where TKeySelector : unmanaged, ISelector<T, TKey>
    {
        private TEnumerator _source;
        private TOtherEnumerator _other;
        private TKeySelector _keySelector;
        private UnsafeHashMapSlim<TKey, byte> _set;
        private T _current;
        private bool _initialized;

        public IntersectBy(TEnumerator source, TOtherEnumerator other, TKeySelector keySelector)
        {
            _source = source;
            _other = other;
            _keySelector = keySelector;
            _set = default;
            _current = default;
            _initialized = false;
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_initialized)
            {
                _set = new UnsafeHashMapSlim<TKey, byte>(64, Allocator.Temp);
                while (_other.MoveNext())
                {
                    ref var marker = ref _set.GetValueRefOrAddDefault(_other.Current, out var exists);
                    if (!exists)
                    {
                        marker = 1;
                    }
                }

                _initialized = true;
            }

            while (_source.MoveNext())
            {
                var value = _source.Current;
                var key = _keySelector.Select(in value);
                ref var marker = ref _set.GetValueRefOrAddDefault(key, out var exists);
                if (!exists || marker != 1)
                {
                    continue;
                }

                marker = 2;
                _current = value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            if (_initialized)
            {
                _set.Dispose();
            }

            _source.Reset();
            _other.Reset();
            _set = default;
            _current = default;
            _initialized = false;
        }

        public void Dispose()
        {
            _source.Dispose();
            _other.Dispose();
            if (_initialized)
            {
                _set.Dispose();
                _set = default;
                _initialized = false;
            }
        }
    
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            return false;
        }
}

    public struct IntersectBy<TEnumerator, TOtherEnumerator, T, TKey> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where TOtherEnumerator : unmanaged, IQueryEnumerator<TKey>
        where T : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
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
    
        public bool TryGetNonEnumeratedCount(out int count)
        {
            count = 0;
            return false;
        }

        public bool TryGetSpan(out ReadOnlySpan<T> span)
        {
            span = default;
            return false;
        }

        public bool TryGetElementAt(int index, out T value)
        {
            value = default;
            return false;
        }
}
}
