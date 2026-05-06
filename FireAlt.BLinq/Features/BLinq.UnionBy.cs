using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Produces the set union of this query and another query according to a key selector.
        /// </summary>
        /// <param name="other">The query whose distinct keyed elements are appended after this query.</param>
        /// <param name="keySelector">Selector used to compute each element key.</param>
        /// <returns>A query that yields the first element for each distinct key across both input queries.</returns>
        public Query<UnionBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>, T> UnionBy<TOtherEnumerator, TKey, TKeySelector>(
            Query<TOtherEnumerator, T> other,
            TKeySelector keySelector)
            where TOtherEnumerator : unmanaged, IEnumerator<T>
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return new Query<UnionBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>, T>(
                new UnionBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>(
                    GetEnumerator(),
                    other.GetEnumerator(),
                    keySelector));
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Produces the set union of two queries according to a key selector.
        /// </summary>
        /// <param name="source">The first query.</param>
        /// <param name="other">The query whose distinct keyed elements are appended after <paramref name="source"/>.</param>
        /// <param name="keySelector">Selector used to compute each element key.</param>
        /// <returns>A query that yields the first element for each distinct key across both input queries.</returns>
        public static Query<UnionBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector>, T> UnionBy<T, TKey, TEnumerator, TOtherEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, T> other,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TOtherEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.UnionBy<TOtherEnumerator, TKey, TKeySelector>(other, keySelector);
        }

        /// <summary>
        /// Produces the set union of two queries according to a delegate key selector.
        /// </summary>
        /// <param name="source">The first query.</param>
        /// <param name="other">The query whose distinct keyed elements are appended after <paramref name="source"/>.</param>
        /// <param name="keySelector">Selector used to compute each element key.</param>
        /// <returns>A query that yields the first element for each distinct key across both input queries.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<UnionBy<TEnumerator, TOtherEnumerator, T, TKey>, T> UnionBy<T, TKey, TEnumerator, TOtherEnumerator>(
            this Query<TEnumerator, T> source,
            Query<TOtherEnumerator, T> other,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TOtherEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Query<UnionBy<TEnumerator, TOtherEnumerator, T, TKey>, T>>();
        }
    }

    public struct UnionBy<TEnumerator, TOtherEnumerator, T, TKey, TKeySelector> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where TOtherEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
        where TKeySelector : unmanaged, ISelector<T, TKey>
    {
        private TEnumerator _source;
        private TOtherEnumerator _other;
        private TKeySelector _keySelector;
        private UnsafeHashMapSlim<TKey, byte> _seen;
        private T _current;
        private byte _state;

        public UnionBy(TEnumerator source, TOtherEnumerator other, TKeySelector keySelector)
        {
            _source = source;
            _other = other;
            _keySelector = keySelector;
            _seen = default;
            _current = default;
            _state = 0;
        }

        public T Current => _current;

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_state == 0)
            {
                _seen = new UnsafeHashMapSlim<TKey, byte>(64, Allocator.Temp);
                _state = 1;
            }

            while (_state == 1 && _source.MoveNext())
            {
                if (TryYield(_source.Current))
                {
                    return true;
                }
            }

            _state = 2;
            while (_other.MoveNext())
            {
                if (TryYield(_other.Current))
                {
                    return true;
                }
            }

            _state = 3;
            return false;
        }

        public void Reset()
        {
            if (_state != 0)
            {
                _seen.Dispose();
            }

            _source.Reset();
            _other.Reset();
            _seen = default;
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _source.Dispose();
            _other.Dispose();
            if (_state != 0)
            {
                _seen.Dispose();
                _seen = default;
                _state = 0;
            }
        }

        private bool TryYield(T value)
        {
            var key = _keySelector.Select(in value);
            ref var marker = ref _seen.GetValueRefOrAddDefault(key, out var exists);
            if (exists)
            {
                return false;
            }

            marker = 1;
            _current = value;
            return true;
        }
    }

    public struct UnionBy<TEnumerator, TOtherEnumerator, T, TKey> : IEnumerator<T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where TOtherEnumerator : unmanaged, IEnumerator<T>
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
    }
}
