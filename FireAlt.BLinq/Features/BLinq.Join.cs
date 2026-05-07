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
        /// Correlates two queries based on matching keys.
        /// </summary>
        /// <param name="outer">The outer query.</param>
        /// <param name="inner">The inner query.</param>
        /// <param name="outerKeySelector">Selector that computes keys for outer elements.</param>
        /// <param name="innerKeySelector">Selector that computes keys for inner elements.</param>
        /// <param name="resultSelector">Selector that combines matching outer and inner elements.</param>
        /// <returns>A query that yields one result for each matching outer/inner pair.</returns>
        public static Query<Join<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector>, TResult>
            Join<TOuter, TInner, TKey, TResult, TOuterEnumerator, TInnerEnumerator, TOuterKeySelector, TInnerKeySelector, TResultSelector>(
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
            where TResultSelector : unmanaged, IJoinResultSelector<TOuter, TInner, TResult>
        {
            return new Query<Join<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector>, TResult>(
                new Join<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector>(
                    outer.GetEnumerator(),
                    inner.GetEnumerator(),
                    outerKeySelector,
                    innerKeySelector,
                    resultSelector));
        }

        /// <summary>
        /// Correlates two queries based on matching keys.
        /// </summary>
        /// <param name="outer">The outer query.</param>
        /// <param name="inner">The inner query.</param>
        /// <param name="outerKeySelector">Selector that computes keys for outer elements.</param>
        /// <param name="innerKeySelector">Selector that computes keys for inner elements.</param>
        /// <param name="resultSelector">Selector that combines matching outer and inner elements.</param>
        /// <returns>A query that yields one result for each matching outer/inner pair.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>), typeof(IJoinResultSelector<,,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<Join<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult>, TResult>
            Join<TOuter, TInner, TKey, TResult, TOuterEnumerator, TInnerEnumerator>(
                this Query<TOuterEnumerator, TOuter> outer,
                Query<TInnerEnumerator, TInner> inner,
                Func<TOuter, TKey> outerKeySelector,
                Func<TInner, TKey> innerKeySelector,
                Func<TOuter, TInner, TResult> resultSelector)
            where TOuter : unmanaged
            where TInner : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TResult : unmanaged
            where TOuterEnumerator : unmanaged, IQueryEnumerator<TOuter>
            where TInnerEnumerator : unmanaged, IQueryEnumerator<TInner>
        {
            return ThrowCodeGen<Query<Join<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult>, TResult>>();
        }
    }

    public struct Join<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult, TOuterKeySelector, TInnerKeySelector, TResultSelector> : IQueryEnumerator<TResult>
        where TOuterEnumerator : unmanaged, IQueryEnumerator<TOuter>
        where TInnerEnumerator : unmanaged, IQueryEnumerator<TInner>
        where TOuter : unmanaged
        where TInner : unmanaged
        where TKey : unmanaged, IEquatable<TKey>
        where TResult : unmanaged
        where TOuterKeySelector : unmanaged, ISelector<TOuter, TKey>
        where TInnerKeySelector : unmanaged, ISelector<TInner, TKey>
        where TResultSelector : unmanaged, IJoinResultSelector<TOuter, TInner, TResult>
    {
        private TOuterEnumerator _outer;
        private TInnerEnumerator _inner;
        private TOuterKeySelector _outerKeySelector;
        private TInnerKeySelector _innerKeySelector;
        private TResultSelector _resultSelector;
        private UnsafeHashMapSlim<TKey, int> _keyToGroupIndex;
        private NativeList<Group<TKey, TInner>> _groups;
        private Group<TKey, TInner> _currentGroup;
        private TOuter _currentOuter;
        private TResult _current;
        private int _innerIndex;
        private byte _state;

        public Join(
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
            _currentGroup = default;
            _currentOuter = default;
            _current = default;
            _innerIndex = 0;
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

            while (true)
            {
                if (_state == 2 && _innerIndex < _currentGroup.Length)
                {
                    var inner = _currentGroup[_innerIndex];
                    _innerIndex++;
                    _current = _resultSelector.Select(in _currentOuter, in inner);
                    return true;
                }

                _state = 1;
                while (_outer.MoveNext())
                {
                    _currentOuter = _outer.Current;
                    var key = _outerKeySelector.Select(in _currentOuter);
                    if (!_keyToGroupIndex.TryGetValue(key, out var groupIndex))
                    {
                        continue;
                    }

                    _currentGroup = _groups[groupIndex];
                    _innerIndex = 0;
                    _state = 2;
                    break;
                }

                if (_state != 2)
                {
                    return false;
                }
            }
        }

        public void Reset()
        {
            DisposeLookup();
            _outer.Reset();
            _inner.Reset();
            _currentGroup = default;
            _currentOuter = default;
            _current = default;
            _innerIndex = 0;
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

    public struct Join<TOuterEnumerator, TInnerEnumerator, TOuter, TInner, TKey, TResult> : IQueryEnumerator<TResult>
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
