using System.Buffers;
using FireAlt.BLinq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// System
[assembly: GenerateQueryExtensionFor(typeof(ReadOnlySequence<>), typeof(ReadOnlySequence<>.Enumerator))]

// UnityEngine.CoreModule
[assembly: GenerateQueryExtensionFor(typeof(NativeArray<>), typeof(NativeArray<>.Enumerator), LengthProperty = "Length", Indexer = true)]
[assembly: GenerateQueryExtensionFor(typeof(NativeArray<>.ReadOnly), typeof(NativeArray<>.ReadOnly.Enumerator), LengthProperty = "Length", Indexer = true)]
[assembly: GenerateQueryExtensionFor(typeof(NativeSlice<>), typeof(NativeSlice<>.Enumerator), LengthProperty = "Length", Indexer = true)]

// Unity.Collections
[assembly: GenerateQueryExtensionFor(typeof(NativeList<>), typeof(NativeArray<>.Enumerator), LengthProperty = "Length", Indexer = true)]
[assembly: GenerateQueryExtensionFor(typeof(UnsafeList<>), typeof(UnsafeList<>.Enumerator), LengthProperty = "Length", Indexer = true)]
[assembly: GenerateQueryExtensionFor(typeof(UnsafeList<>.ReadOnly), typeof(UnsafeList<>.Enumerator), LengthProperty = "Length", Indexer = true)]
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
[assembly: GenerateQueryExtensionFor(typeof(Unity.Entities.DynamicBuffer<>), typeof(NativeArray<>.Enumerator), LengthProperty = "Length", Indexer = true)]
#endif

#if FA_CORE
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.UnsafeArray<>), typeof(FireAlt.Core.Collections.UnsafeArray<>.Enumerator), LengthProperty = "Length", Indexer = true)]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.UnsafeThreadData<>), typeof(FireAlt.Core.Collections.UnsafeThreadData<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.NativeThreadData<>), typeof(FireAlt.Core.Collections.NativeThreadData<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.UnsafeThreadList<>), typeof(FireAlt.Core.Collections.UnsafeThreadList<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.NativeThreadList<>), typeof(FireAlt.Core.Collections.NativeThreadList<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.UnsafePriorityHeap<>), typeof(FireAlt.Core.Collections.UnsafePriorityHeap<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.NativePriorityHeap<>), typeof(FireAlt.Core.Collections.NativePriorityHeap<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.UnsafePriorityQueue<>), typeof(FireAlt.Core.Collections.UnsafePriorityQueue<>.Enumerator))]
[assembly: GenerateQueryExtensionFor(typeof(FireAlt.Core.Collections.NativePriorityQueue<>), typeof(FireAlt.Core.Collections.NativePriorityQueue<>.Enumerator))]
#endif

#if BL_CORE
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Collections.UnsafeArray<>), typeof(BovineLabs.Core.Collections.UnsafeArray<>.Enumerator), LengthProperty = "Length", Indexer = true)]
[assembly: GenerateQueryExtensionFor(typeof(BovineLabs.Core.Collections.UnsafeDynamicBuffer<>), typeof(NativeArray<>.Enumerator), LengthProperty = "Length", Indexer = true)]
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
