using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Counts elements by key.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <returns>A query of key and count pairs. Keys appear in the order they are first encountered.</returns>
        public Query<NativeArrayQueryEnumerator<KeyValuePair<TKey, int>>, KeyValuePair<TKey, int>> CountBy<TKey, TKeySelector>(
            TKeySelector keySelector)
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToRawQuery(
                BLinqUtilities.CountBy<T, TKey, TEnumerator, TKeySelector>(
                    GetEnumerator(),
                    keySelector,
                    Allocator.Temp));
        }

        /// <summary>
        /// Counts elements by key and stores the results in a native list.
        /// </summary>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>A list of key and count pairs. Keys appear in the order they are first encountered.</returns>
        public NativeList<KeyValuePair<TKey, int>> ToCountedBy<TKey, TKeySelector>(
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.CountBy<T, TKey, TEnumerator, TKeySelector>(
                GetEnumerator(),
                keySelector,
                allocator);
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Counts elements by key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <returns>A query of key and count pairs. Keys appear in the order they are first encountered.</returns>
        public static Query<NativeArrayQueryEnumerator<KeyValuePair<TKey, int>>, KeyValuePair<TKey, int>> CountBy<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToRawQuery(
                BLinqUtilities.CountBy<T, TKey, TEnumerator, TKeySelector>(
                    source.GetEnumerator(),
                    keySelector,
                    Allocator.Temp));
        }

        /// <summary>
        /// Counts elements by key and stores the results in a native list.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>A list of key and count pairs. Keys appear in the order they are first encountered.</returns>
        public static NativeList<KeyValuePair<TKey, int>> ToCountedBy<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.CountBy<T, TKey, TEnumerator, TKeySelector>(
                source.GetEnumerator(),
                keySelector,
                allocator);
        }

        /// <summary>
        /// Counts elements by key.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <returns>A query of key and count pairs. Keys appear in the order they are first encountered.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Query<NativeArrayQueryEnumerator<KeyValuePair<TKey, int>>, KeyValuePair<TKey, int>> CountBy<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<Query<NativeArrayQueryEnumerator<KeyValuePair<TKey, int>>, KeyValuePair<TKey, int>>>();
        }

        /// <summary>
        /// Counts elements by key and stores the results in a native list.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="keySelector">The function used to compute a key for each element.</param>
        /// <param name="allocator">Allocator used for the result storage.</param>
        /// <returns>A list of key and count pairs. Keys appear in the order they are first encountered.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeList<KeyValuePair<TKey, int>> ToCountedBy<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<NativeList<KeyValuePair<TKey, int>>>();
        }
    }

    internal static partial class BLinqUtilities
    {
        private const int DEFAULT_COUNT_BY_CAPACITY = 64;

        public static NativeList<KeyValuePair<TKey, int>> CountBy<TSource, TKey, TEnumerator, TKeySelector>(
            TEnumerator source,
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where TSource : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IQueryEnumerator<TSource>
            where TKeySelector : unmanaged, ISelector<TSource, TKey>
        {
            var keyToIndex = new UnsafeHashMapSlim<TKey, int>(DEFAULT_COUNT_BY_CAPACITY, Allocator.Temp);
            var counts = new NativeList<KeyValuePair<TKey, int>>(DEFAULT_COUNT_BY_CAPACITY, allocator);

            while (source.MoveNext())
            {
                var value = source.Current;
                var key = keySelector.Select(in value);
                ref var index = ref keyToIndex.GetValueRefOrAddDefault(key, out var exists);

                if (!exists)
                {
                    index = counts.Length;
                    counts.Add(new KeyValuePair<TKey, int>(key, 1));
                }
                else
                {
                    var count = counts[index];
                    counts[index] = new KeyValuePair<TKey, int>(count.Key, checked(count.Value + 1));
                }
            }

            source.Dispose();
            return counts;
        }
    }
}
