using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
#if ENTITIES
        public static Query<NativeArray<T>.Enumerator, T> AsQuery<T>(this DynamicBuffer<T> collection)
            where T : unmanaged
        {
            return new Query<NativeArray<T>.Enumerator, T>(collection.GetEnumerator());
        }
#endif
        
#if FA_CORE
        public static Query<KrasCore.UnsafeArray<T>.Enumerator, T> AsQuery<T>(this KrasCore.UnsafeArray<T> collection)
            where T : unmanaged
        {
            return new Query<KrasCore.UnsafeArray<T>.Enumerator, T>(collection.GetEnumerator());
        }
#endif

#if BL_CORE
        public static Query<BovineLabs.Core.Collections.UnsafeArray<T>.Enumerator, T> AsQuery<T>(this BovineLabs.Core.Collections.UnsafeArray<T> collection)
            where T : unmanaged
        {
            return new Query<BovineLabs.Core.Collections.UnsafeArray<T>.Enumerator, T>(collection.GetEnumerator());
        }
        
        public static Query<UnsafeHashMap<TKey, TValue>.Enumerator, KVPair<TKey, TValue>> AsQuery<TKey, TValue>(this UnsafeMultiHashMap<TKey, TValue> collection)
            where TKey : unmanaged, IEquatable<TKey>
            where TValue : unmanaged
        {
            return new Query<UnsafeHashMap<TKey, TValue>.Enumerator, KVPair<TKey, TValue>>(collection.GetEnumerator());
        }
#endif
        
        public static Query<NativeArray<T>.Enumerator, T> AsQuery<T>(this NativeArray<T> collection)
            where T : unmanaged
        {
            return new Query<NativeArray<T>.Enumerator, T>(collection.GetEnumerator());
        }
        
        public static Query<NativeSlice<T>.Enumerator, T> AsQuery<T>(this NativeSlice<T> collection)
            where T : unmanaged
        {
            return new Query<NativeSlice<T>.Enumerator, T>(collection.GetEnumerator());
        }
        
        public static Query<NativeArray<T>.Enumerator, T> AsQuery<T>(this NativeList<T> collection)
            where T : unmanaged
        {
            return new Query<NativeArray<T>.Enumerator, T>(collection.GetEnumerator());
        }

        public static Query<UnsafeList<T>.Enumerator, T> AsQuery<T>(this UnsafeList<T> collection)
            where T : unmanaged
        {
            return new Query<UnsafeList<T>.Enumerator, T>(collection.GetEnumerator());
        }

        public static Query<NativeQueue<T>.Enumerator, T> AsQuery<T>(this NativeQueue<T> collection)
            where T : unmanaged
        {
            return new Query<NativeQueue<T>.Enumerator, T>(collection.AsReadOnly().GetEnumerator());
        }

        public static Query<NativeHashSet<T>.Enumerator, T> AsQuery<T>(this NativeHashSet<T> collection)
            where T : unmanaged, IEquatable<T>
        {
            return new Query<NativeHashSet<T>.Enumerator, T>(collection.GetEnumerator());
        }

        public static Query<NativeParallelHashSet<T>.Enumerator, T> AsQuery<T>(this NativeParallelHashSet<T> collection)
            where T : unmanaged, IEquatable<T>
        {
            return new Query<NativeParallelHashSet<T>.Enumerator, T>(collection.GetEnumerator());
        }
    }
}
