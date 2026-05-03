using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public struct DelegateWhereQuery<T, TEnumerator> : IEnumerator<T>
        where T : unmanaged
        where TEnumerator : unmanaged, IEnumerator<T>
    {
        public T Current => Throw<T>();

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return Throw<bool>();
        }

        public void Reset()
        {
            Throw();
        }

        public void Dispose()
        {
        }

        private static void Throw()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }

        private static TResult Throw<TResult>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }

    public struct DelegateSelectQuery<TSource, TResult, TEnumerator> : IEnumerator<TResult>
        where TSource : unmanaged
        where TResult : unmanaged
        where TEnumerator : unmanaged, IEnumerator<TSource>
    {
        public TResult Current => Throw<TResult>();

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            return Throw<bool>();
        }

        public void Reset()
        {
            Throw();
        }

        public void Dispose()
        {
        }

        private static void Throw()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }

        private static T Throw<T>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }

    public static partial class BLinqExtensions
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<T, WhereQuery<T, TEnumerator, TPredicate>> Where<T, TEnumerator, TPredicate>(
            this Query<T, TEnumerator> source,
            TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return source.Where(predicate);
        }

        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<T, DelegateWhereQuery<T, TEnumerator>> Where<T, TEnumerator>(
            this Query<T, TEnumerator> source,
            Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return Throw<Query<T, DelegateWhereQuery<T, TEnumerator>>>();
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<TResult, SelectQuery<TSource, TResult, TEnumerator, TSelector>> Select<TSource, TResult, TEnumerator, TSelector>(
            this Query<TSource, TEnumerator> source,
            TSelector selector)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TSelector : unmanaged, ISelector<TSource, TResult>
        {
            return source.Select<TResult, TSelector>(selector);
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<TResult, DelegateSelectQuery<TSource, TResult, TEnumerator>> Select<TSource, TResult, TEnumerator>(
            this Query<TSource, TEnumerator> source,
            Func<TSource, TResult> selector)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return Throw<Query<TResult, DelegateSelectQuery<TSource, TResult, TEnumerator>>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult Sum<TSource, TResult, TEnumerator>(
            this Query<TSource, TEnumerator> source,
            Func<TSource, TResult> selector,
            TResult _ = default)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return Throw<TResult>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<KeyValuePair<TKey, TAccumulate>, NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator>(
                this Query<TSource, TEnumerator> source,
                Func<TSource, TKey> keySelector,
                TAccumulate seed,
                Func<TAccumulate, TSource, TAccumulate> aggregator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return Throw<Query<KeyValuePair<TKey, TAccumulate>, NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>), typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<KeyValuePair<TKey, TAccumulate>, NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator>(
                this Query<TSource, TEnumerator> source,
                Func<TSource, TKey> keySelector,
                Func<TKey, TAccumulate> seedSelector,
                Func<TAccumulate, TSource, TAccumulate> aggregator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return Throw<Query<KeyValuePair<TKey, TAccumulate>, NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator>>();
        }

        private static T Throw<T>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }
}
