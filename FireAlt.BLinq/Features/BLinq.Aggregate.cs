using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Applies an accumulator over the query.
        /// </summary>
        /// <param name="aggregator">The accumulator used to combine elements.</param>
        /// <returns>The final accumulated value.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains no elements.</exception>
        public T Aggregate<TAggregator>(TAggregator aggregator)
            where TAggregator : unmanaged, IAggregator<T, T>
        {
            return BLinqUtilities.Aggregate<T, TEnumerator, TAggregator>(GetEnumerator(), aggregator);
        }

        /// <summary>
        /// Applies an accumulator over the query using <paramref name="seed"/> as the initial value.
        /// </summary>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <returns>The final accumulated value.</returns>
        public TAccumulate Aggregate<TAccumulate, TAggregator>(TAccumulate seed, TAggregator aggregator)
            where TAccumulate : unmanaged
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
        {
            return BLinqUtilities.Aggregate<T, TAccumulate, TEnumerator, TAggregator>(GetEnumerator(), seed, aggregator);
        }

        /// <summary>
        /// Applies an accumulator over the query and transforms the final aggregate value.
        /// </summary>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <param name="resultSelector">The selector used to transform the final aggregate.</param>
        /// <returns>The transformed final aggregate value.</returns>
        public TResult Aggregate<TAccumulate, TResult, TAggregator, TResultSelector>(
            TAccumulate seed,
            TAggregator aggregator,
            TResultSelector resultSelector)
            where TAccumulate : unmanaged
            where TResult : unmanaged
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
            where TResultSelector : unmanaged, ISelector<TAccumulate, TResult>
        {
            return BLinqUtilities.Aggregate<T, TAccumulate, TResult, TEnumerator, TAggregator, TResultSelector>(
                GetEnumerator(),
                seed,
                aggregator,
                resultSelector);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Applies an accumulator over the query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="aggregator">The accumulator used to combine elements.</param>
        /// <returns>The final accumulated value.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains no elements.</exception>
        public static T Aggregate<T, TEnumerator, TAggregator>(this Query<TEnumerator, T> source, TAggregator aggregator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TAggregator : unmanaged, IAggregator<T, T>
        {
            return source.Aggregate(aggregator);
        }

        /// <summary>
        /// Applies an accumulator over the query using <paramref name="seed"/> as the initial value.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <returns>The final accumulated value.</returns>
        public static TAccumulate Aggregate<TSource, TAccumulate, TEnumerator, TAggregator>(
            this Query<TEnumerator, TSource> source,
            TAccumulate seed,
            TAggregator aggregator)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return source.Aggregate(seed, aggregator);
        }

        /// <summary>
        /// Applies an accumulator over the query and transforms the final aggregate value.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <param name="resultSelector">The selector used to transform the final aggregate.</param>
        /// <returns>The transformed final aggregate value.</returns>
        public static TResult Aggregate<TSource, TAccumulate, TResult, TEnumerator, TAggregator, TResultSelector>(
            this Query<TEnumerator, TSource> source,
            TAccumulate seed,
            TAggregator aggregator,
            TResultSelector resultSelector)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
            where TResultSelector : unmanaged, ISelector<TAccumulate, TResult>
        {
            return source.Aggregate<TAccumulate, TResult, TAggregator, TResultSelector>(seed, aggregator, resultSelector);
        }

        /// <summary>
        /// Applies an accumulator over the ordered query.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="aggregator">The accumulator used to combine elements.</param>
        /// <returns>The final accumulated value.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains no elements.</exception>
        public static T Aggregate<T, TEnumerator, TComparer, TAggregator>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TAggregator aggregator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TAggregator : unmanaged, IAggregator<T, T>
        {
            return BLinqUtilities.Aggregate<T, OrderedQueryEnumerator<T, TEnumerator, TComparer>, TAggregator>(
                source.GetEnumerator(),
                aggregator);
        }

        /// <summary>
        /// Applies an accumulator over the ordered query using <paramref name="seed"/> as the initial value.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <returns>The final accumulated value.</returns>
        public static TAccumulate Aggregate<TSource, TAccumulate, TEnumerator, TComparer, TAggregator>(
            this OrderedQuery<TEnumerator, TSource, TComparer> source,
            TAccumulate seed,
            TAggregator aggregator)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TComparer : unmanaged, IComparer<TSource>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return BLinqUtilities.Aggregate<TSource, TAccumulate, OrderedQueryEnumerator<TSource, TEnumerator, TComparer>, TAggregator>(
                source.GetEnumerator(),
                seed,
                aggregator);
        }

        /// <summary>
        /// Applies an accumulator over the ordered query and transforms the final aggregate value.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <param name="resultSelector">The selector used to transform the final aggregate.</param>
        /// <returns>The transformed final aggregate value.</returns>
        public static TResult Aggregate<TSource, TAccumulate, TResult, TEnumerator, TComparer, TAggregator, TResultSelector>(
            this OrderedQuery<TEnumerator, TSource, TComparer> source,
            TAccumulate seed,
            TAggregator aggregator,
            TResultSelector resultSelector)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TComparer : unmanaged, IComparer<TSource>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
            where TResultSelector : unmanaged, ISelector<TAccumulate, TResult>
        {
            return BLinqUtilities.Aggregate<TSource, TAccumulate, TResult, OrderedQueryEnumerator<TSource, TEnumerator, TComparer>, TAggregator, TResultSelector>(
                source.GetEnumerator(),
                seed,
                aggregator,
                resultSelector);
        }

        /// <summary>
        /// Applies a delegate accumulator over the query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="aggregator">The accumulator used to combine elements.</param>
        /// <returns>The final accumulated value.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains no elements.</exception>
        [NativeDelegateMethod(typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T Aggregate<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, T, T> aggregator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }

        /// <summary>
        /// Applies a delegate accumulator over the query using <paramref name="seed"/> as the initial value.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <returns>The final accumulated value.</returns>
        [NativeDelegateMethod(typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TAccumulate Aggregate<TSource, TAccumulate, TEnumerator>(
            this Query<TEnumerator, TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> aggregator)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return ThrowCodeGen<TAccumulate>();
        }

        /// <summary>
        /// Applies a delegate accumulator over the query and transforms the final aggregate value.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <param name="resultSelector">The selector used to transform the final aggregate.</param>
        /// <returns>The transformed final aggregate value.</returns>
        [NativeDelegateMethod(typeof(IAggregator<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult Aggregate<TSource, TAccumulate, TResult, TEnumerator>(
            this Query<TEnumerator, TSource> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> aggregator,
            Func<TAccumulate, TResult> resultSelector)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return ThrowCodeGen<TResult>();
        }

        /// <summary>
        /// Applies a delegate accumulator over the ordered query.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="aggregator">The accumulator used to combine elements.</param>
        /// <returns>The final accumulated value.</returns>
        /// <exception cref="InvalidOperationException">The source sequence contains no elements.</exception>
        [NativeDelegateMethod(typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T Aggregate<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, T, T> aggregator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<T>();
        }

        /// <summary>
        /// Applies a delegate accumulator over the ordered query using <paramref name="seed"/> as the initial value.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <returns>The final accumulated value.</returns>
        [NativeDelegateMethod(typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TAccumulate Aggregate<TSource, TAccumulate, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, TSource, TComparer> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> aggregator)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TComparer : unmanaged, IComparer<TSource>
        {
            return ThrowCodeGen<TAccumulate>();
        }

        /// <summary>
        /// Applies a delegate accumulator over the ordered query and transforms the final aggregate value.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="seed">The initial aggregate value.</param>
        /// <param name="aggregator">The accumulator used to combine the aggregate with each element.</param>
        /// <param name="resultSelector">The selector used to transform the final aggregate.</param>
        /// <returns>The transformed final aggregate value.</returns>
        [NativeDelegateMethod(typeof(IAggregator<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult Aggregate<TSource, TAccumulate, TResult, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, TSource, TComparer> source,
            TAccumulate seed,
            Func<TAccumulate, TSource, TAccumulate> aggregator,
            Func<TAccumulate, TResult> resultSelector)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TComparer : unmanaged, IComparer<TSource>
        {
            return ThrowCodeGen<TResult>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static T Aggregate<T, TEnumerator, TAggregator>(TEnumerator enumerator, TAggregator aggregator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TAggregator : unmanaged, IAggregator<T, T>
        {
            if (!enumerator.MoveNext())
            {
                enumerator.Dispose();
                throw new InvalidOperationException("The BLinq source contains no elements.");
            }

            var aggregate = enumerator.Current;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                aggregate = aggregator.Aggregate(in aggregate, in value);
            }

            enumerator.Dispose();
            return aggregate;
        }

        public static TAccumulate Aggregate<TSource, TAccumulate, TEnumerator, TAggregator>(
            TEnumerator enumerator,
            TAccumulate seed,
            TAggregator aggregator)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            var aggregate = seed;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                aggregate = aggregator.Aggregate(in aggregate, in value);
            }

            enumerator.Dispose();
            return aggregate;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult, TEnumerator, TAggregator, TResultSelector>(
            TEnumerator enumerator,
            TAccumulate seed,
            TAggregator aggregator,
            TResultSelector resultSelector)
            where TSource : unmanaged
            where TAccumulate : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
            where TResultSelector : unmanaged, ISelector<TAccumulate, TResult>
        {
            var aggregate = Aggregate<TSource, TAccumulate, TEnumerator, TAggregator>(
                enumerator,
                seed,
                aggregator);
            return resultSelector.Select(in aggregate);
        }
    }
}
