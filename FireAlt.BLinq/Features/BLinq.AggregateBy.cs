using System;
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
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public Query<NativeArray<KeyValuePair<TAccumulate, TAccumulate>>.Enumerator, KeyValuePair<TAccumulate, TAccumulate>>
            AggregateBy<TAccumulate, TKeySelector, TAggregator>(
                TKeySelector keySelector,
                TAccumulate seed,
                TAggregator aggregator)
            where TAccumulate : unmanaged, IEquatable<TAccumulate>
            where TKeySelector : unmanaged, ISelector<T, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
        {
            return BLinqUtilities.AggregateBy<T, TAccumulate, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                GetEnumerator(), keySelector, seed, aggregator, Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TKey, TAccumulate, TKeySelector, TAggregator>(
                TKeySelector keySelector,
                TAccumulate seed,
                TAggregator aggregator)
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
        {
            return BLinqUtilities.AggregateBy<T, TKey, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                GetEnumerator(), keySelector, seed, aggregator, Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Aggregates elements by key using a per-key seed selector.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seedSelector">The function used to compute the seed for a key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TKey, TAccumulate, TKeySelector, TSeedSelector, TAggregator>(
                TKeySelector keySelector,
                TSeedSelector seedSelector,
                TAggregator aggregator)
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TSeedSelector : unmanaged, ISelector<TKey, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
        {
            return BLinqUtilities.AggregateBy<T, TKey, TAccumulate, TEnumerator, TKeySelector, TSeedSelector, TAggregator>(
                GetEnumerator(), keySelector, seedSelector, aggregator, Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public NativeList<KeyValuePair<TAccumulate, TAccumulate>> ToAggregatedBy<TAccumulate, TKeySelector, TAggregator>(
            TKeySelector keySelector,
            TAccumulate seed,
            TAggregator aggregator,
            AllocatorManager.AllocatorHandle allocator)
            where TAccumulate : unmanaged, IEquatable<TAccumulate>
            where TKeySelector : unmanaged, ISelector<T, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
        {
            return BLinqUtilities.AggregateBy<T, TAccumulate, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                GetEnumerator(), keySelector, seed, aggregator, allocator);
        }

        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public NativeList<KeyValuePair<TKey, TAccumulate>> ToAggregatedBy<TKey, TAccumulate, TKeySelector, TAggregator>(
            TKeySelector keySelector,
            TAccumulate seed,
            TAggregator aggregator,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
        {
            return BLinqUtilities.AggregateBy<T, TKey, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                GetEnumerator(), keySelector, seed, aggregator, allocator);
        }

        /// <summary>
        /// Aggregates elements by key using a per-key seed selector.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seedSelector">The function used to compute the seed for a key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public NativeList<KeyValuePair<TKey, TAccumulate>> ToAggregatedBy<TKey, TAccumulate, TKeySelector, TSeedSelector, TAggregator>(
            TKeySelector keySelector,
            TSeedSelector seedSelector,
            TAggregator aggregator,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TSeedSelector : unmanaged, ISelector<TKey, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, T>
        {
            return BLinqUtilities.AggregateBy<T, TKey, TAccumulate, TEnumerator, TKeySelector, TSeedSelector, TAggregator>(
                GetEnumerator(), keySelector, seedSelector, aggregator, allocator);
        }
    }
    
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public static Query<NativeArray<KeyValuePair<TAccumulate, TAccumulate>>.Enumerator, KeyValuePair<TAccumulate, TAccumulate>>
            AggregateBy<TSource, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                this Query<TEnumerator, TSource> source,
                TKeySelector keySelector,
                TAccumulate seed,
                TAggregator aggregator)
            where TSource : unmanaged
            where TAccumulate : unmanaged, IEquatable<TAccumulate>
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return BLinqUtilities.AggregateBy<TSource, TAccumulate, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                source.GetEnumerator(), keySelector, seed, aggregator, Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public static Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                this Query<TEnumerator, TSource> source,
                TKeySelector keySelector,
                TAccumulate seed,
                TAggregator aggregator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TKey>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return BLinqUtilities.AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                source.GetEnumerator(), keySelector, seed, aggregator, Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Aggregates elements by key using a per-key seed selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seedSelector">The function used to compute the seed for a key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public static Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TSeedSelector, TAggregator>(
                this Query<TEnumerator, TSource> source,
                TKeySelector keySelector,
                TSeedSelector seedSelector,
                TAggregator aggregator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TKey>
            where TSeedSelector : unmanaged, ISelector<TKey, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return BLinqUtilities.AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TSeedSelector, TAggregator>(
                source.GetEnumerator(), keySelector, seedSelector, aggregator, Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public static NativeList<KeyValuePair<TAccumulate, TAccumulate>>
            ToAggregatedBy<TSource, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                this Query<TEnumerator, TSource> source,
                TKeySelector keySelector,
                TAccumulate seed,
                TAggregator aggregator,
                AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TAccumulate : unmanaged, IEquatable<TAccumulate>
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return BLinqUtilities.AggregateBy<TSource, TAccumulate, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                source.GetEnumerator(), keySelector, seed, aggregator, allocator);
        }

        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public static NativeList<KeyValuePair<TKey, TAccumulate>>
            ToAggregatedBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                this Query<TEnumerator, TSource> source,
                TKeySelector keySelector,
                TAccumulate seed,
                TAggregator aggregator,
                AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TKey>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return BLinqUtilities.AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                source.GetEnumerator(), keySelector, seed, aggregator, allocator);
        }

        /// <summary>
        /// Aggregates elements by key using a per-key seed selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seedSelector">The function used to compute the seed for a key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        public static NativeList<KeyValuePair<TKey, TAccumulate>>
            ToAggregatedBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TSeedSelector, TAggregator>(
                this Query<TEnumerator, TSource> source,
                TKeySelector keySelector,
                TSeedSelector seedSelector,
                TAggregator aggregator,
                AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TKey>
            where TSeedSelector : unmanaged, ISelector<TKey, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            return BLinqUtilities.AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TSeedSelector, TAggregator>(
                source.GetEnumerator(), keySelector, seedSelector, aggregator, allocator);
        }
        
        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>), typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator>(
                this Query<TEnumerator, TSource> source,
                Func<TSource, TKey> keySelector,
                TAccumulate seed,
                Func<TAccumulate, TSource, TAccumulate> aggregator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return ThrowCodeGen<Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>>();
        }

        /// <summary>
        /// Aggregates elements by key using a per-key seed selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seedSelector">The function used to compute the seed for a key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <returns>
        /// A query of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>), typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator>(
                this Query<TEnumerator, TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TKey, TAccumulate> seedSelector,
                Func<TAccumulate, TSource, TAccumulate> aggregator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return ThrowCodeGen<Query<NativeArray<KeyValuePair<TKey, TAccumulate>>.Enumerator, KeyValuePair<TKey, TAccumulate>>>();
        }

        /// <summary>
        /// Aggregates elements by key using a shared seed value for each key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seed">The initial aggregate value for each new key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>), typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeList<KeyValuePair<TKey, TAccumulate>>
            ToAggregatedBy<TSource, TKey, TAccumulate, TEnumerator>(
                this Query<TEnumerator, TSource> source,
                Func<TSource, TKey> keySelector,
                TAccumulate seed,
                Func<TAccumulate, TSource, TAccumulate> aggregator,
                AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return ThrowCodeGen<NativeList<KeyValuePair<TKey, TAccumulate>>>();
        }

        /// <summary>
        /// Aggregates elements by key using a per-key seed selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="seedSelector">The function used to compute the seed for a key.</param>
        /// <param name="aggregator">The function used to combine the current aggregate with each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>
        /// A list of key and aggregate pairs. Keys appear in the order they are first encountered.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>), typeof(IAggregator<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeList<KeyValuePair<TKey, TAccumulate>>
            ToAggregatedBy<TSource, TKey, TAccumulate, TEnumerator>(
                this Query<TEnumerator, TSource> source,
                Func<TSource, TKey> keySelector,
                Func<TKey, TAccumulate> seedSelector,
                Func<TAccumulate, TSource, TAccumulate> aggregator,
                AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return ThrowCodeGen<NativeList<KeyValuePair<TKey, TAccumulate>>>();
        }
    }

    internal static partial class BLinqUtilities
    {
        private const int DEFAULT_AGGREGATE_BY_CAPACITY = 64;

        public static NativeList<KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TAggregator>(
                TEnumerator source,
                TKeySelector keySelector,
                TAccumulate seed,
                TAggregator aggregator,
                AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TKey>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            var aggregatesByKey = new UnsafeHashMapSlim<TKey, TAccumulate>(DEFAULT_AGGREGATE_BY_CAPACITY, Allocator.Temp);

            while (source.MoveNext())
            {
                var value = source.Current;
                var key = keySelector.Select(in value);
                ref var aggregate = ref aggregatesByKey.GetValueRefOrAddDefault(key, out var exists);

                if (!exists)
                {
                    aggregate = aggregator.Aggregate(in seed, in value);
                }
                else
                {
                    aggregate = aggregator.Aggregate(in aggregate, in value);
                }
            }

            source.Dispose();
            var aggregates = new NativeList<KeyValuePair<TKey, TAccumulate>>(aggregatesByKey.Count, allocator);
            var aggregateEnumerator = aggregatesByKey.GetEnumerator();
            while (aggregateEnumerator.TryGetNext(out var aggregate))
            {
                aggregates.Add(aggregate);
            }

            aggregatesByKey.Dispose();
            return aggregates;
        }

        public static NativeList<KeyValuePair<TKey, TAccumulate>>
            AggregateBy<TSource, TKey, TAccumulate, TEnumerator, TKeySelector, TSeedSelector, TAggregator>(
                TEnumerator source,
                TKeySelector keySelector,
                TSeedSelector seedSelector,
                TAggregator aggregator,
                AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TAccumulate : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TKey>
            where TSeedSelector : unmanaged, ISelector<TKey, TAccumulate>
            where TAggregator : unmanaged, IAggregator<TAccumulate, TSource>
        {
            var aggregatesByKey = new UnsafeHashMapSlim<TKey, TAccumulate>(DEFAULT_AGGREGATE_BY_CAPACITY, Allocator.Temp);

            while (source.MoveNext())
            {
                var value = source.Current;
                var key = keySelector.Select(in value);
                ref var aggregate = ref aggregatesByKey.GetValueRefOrAddDefault(key, out var exists);

                if (!exists)
                {
                    var seed = seedSelector.Select(in key);
                    aggregate = aggregator.Aggregate(in seed, in value);
                }
                else
                {
                    aggregate = aggregator.Aggregate(in aggregate, in value);
                }
            }

            source.Dispose();
            var aggregates = new NativeList<KeyValuePair<TKey, TAccumulate>>(aggregatesByKey.Count, allocator);
            var aggregateEnumerator = aggregatesByKey.GetEnumerator();
            while (aggregateEnumerator.TryGetNext(out var aggregate))
            {
                aggregates.Add(aggregate);
            }

            aggregatesByKey.Dispose();
            return aggregates;
        }
    }
}
