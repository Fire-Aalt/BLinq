using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using KrasCore;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        public NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
        {
            var list = ToNativeList(Allocator.Temp);
            return list.ToArray(allocator);
        }

        public UnsafeArray<T> ToUnsafeArray(Allocator allocator)
        {
            var list = ToNativeList(Allocator.Temp);
            var array = new UnsafeArray<T>(list.Length, allocator, NativeArrayOptions.UninitializedMemory);
            UnsafeArray<T>.Copy(list.AsArray(), array);
            return array;
        }

        public UnsafeList<T> ToUnsafeList(AllocatorManager.AllocatorHandle allocator)
        {
            var enumerator = GetEnumerator();
            var list = new UnsafeList<T>(0, allocator);
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }

            enumerator.Dispose();
            return list;
        }

        public NativeList<T> ToNativeList(AllocatorManager.AllocatorHandle allocator)
        {
            var enumerator = GetEnumerator();
            var list = new NativeList<T>(allocator);
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }

            enumerator.Dispose();
            return list;
        }

        public T[] ToManagedArray()
        {
            var list = ToNativeList(Allocator.Temp);
            return list.ToManagedArray();
        }

        public List<T> ToManagedList()
        {
            var enumerator = GetEnumerator();
            var list = new List<T>();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current);
            }

            enumerator.Dispose();
            return list;
        }
    }
}
