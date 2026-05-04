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
        public Query<Select<TEnumerator, T, T, TSelector>, T> Select<TSelector>(TSelector selector)
            where TSelector : unmanaged, ISelector<T, T>
        {
            return new Query<Select<TEnumerator, T, T, TSelector>, T>(
                new Select<TEnumerator, T, T, TSelector>(GetEnumerator(), selector));
        }

        public Query<Select<TEnumerator, T, TResult, TSelector>, TResult> Select<TResult, TSelector>(TSelector selector)
            where TResult : unmanaged
            where TSelector : unmanaged, ISelector<T, TResult>
        {
            return new Query<Select<TEnumerator, T, TResult, TSelector>, TResult>(
                new Select<TEnumerator, T, TResult, TSelector>(GetEnumerator(), selector));
        }
    }

    public static partial class BLinqExtensions
    {
        public static Query<Select<TEnumerator, T, TResult, TSelector>, TResult> Select<T, TResult, TEnumerator, TSelector>(
            this Query<TEnumerator, T> source, TSelector selector)
            where T : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TSelector : unmanaged, ISelector<T, TResult>
        {
            return source.Select<TResult, TSelector>(selector);
        }
        
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<Select<TEnumerator, T, TResult>, TResult> Select<T, TResult, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TResult> selector)
            where T : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Query<Select<TEnumerator, T, TResult>, TResult>>();
        }
    }
    
    public struct Select<TEnumerator, TSource, TResult, TSelector> : IEnumerator<TResult>
        where TEnumerator : unmanaged, IEnumerator<TSource>
        where TSource : unmanaged
        where TResult : unmanaged
        where TSelector : unmanaged, ISelector<TSource, TResult>
    {
        private TEnumerator _source;
        private TSelector _selector;

        public Select(TEnumerator source, TSelector selector)
        {
            _source = source;
            _selector = selector;
        }

        public TResult Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var value = _source.Current;
                return _selector.Select(in value);
            }
        }

        object IEnumerator.Current => Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return _source.MoveNext();
        }

        public void Reset()
        {
            _source.Reset();
        }

        public void Dispose()
        {
            _source.Dispose();
        }
    }
    
    public struct Select<TEnumerator, TSource, TResult> : IEnumerator<TResult>
        where TEnumerator : unmanaged, IEnumerator<TSource>
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
    }
}
