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
        /// Returns distinct elements from the query according to a key selector.
        /// </summary>
        /// <param name="keySelector">Selector used to compute each element key.</param>
        /// <returns>A query that yields the first element for each distinct key.</returns>
        public Query<DistinctBy<TEnumerator, T, TKey, TKeySelector>, T> DistinctBy<TKey, TKeySelector>(
            TKeySelector keySelector)
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return new Query<DistinctBy<TEnumerator, T, TKey, TKeySelector>, T>(
                new DistinctBy<TEnumerator, T, TKey, TKeySelector>(GetEnumerator(), keySelector));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns distinct elements from the query according to a key selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selector used to compute each element key.</param>
        /// <returns>A query that yields the first element for each distinct key.</returns>
        public static Query<DistinctBy<TEnumerator, T, TKey, TKeySelector>, T> DistinctBy<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.DistinctBy<TKey, TKeySelector>(keySelector);
        }

        /// <summary>
        /// Returns distinct elements from the query according to a delegate key selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">Selector used to compute each element key.</param>
        /// <returns>A query that yields the first element for each distinct key.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<DistinctBy<TEnumerator, T, TKey>, T> DistinctBy<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<Query<DistinctBy<TEnumerator, T, TKey>, T>>();
        }
    }

    public struct DistinctBy<TEnumerator, T, TKey, TKeySelector> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
        where TKeySelector : unmanaged, ISelector<T, TKey>
    {
        private TEnumerator _source;
        private TKeySelector _keySelector;
        private UnsafeHashMapSlim<TKey, byte> _seen;
        private T _current;
        private bool _initialized;

        public DistinctBy(TEnumerator source, TKeySelector keySelector)
        {
            _source = source;
            _keySelector = keySelector;
            _seen = default;
            _current = default;
            _initialized = false;
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (!_initialized)
            {
                _seen = new UnsafeHashMapSlim<TKey, byte>(64, Allocator.Temp);
                _initialized = true;
            }

            while (_source.MoveNext())
            {
                var value = _source.Current;
                var key = _keySelector.Select(in value);
                ref var marker = ref _seen.GetValueRefOrAddDefault(key, out var exists);
                if (exists)
                {
                    continue;
                }

                marker = 1;
                _current = value;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            if (_initialized)
            {
                _seen.Dispose();
            }

            _source.Reset();
            _seen = default;
            _current = default;
            _initialized = false;
        }

        public void Dispose()
        {
            _source.Dispose();
            if (_initialized)
            {
                _seen.Dispose();
                _seen = default;
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

    public struct DistinctBy<TEnumerator, T, TKey> : IQueryEnumerator<T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
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
