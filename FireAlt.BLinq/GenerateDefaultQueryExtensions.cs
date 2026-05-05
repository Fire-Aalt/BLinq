using FireAlt.BLinq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[assembly: GenerateQueryExtensionFor(typeof(NativeArray<>), typeof(NativeArray<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeArray<>.ReadOnly), typeof(NativeArray<>.ReadOnly.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeSlice<>), typeof(NativeSlice<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeList<>), typeof(NativeArray<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(UnsafeList<>), typeof(UnsafeList<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(UnsafeList<>.ReadOnly), typeof(UnsafeList<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeQueue<>.ReadOnly), typeof(NativeQueue<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeHashSet<>), typeof(NativeHashSet<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeHashSet<>.ReadOnly), typeof(NativeHashSet<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeHashMap<,>), typeof(NativeHashMap<,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeHashMap<,>.ReadOnly), typeof(NativeHashMap<,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeParallelHashSet<>), typeof(NativeParallelHashSet<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeParallelHashSet<>.ReadOnly), typeof(NativeParallelHashSet<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeParallelHashMap<,>), typeof(NativeParallelHashMap<,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeParallelHashMap<,>.ReadOnly), typeof(NativeParallelHashMap<,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeParallelMultiHashMap<,>), typeof(NativeParallelMultiHashMap<,>.KeyValueEnumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeParallelMultiHashMap<,>.ReadOnly), typeof(NativeParallelMultiHashMap<,>.KeyValueEnumerator))]

#if ENTITIES
[assembly: GenerateQueryExtensionFor(typeof(Unity.Entities.DynamicBuffer<>), typeof(NativeArray<>.Enumerator))]
#endif

#if FA_CORE
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.UnsafeArray<>), typeof(KrasCore.UnsafeArray<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.UnsafeThreadData<>), typeof(KrasCore.UnsafeThreadData<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.NativeThreadData<>), typeof(KrasCore.NativeThreadData<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.UnsafeThreadList<>), typeof(KrasCore.UnsafeThreadList<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.NativeThreadList<>), typeof(KrasCore.NativeThreadList<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.UnsafePriorityHeap<>), typeof(KrasCore.UnsafePriorityHeap<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.NativePriorityHeap<>), typeof(KrasCore.NativePriorityHeap<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.UnsafePriorityQueue<>), typeof(KrasCore.UnsafePriorityQueue<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(KrasCore.NativePriorityQueue<>), typeof(KrasCore.NativePriorityQueue<>.Enumerator))]
#endif

#if BL_CORE
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Collections.UnsafeArray<>), typeof(BovineLabs.Core.Collections.UnsafeArray<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Collections.UnsafeDynamicBuffer<>), typeof(NativeArray<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeMultiHashMap<,>), typeof(NativeHashMap<,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(NativeMultiHashMap<,>.ReadOnly), typeof(NativeHashMap<,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(UnsafeMultiHashMap<,>), typeof(UnsafeHashMap<,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Collections.BlobHashMap<,>), typeof(BovineLabs.Core.Collections.BlobHashMapEnumerator<,>))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Collections.BlobMultiHashMap<,>), typeof(BovineLabs.Core.Collections.BlobHashMapEnumerator<,>))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Iterators.DynamicHashMap<,>), typeof(BovineLabs.Core.Iterators.DynamicHashMapEnumerator<,>))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Iterators.DynamicMultiHashMap<,>), typeof(BovineLabs.Core.Iterators.DynamicHashMapEnumerator<,>))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Iterators.DynamicHashSet<>), typeof(BovineLabs.Core.Iterators.DynamicHashSetEnumerator<>))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Iterators.DynamicVariableMap<,,,>), typeof(BovineLabs.Core.Iterators.DynamicVariableMap<,,,>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Iterators.DynamicVariableMap<,,,,,>), typeof(BovineLabs.Core.Iterators.DynamicVariableMap<,,,,,>.Enumerator))]
#endif
