using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Correlates two queries based on matching keys and groups inner matches for each outer element.
        /// </summary>
        /// <param name="outer">The outer query.</param>
        /// <param name="inner">The inner query.</param>
        /// <param name="outerKeySelector">Selector that computes keys for outer elements.</param>
        /// <param name="innerKeySelector">Selector that computes keys for inner elements.</param>
        /// <param name="resultSelector">Selector that combines each outer element with its matching inner group.</param>
        /// <returns>A query that yields one result for each outer element.</returns>
        public static Query<GroupJoin<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector>, TResult>
            GroupJoin<TOuter, TInner, TKey, TResult, TOuterEnumerator, TInnerEnumerator, TOuterKeySelector, TInnerKeySelector, TResultSelector>(
                this Query<TOuterEnumerator, TOuter> outer,
                Query<TInnerEnumerator, TInner> inner,
                TOuterKeySelector outerKeySelector,
                TInnerKeySelector innerKeySelector,
                TResultSelector resultSelector)
            where TOuter : unmanaged
            where TInner : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TResult : unmanaged
            where TOuterEnumerator : unmanaged, IQueryEnumerator<TOuter>
            where TInnerEnumerator : unmanaged, IQueryEnumerator<TInner>
            where TOuterKeySelector : unmanaged, ISelector<TOuter, TKey>
            where TInnerKeySelector : unmanaged, ISelector<TInner, TKey>
            where TResultSelector : unmanaged, IGroupJoinResultSelector<TOuter, Group<TKey, TInner>, TResult>
        {
            return new Query<GroupJoin<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector>, TResult>(
                new GroupJoin<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector>(
                    outer.GetEnumerator(),
                    inner.GetEnumerator(),
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector));
        }

        /// <summary>
        /// Correlates two queries based on matching keys and groups inner matches for each outer element.
        /// </summary>
        /// <param name="outer">The outer query.</param>
        /// <param name="inner">The inner query.</param>
        /// <param name="outerKeySelector">Selector that computes keys for outer elements.</param>
        /// <param name="innerKeySelector">Selector that computes keys for inner elements.</param>
        /// <param name="resultSelector">Selector that combines each outer element with its matching inner group.</param>
        /// <returns>A query that yields one result for each outer element.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>), typeof(IGroupJoinResultSelector<,,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<GroupJoin<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult>, TResult>
            GroupJoin<TOuter, TInner, TKey, TResult, TOuterEnumerator, TInnerEnumerator>(
                this Query<TOuterEnumerator, TOuter> outer,
                Query<TInnerEnumerator, TInner> inner,
                Func<TOuter, TKey> outerKeySelector,
                Func<TInner, TKey> innerKeySelector,
                Func<TOuter, Group<TKey, TInner>, TResult> resultSelector)
            where TOuter : unmanaged
            where TInner : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TResult : unmanaged
            where TOuterEnumerator : unmanaged, IQueryEnumerator<TOuter>
            where TInnerEnumerator : unmanaged, IQueryEnumerator<TInner>
        {
            return ThrowCodeGen<Query<GroupJoin<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult>, TResult>>();
        }
    }

    public struct GroupJoin<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector> : IQueryEnumerator<TResult>
        where TOuterEnumerator : unmanaged, IQueryEnumerator<TOuter>
        where TInnerEnumerator : unmanaged, IQueryEnumerator<TInner>
        where TOuter : unmanaged
        where TInner : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
        where TResult : unmanaged
        where TOuterKeySelector : unmanaged, ISelector<TOuter, TKey>
        where TInnerKeySelector : unmanaged, ISelector<TInner, TKey>
        where TResultSelector : unmanaged, IGroupJoinResultSelector<TOuter, Group<TKey, TInner>, TResult>
    {
        private TOuterEnumerator _outer;
        private TInnerEnumerator _inner;
        private TOuterKeySelector _outerKeySelector;
        private TInnerKeySelector _innerKeySelector;
        private TResultSelector _resultSelector;
        private UnsafeHashMapSlim<TKey, int> _keyToGroupIndex;
        private NativeList<Group<TKey, TInner>> _groups;
        private TResult _current;
        private byte _state;

        public GroupJoin(
            TOuterEnumerator outer,
            TInnerEnumerator inner,
            TOuterKeySelector outerKeySelector,
            TInnerKeySelector innerKeySelector,
            TResultSelector resultSelector)
        {
            _outer = outer;
            _inner = inner;
            _outerKeySelector = outerKeySelector;
            _innerKeySelector = innerKeySelector;
            _resultSelector = resultSelector;
            _keyToGroupIndex = default;
            _groups = default;
            _current = default;
            _state = 0;
        }

        public TResult Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _current;
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_state == 0)
            {
                BuildLookup();
                _state = 1;
            }

            if (!_outer.MoveNext())
            {
                return false;
            }

            var outer = _outer.Current;
            var key = _outerKeySelector.Select(in outer);
            if (_keyToGroupIndex.TryGetValue(key, out var groupIndex))
            {
                var group = _groups[groupIndex];
                _current = _resultSelector.Select(in outer, in group);
                return true;
            }

            var emptyGroup = new Group<TKey, TInner>(key, Allocator.Temp);
            _current = _resultSelector.Select(in outer, in emptyGroup);
            return true;
        }

        public void Reset()
        {
            DisposeLookup();
            _outer.Reset();
            _inner.Reset();
            _current = default;
            _state = 0;
        }

        public void Dispose()
        {
            _outer.Dispose();
            _inner.Dispose();
            DisposeLookup();
        }

        private void BuildLookup()
        {
            _keyToGroupIndex = new UnsafeHashMapSlim<TKey, int>(64, Allocator.Temp);
            _groups = new NativeList<Group<TKey, TInner>>(64, Allocator.Temp);

            while (_inner.MoveNext())
            {
                var value = _inner.Current;
                var key = _innerKeySelector.Select(in value);
                ref var groupIndex = ref _keyToGroupIndex.GetValueRefOrAddDefault(key, out var exists);
                if (!exists)
                {
                    groupIndex = _groups.Length;
                    _groups.Add(new Group<TKey, TInner>(key, value, Allocator.Temp));
                }
                else
                {
                    ref var group = ref _groups.ElementAt(groupIndex);
                    group.Add(in value);
                }
            }

            _inner.Dispose();
        }

        private void DisposeLookup()
        {
            if (_groups.IsCreated)
            {
                for (var i = 0; i < _groups.Length; i++)
                {
                    ref var group = ref _groups.ElementAt(i);
                    group.Dispose();
                }

                _groups.Dispose();
                _groups = default;
            }

            if (_state != 0)
            {
                _keyToGroupIndex.Dispose();
                _keyToGroupIndex = default;
            }
        }
    
        public bool TryGetElementAt(int index, out TResult value)
        {
            value = default;
            return false;
        }
}

    public struct GroupJoin<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult> : IQueryEnumerator<TResult>
        where TOuterEnumerator : unmanaged, IQueryEnumerator<TOuter>
        where TInnerEnumerator : unmanaged, IQueryEnumerator<TInner>
        where TOuter : unmanaged
        where TInner : unmanaged
        where TKey : unmanaged
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
    
        public bool TryGetElementAt(int index, out TResult value)
        {
            value = default;
            return false;
        }
}
}
