using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
#if ENTITIES
        public static Query<T, NativeArray<T>.Enumerator> AsQuery<T>(this DynamicBuffer<T> collection)
            where T : unmanaged
        {
            return new Query<T, NativeArray<T>.Enumerator>(collection.GetEnumerator());
        }
#endif
        
#if FA_CORE
        public static Query<T, KrasCore.UnsafeArray<T>.Enumerator> AsQuery<T>(this KrasCore.UnsafeArray<T> collection)
            where T : unmanaged
        {
            return new Query<T, KrasCore.UnsafeArray<T>.Enumerator>(collection.GetEnumerator());
        }
#endif

#if BL_CORE
        public static Query<T, BovineLabs.Core.Collections.UnsafeArray<T>.Enumerator> AsQuery<T>(this BovineLabs.Core.Collections.UnsafeArray<T> collection)
            where T : unmanaged
        {
            return new Query<T, BovineLabs.Core.Collections.UnsafeArray<T>.Enumerator>(collection.GetEnumerator());
        }
        
        public static Query<KVPair<TKey, TValue>, UnsafeHashMap<TKey, TValue>.Enumerator> AsQuery<TKey, TValue>(this UnsafeMultiHashMap<TKey, TValue> collection)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            return new Query<KVPair<TKey, TValue>, UnsafeHashMap<TKey, TValue>.Enumerator>(collection.GetEnumerator());
        }
#endif
        
        public static Query<T, NativeArray<T>.Enumerator> AsQuery<T>(this NativeArray<T> collection)
            where T : unmanaged
        {
            return new Query<T, NativeArray<T>.Enumerator>(collection.GetEnumerator());
        }
        
        public static Query<T, NativeSlice<T>.Enumerator> AsQuery<T>(this NativeSlice<T> collection)
            where T : unmanaged
        {
            return new Query<T, NativeSlice<T>.Enumerator>(collection.GetEnumerator());
        }
        
        public static Query<T, NativeArray<T>.Enumerator> AsQuery<T>(this NativeList<T> collection)
            where T : unmanaged
        {
            return new Query<T, NativeArray<T>.Enumerator>(collection.GetEnumerator());
        }

        public static Query<T, UnsafeList<T>.Enumerator> AsQuery<T>(this UnsafeList<T> collection)
            where T : unmanaged
        {
            return new Query<T, UnsafeList<T>.Enumerator>(collection.GetEnumerator());
        }

        public static Query<T, NativeQueue<T>.Enumerator> AsQuery<T>(this NativeQueue<T> collection)
            where T : unmanaged
        {
            return new Query<T, NativeQueue<T>.Enumerator>(collection.AsReadOnly().GetEnumerator());
        }

        public static Query<T, NativeHashSet<T>.Enumerator> AsQuery<T>(this NativeHashSet<T> collection)
            where T : unmanaged, IEquatable<T>
        {
            return new Query<T, NativeHashSet<T>.Enumerator>(collection.GetEnumerator());
        }

        public static Query<T, NativeParallelHashSet<T>.Enumerator> AsQuery<T>(this NativeParallelHashSet<T> collection)
            where T : unmanaged, IEquatable<T>
        {
            return new Query<T, NativeParallelHashSet<T>.Enumerator>(collection.GetEnumerator());
        }
    }
}
