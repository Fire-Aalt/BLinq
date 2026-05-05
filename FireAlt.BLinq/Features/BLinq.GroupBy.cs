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
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <returns>
        /// A new query whose elements are groups keyed by the selected value, preserving the order in which
        /// each key first appears and the order of elements inside each group.
        /// </returns>
        public Query<LookupEnumerator<TKey, T>, Group<TKey, T>> GroupBy<TKey, TKeySelector>(
            TKeySelector keySelector)
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.GroupBy<T, TKey, TEnumerator, TKeySelector>(
                GetEnumerator(),
                keySelector,
                Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="allocator">Allocator used for the lookup storage.</param>
        /// <returns>
        /// A lookup containing groups keyed by the selected value, preserving the order in which each key first
        /// appears and the order of elements inside each group.
        /// </returns>
        public Lookup<TKey, T> ToLookup<TKey, TKeySelector>(
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.GroupBy<T, TKey, TEnumerator, TKeySelector>(
                GetEnumerator(),
                keySelector,
                allocator);
        }
    }
    
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <returns>
        /// A new query whose elements are groups keyed by the selected value, preserving the order in which
        /// each key first appears and the order of elements inside each group.
        /// </returns>
        public static Query<LookupEnumerator<TKey, T>, Group<TKey, T>> GroupBy<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.GroupBy<T, TKey, TEnumerator, TKeySelector>(
                source.GetEnumerator(),
                keySelector,
                Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <returns>
        /// A new query whose elements are groups keyed by the selected value, preserving the order in which
        /// each key first appears and the order of elements inside each group.
        /// </returns>
        public static Query<LookupEnumerator<T, T>, Group<T, T>> GroupBy<T, TEnumerator, TKeySelector>(this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, T>
        {
            return BLinqUtilities.GroupBy<T, T, TEnumerator, TKeySelector>(
                source.GetEnumerator(),
                keySelector,
                Allocator.Temp).AsQuery();
        }

        /// <summary>
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="allocator">Allocator used for the lookup storage.</param>
        /// <returns>
        /// A lookup containing groups keyed by the selected value, preserving the order in which each key first
        /// appears and the order of elements inside each group.
        /// </returns>
        public static Lookup<TKey, T> ToLookup<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.GroupBy<T, TKey, TEnumerator, TKeySelector>(
                source.GetEnumerator(),
                keySelector,
                allocator);
        }

        /// <summary>
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="allocator">Allocator used for the lookup storage.</param>
        /// <returns>
        /// A lookup containing groups keyed by the selected value, preserving the order in which each key first
        /// appears and the order of elements inside each group.
        /// </returns>
        public static Lookup<T, T> ToLookup<T, TEnumerator, TKeySelector>(this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, T>
        {
            return BLinqUtilities.GroupBy<T, T, TEnumerator, TKeySelector>(
                source.GetEnumerator(),
                keySelector,
                allocator);
        }
        
        /// <summary>
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <returns>
        /// A new query whose elements are groups keyed by the selected value, preserving the order in which
        /// each key first appears and the order of elements inside each group.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<LookupEnumerator<TKey, T>, Group<TKey, T>> GroupBy<T, TKey, TEnumerator>(
                this Query<TEnumerator, T> source, Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Query<LookupEnumerator<TKey, T>, Group<TKey, T>>>();
        }
        
        /// <summary>
        /// Groups the elements of the query by key and returns the grouped results.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="allocator">Allocator used for the lookup storage.</param>
        /// <returns>
        /// A lookup containing groups keyed by the selected value, preserving the order in which each key first
        /// appears and the order of elements inside each group.
        /// </returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Lookup<TKey, T> ToLookup<T, TKey, TEnumerator>(
                this Query<TEnumerator, T> source,
                Func<T, TKey> keySelector,
                AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Lookup<TKey, T>>();
        }
    }

    internal static partial class BLinqUtilities
    {
        private const int DEFAULT_GROUP_BY_CAPACITY = 64;

        public static Lookup<TKey, T> GroupBy<T, TKey, TEnumerator, TKeySelector>(
            TEnumerator source,
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            var keyToGroupIndex = new UnsafeHashMapSlim<TKey, int>(DEFAULT_GROUP_BY_CAPACITY, Allocator.Temp);
            var groups = new NativeList<Group<TKey, T>>(DEFAULT_GROUP_BY_CAPACITY, allocator);
            var valueCount = 0;

            while (source.MoveNext())
            {
                var value = source.Current;
                var key = keySelector.Select(in value);
                ref var groupIndex = ref keyToGroupIndex.GetValueRefOrAddDefault(key, out var exists);

                if (!exists)
                {
                    groupIndex = groups.Length;
                    groups.Add(new Group<TKey, T>(key, value, allocator));
                }
                else
                {
                    ref var group = ref groups.ElementAt(groupIndex);
                    group.Add(in value);
                }

                valueCount++;
            }

            source.Dispose();
            keyToGroupIndex.Dispose();

            return new Lookup<TKey, T>(groups, valueCount);
        }
    }
}
