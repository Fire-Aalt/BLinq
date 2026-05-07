using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
        {
            return BLinqUtilities.ToNativeArray<T, TEnumerator>(GetEnumerator(), allocator);
        }

        public UnsafeList<T> ToUnsafeList(AllocatorManager.AllocatorHandle allocator)
        {
            return BLinqUtilities.ToUnsafeList<T, TEnumerator>(GetEnumerator(), allocator);
        }

        public NativeList<T> ToNativeList(AllocatorManager.AllocatorHandle allocator)
        {
            return BLinqUtilities.ToNativeList<T, TEnumerator>(GetEnumerator(), allocator);
        }

        public T[] ToManagedArray()
        {
            return BLinqUtilities.ToManagedArray<T, TEnumerator>(GetEnumerator());
        }

        public List<T> ToManagedList()
        {
            return BLinqUtilities.ToManagedList<T, TEnumerator>(GetEnumerator());
        }

        public Dictionary<TKey, T> ToManagedDictionary<TKey, TKeySelector>(TKeySelector keySelector)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToManagedDictionary<T, TKey, T, TEnumerator, TKeySelector, IdentitySelector<T>>(
                GetEnumerator(),
                keySelector,
                new IdentitySelector<T>(),
                null);
        }

        public Dictionary<TKey, T> ToManagedDictionary<TKey, TKeySelector>(
            TKeySelector keySelector,
            IEqualityComparer<TKey> comparer)
            where TKey : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToManagedDictionary<T, TKey, T, TEnumerator, TKeySelector, IdentitySelector<T>>(
                GetEnumerator(),
                keySelector,
                new IdentitySelector<T>(),
                comparer);
        }

        public Dictionary<TKey, TValue> ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(
            TKeySelector keySelector,
            TValueSelector valueSelector)
            where TKey : unmanaged
            where TValue : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return BLinqUtilities.ToManagedDictionary<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
                GetEnumerator(),
                keySelector,
                valueSelector,
                null);
        }

        public Dictionary<TKey, TValue> ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(
            TKeySelector keySelector,
            TValueSelector valueSelector,
            IEqualityComparer<TKey> comparer)
            where TKey : unmanaged
            where TValue : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return BLinqUtilities.ToManagedDictionary<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
                GetEnumerator(),
                keySelector,
                valueSelector,
                comparer);
        }

        public HashSet<T> ToManagedHashSet()
        {
            return BLinqUtilities.ToManagedHashSet<T, TEnumerator>(GetEnumerator(), null);
        }

        public HashSet<T> ToManagedHashSet(IEqualityComparer<T> comparer)
        {
            return BLinqUtilities.ToManagedHashSet<T, TEnumerator>(GetEnumerator(), comparer);
        }

        public NativeHashMap<TKey, T> ToNativeHashMap<TKey, TKeySelector>(
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return BLinqUtilities.ToNativeHashMap<T, TKey, T, TEnumerator, TKeySelector, IdentitySelector<T>>(
                GetEnumerator(),
                keySelector,
                new IdentitySelector<T>(),
                allocator);
        }

        public NativeHashMap<TKey, TValue> ToNativeHashMap<TKey, TValue, TKeySelector, TValueSelector>(
            TKeySelector keySelector,
            TValueSelector valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return BLinqUtilities.ToNativeHashMap<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
                GetEnumerator(),
                keySelector,
                valueSelector,
                allocator);
        }

    }

    public static partial class BLinqExtensions
    {
        public static NativeHashSet<T> ToNativeHashSet<T, TEnumerator>(
            this Query<TEnumerator, T> source,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return BLinqUtilities.ToNativeHashSet<T, TEnumerator>(source.GetEnumerator(), allocator);
        }

        public static NativeHashSet<T> ToNativeHashSet<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return BLinqUtilities.ToSortedNativeHashSet<T, TEnumerator, TComparer>(
                source._source.GetEnumerator(),
                source._comparer,
                allocator);
        }

        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.ToManagedDictionary<TKey, TKeySelector>(keySelector);
        }

        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.ToManagedDictionary<TKey, TKeySelector>(keySelector, comparer);
        }

        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TValueSelector valueSelector)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return source.ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(keySelector, valueSelector);
        }

        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return source.ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(keySelector, valueSelector, comparer);
        }

        public static NativeHashMap<TKey, T> ToNativeHashMap<T, TKey, TEnumerator, TKeySelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.ToNativeHashMap<TKey, TKeySelector>(keySelector, allocator);
        }

        public static NativeHashMap<TKey, TValue> ToNativeHashMap<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
            this Query<TEnumerator, T> source,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return source.ToNativeHashMap<TKey, TValue, TKeySelector, TValueSelector>(keySelector, valueSelector, allocator);
        }

        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator, TComparer, TKeySelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.ToManagedDictionary<TKey, TKeySelector>(keySelector);
        }

        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator, TComparer, TKeySelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.ToManagedDictionary<TKey, TKeySelector>(keySelector, comparer);
        }

        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector,
            TValueSelector valueSelector)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return source.ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(keySelector, valueSelector);
        }

        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return source.ToManagedDictionary<TKey, TValue, TKeySelector, TValueSelector>(keySelector, valueSelector, comparer);
        }

        public static NativeHashMap<TKey, T> ToNativeHashMap<T, TKey, TEnumerator, TComparer, TKeySelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
        {
            return source.ToNativeHashMap<TKey, TKeySelector>(keySelector, allocator);
        }

        public static NativeHashMap<TKey, TValue> ToNativeHashMap<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            return source.ToNativeHashMap<TKey, TValue, TKeySelector, TValueSelector>(keySelector, valueSelector, allocator);
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Dictionary<TKey, T>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Dictionary<TKey, T>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Dictionary<TKey, TValue>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<Dictionary<TKey, TValue>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeHashMap<TKey, T> ToNativeHashMap<T, TKey, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<NativeHashMap<TKey, T>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeHashMap<TKey, TValue> ToNativeHashMap<T, TKey, TValue, TEnumerator>(
            this Query<TEnumerator, T> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<NativeHashMap<TKey, TValue>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<Dictionary<TKey, T>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, T> ToManagedDictionary<T, TKey, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<Dictionary<TKey, T>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<Dictionary<TKey, TValue>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<Dictionary<TKey, TValue>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeHashMap<TKey, T> ToNativeHashMap<T, TKey, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<NativeHashMap<TKey, T>>();
        }

        [NativeDelegateMethod(typeof(ISelector<,>), typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static NativeHashMap<TKey, TValue> ToNativeHashMap<T, TKey, TValue, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, TKey> keySelector,
            Func<T, TValue> valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<NativeHashMap<TKey, TValue>>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static NativeArray<T> ToNativeArray<T, TEnumerator>(
            TEnumerator source,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var list = ToNativeList<T, TEnumerator>(source, Allocator.Temp);
            var array = list.ToArray(allocator);
            list.Dispose();
            return array;
        }

        public static UnsafeList<T> ToUnsafeList<T, TEnumerator>(
            TEnumerator source,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var list = new UnsafeList<T>(0, allocator);
            while (source.MoveNext())
            {
                list.Add(source.Current);
            }

            source.Dispose();
            return list;
        }

        public static NativeList<T> ToNativeList<T, TEnumerator>(
            TEnumerator source,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var list = new NativeList<T>(allocator);
            while (source.MoveNext())
            {
                list.Add(source.Current);
            }

            source.Dispose();
            return list;
        }

        public static T[] ToManagedArray<T, TEnumerator>(TEnumerator source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var list = ToNativeList<T, TEnumerator>(source, Allocator.Temp);
            var array = ToManagedArray(list);
            list.Dispose();
            return array;
        }

        public static List<T> ToManagedList<T, TEnumerator>(TEnumerator source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var list = new List<T>();
            while (source.MoveNext())
            {
                list.Add(source.Current);
            }

            source.Dispose();
            return list;
        }

        public static NativeArray<T> ToSortedNativeArray<T, TEnumerator, TComparer>(
            TEnumerator source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToSortedNativeList<T, TEnumerator, TComparer>(source, comparer, Allocator.Temp);
            var array = list.ToArray(allocator);
            list.Dispose();
            return array;
        }

        public static UnsafeList<T> ToSortedUnsafeList<T, TEnumerator, TComparer>(
            TEnumerator source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToUnsafeList<T, TEnumerator>(source, allocator);
            StableSort(ref list, comparer);
            return list;
        }

        public static NativeList<T> ToSortedNativeList<T, TEnumerator, TComparer>(
            TEnumerator source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToNativeList<T, TEnumerator>(source, allocator);
            StableSort(list, comparer);
            return list;
        }

        public static void StableSort<T, TComparer>(NativeList<T> list, TComparer comparer)
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            if (list.Length < 2)
            {
                return;
            }

            var entries = new NativeArray<StableSortEntry<T>>(list.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < list.Length; i++)
            {
                entries[i] = new StableSortEntry<T>(list[i], i);
            }

            entries.Sort(new StableSortEntryComparer<T, TComparer>(comparer));

            for (var i = 0; i < entries.Length; i++)
            {
                list[i] = entries[i].Value;
            }

            entries.Dispose();
        }

        public static void StableSort<T, TComparer>(ref UnsafeList<T> list, TComparer comparer)
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            if (list.Length < 2)
            {
                return;
            }

            var entries = new NativeArray<StableSortEntry<T>>(list.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < list.Length; i++)
            {
                entries[i] = new StableSortEntry<T>(list[i], i);
            }

            entries.Sort(new StableSortEntryComparer<T, TComparer>(comparer));

            for (var i = 0; i < entries.Length; i++)
            {
                list[i] = entries[i].Value;
            }

            entries.Dispose();
        }

        private readonly struct StableSortEntry<T>
            where T : unmanaged
        {
            public readonly T Value;
            public readonly int Index;

            public StableSortEntry(T value, int index)
            {
                Value = value;
                Index = index;
            }
        }

        private struct StableSortEntryComparer<T, TComparer> : IComparer<StableSortEntry<T>>
            where T : unmanaged
            where TComparer : unmanaged, IComparer<T>
        {
            private TComparer _comparer;

            public StableSortEntryComparer(TComparer comparer)
            {
                _comparer = comparer;
            }

            public int Compare(StableSortEntry<T> x, StableSortEntry<T> y)
            {
                var result = _comparer.Compare(x.Value, y.Value);
                return result != 0 ? result : x.Index.CompareTo(y.Index);
            }
        }

        public static T[] ToSortedManagedArray<T, TEnumerator, TComparer>(
            TEnumerator source,
            TComparer comparer)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToSortedNativeList<T, TEnumerator, TComparer>(source, comparer, Allocator.Temp);
            var array = ToManagedArray(list);
            list.Dispose();
            return array;
        }

        private static T[] ToManagedArray<T>(NativeList<T> list)
            where T : unmanaged
        {
            var array = new T[list.Length];
            for (var i = 0; i < list.Length; i++)
            {
                array[i] = list[i];
            }

            return array;
        }

        public static List<T> ToSortedManagedList<T, TEnumerator, TComparer>(
            TEnumerator source,
            TComparer comparer)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            var array = ToSortedManagedArray<T, TEnumerator, TComparer>(source, comparer);
            return new List<T>(array);
        }

        public static Dictionary<TKey, TValue> ToManagedDictionary<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
            TEnumerator source,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            IEqualityComparer<TKey> comparer)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            var dictionary = new Dictionary<TKey, TValue>(comparer);
            try
            {
                while (source.MoveNext())
                {
                    var value = source.Current;
                    dictionary.Add(keySelector.Select(in value), valueSelector.Select(in value));
                }
            }
            finally
            {
                source.Dispose();
            }

            return dictionary;
        }

        public static Dictionary<TKey, TValue> ToSortedManagedDictionary<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
            TEnumerator source,
            TComparer comparer,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            IEqualityComparer<TKey> equalityComparer)
            where T : unmanaged
            where TKey : unmanaged
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            var list = ToSortedNativeList<T, TEnumerator, TComparer>(source, comparer, Allocator.Temp);
            try
            {
                return ToManagedDictionary<T, TKey, TValue, NativeArray<T>.Enumerator, TKeySelector, TValueSelector>(
                    list.GetEnumerator(),
                    keySelector,
                    valueSelector,
                    equalityComparer);
            }
            finally
            {
                list.Dispose();
            }
        }

        public static HashSet<T> ToManagedHashSet<T, TEnumerator>(
            TEnumerator source,
            IEqualityComparer<T> comparer)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var hashSet = new HashSet<T>(comparer);
            try
            {
                while (source.MoveNext())
                {
                    hashSet.Add(source.Current);
                }
            }
            finally
            {
                source.Dispose();
            }

            return hashSet;
        }

        public static HashSet<T> ToSortedManagedHashSet<T, TEnumerator, TComparer>(
            TEnumerator source,
            TComparer comparer,
            IEqualityComparer<T> equalityComparer)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToSortedNativeList<T, TEnumerator, TComparer>(source, comparer, Allocator.Temp);
            try
            {
                return ToManagedHashSet<T, NativeArray<T>.Enumerator>(list.GetEnumerator(), equalityComparer);
            }
            finally
            {
                list.Dispose();
            }
        }

        public static NativeHashMap<TKey, TValue> ToNativeHashMap<T, TKey, TValue, TEnumerator, TKeySelector, TValueSelector>(
            TEnumerator source,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            var list = ToNativeList<T, TEnumerator>(source, Allocator.Temp);
            var hashMap = new NativeHashMap<TKey, TValue>(list.Length, allocator);
            try
            {
                for (var i = 0; i < list.Length; i++)
                {
                    var value = list[i];
                    var key = keySelector.Select(in value);
                    if (!hashMap.TryAdd(key, valueSelector.Select(in value)))
                    {
                        throw new ArgumentException("An item with the same key has already been added.");
                    }
                }
            }
            catch
            {
                hashMap.Dispose();
                throw;
            }
            finally
            {
                list.Dispose();
            }

            return hashMap;
        }

        public static NativeHashMap<TKey, TValue> ToSortedNativeHashMap<T, TKey, TValue, TEnumerator, TComparer, TKeySelector, TValueSelector>(
            TEnumerator source,
            TComparer comparer,
            TKeySelector keySelector,
            TValueSelector valueSelector,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TKeySelector : unmanaged, ISelector<T, TKey>
            where TValueSelector : unmanaged, ISelector<T, TValue>
        {
            var list = ToSortedNativeList<T, TEnumerator, TComparer>(source, comparer, Allocator.Temp);
            try
            {
                return ToNativeHashMap<T, TKey, TValue, NativeArray<T>.Enumerator, TKeySelector, TValueSelector>(
                    list.GetEnumerator(),
                    keySelector,
                    valueSelector,
                    allocator);
            }
            finally
            {
                list.Dispose();
            }
        }

        public static NativeHashSet<T> ToNativeHashSet<T, TEnumerator>(
            TEnumerator source,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var list = ToNativeList<T, TEnumerator>(source, Allocator.Temp);
            var hashSet = new NativeHashSet<T>(list.Length, allocator);
            for (var i = 0; i < list.Length; i++)
            {
                hashSet.Add(list[i]);
            }

            list.Dispose();
            return hashSet;
        }

        public static NativeHashSet<T> ToSortedNativeHashSet<T, TEnumerator, TComparer>(
            TEnumerator source,
            TComparer comparer,
            AllocatorManager.AllocatorHandle allocator)
            where T : unmanaged, IEquatable<T>
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            var list = ToSortedNativeList<T, TEnumerator, TComparer>(source, comparer, Allocator.Temp);
            try
            {
                return ToNativeHashSet<T, NativeArray<T>.Enumerator>(list.GetEnumerator(), allocator);
            }
            finally
            {
                list.Dispose();
            }
        }
    }
}
