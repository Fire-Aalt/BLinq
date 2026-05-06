using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Materialize_ReturnsRequestedCollectionTypes()
        {
            var input = new NativeArray<int>(new[] { 0, 1, 2, 3 }, Allocator.Temp);
            var expected = input.Select(value => value).ToArray();
            var array = input.AsQuery().ToNativeArray(Allocator.Temp);
            var unsafeList = input.AsQuery().ToUnsafeList(Allocator.Temp);
            var managedArray = input.AsQuery().ToManagedArray();
            var managedList = input.AsQuery().ToManagedList();

            AssertSequence(array, expected);
            AssertSequence(unsafeList, expected);
            CollectionAssert.AreEqual(expected, managedArray);
            CollectionAssert.AreEqual(expected, managedList);
        }

        [Test]
        public void Materialize_OrderedQuery_ReturnsRequestedCollectionTypes()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 2, 0 }, Allocator.Temp);
            var expected = input.OrderBy(value => value).ToArray();
            var ordered = input.AsQuery().OrderBy();
            var array = ordered.ToNativeArray(Allocator.Temp);
            var unsafeList = ordered.ToUnsafeList(Allocator.Temp);
            var managedArray = ordered.ToManagedArray();
            var managedList = ordered.ToManagedList();

            AssertSequence(array, expected);
            AssertSequence(unsafeList, expected);
            CollectionAssert.AreEqual(expected, managedArray);
            CollectionAssert.AreEqual(expected, managedList);
        }

        [Test]
        public void ToManagedDictionary_UsesDelegateSelectors()
        {
            var input = CreatePairs();

            var dictionary = input.AsQuery()
                .ToManagedDictionary(value => value.Key, value => value.Value);

            AssertDictionary(dictionary, new[] { 100, 200, 300 });
        }

        [Test]
        public void ToManagedDictionary_OrderedQuery_UsesDelegateSelectors()
        {
            var input = CreatePairs();

            var dictionary = input.AsQuery()
                .OrderBy(value => value.Sort)
                .ToManagedDictionary(value => value.Key, value => value.Value);

            AssertDictionary(dictionary, new[] { 100, 200, 300 });
        }

        [Test]
        public void ToManagedDictionary_DuplicateKeysThrow()
        {
            var input = new NativeArray<Pair>(
                new[]
                {
                    new Pair(1, 100, 0),
                    new Pair(1, 200, 1),
                },
                Allocator.Temp);

            Assert.Throws<ArgumentException>(() => input.AsQuery()
                .ToManagedDictionary(value => value.Key, value => value.Value));
            Assert.Throws<ArgumentException>(() => input.AsQuery()
                .OrderBy(value => value.Sort)
                .ToManagedDictionary(value => value.Key, value => value.Value));
        }

        [Test]
        public void ToManagedHashSet_MatchesLinqToHashSet()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 3, 2, 1 }, Allocator.Temp);
            var expected = input.ToHashSet();

            var queryHashSet = input.AsQuery().ToManagedHashSet();
            var orderedHashSet = input.AsQuery().OrderBy().ToManagedHashSet();

            CollectionAssert.AreEquivalent(expected, queryHashSet);
            CollectionAssert.AreEquivalent(expected, orderedHashSet);
        }

        [Test]
        public void ToNativeHashMap_UsesDelegateSelectors()
        {
            var input = CreatePairs();
            var hashMap = input.AsQuery()
                .ToNativeHashMap(value => value.Key, value => value.Value, Allocator.Temp);

            AssertNativeHashMap(hashMap, new[] { 100, 200, 300 });
            hashMap.Dispose();
        }

        [Test]
        public void ToNativeHashMap_OrderedQuery_UsesNestedDelegateSelectors()
        {
            var input = CreatePairs();
            var hashMap = input.AsQuery()
                .OrderBy(value => value.Sort)
                .ToNativeHashMap(value => value.Key, value => value.Value, Allocator.Temp);

            AssertNativeHashMap(hashMap, new[] { 100, 200, 300 });
            hashMap.Dispose();
        }

        [Test]
        public void ToNativeHashMap_DuplicateKeysThrow()
        {
            var input = new NativeArray<Pair>(
                new[]
                {
                    new Pair(1, 100, 0),
                    new Pair(1, 200, 1),
                },
                Allocator.Temp);

            Assert.Throws<ArgumentException>(() => input.AsQuery()
                .ToNativeHashMap(value => value.Key, value => value.Value, Allocator.Temp));
            Assert.Throws<ArgumentException>(() => input.AsQuery()
                .OrderBy(value => value.Sort)
                .ToNativeHashMap(value => value.Key, value => value.Value, Allocator.Temp));
        }

        [Test]
        public void ToNativeHashSet_MatchesHashSetSemantics()
        {
            var input = new NativeArray<int>(new[] { 3, 1, 3, 2, 1 }, Allocator.Temp);
            var queryHashSet = input.AsQuery().ToNativeHashSet(Allocator.Temp);
            var orderedHashSet = input.AsQuery().OrderBy().ToNativeHashSet(Allocator.Temp);

            AssertNativeHashSet(queryHashSet, new[] { 1, 2, 3 });
            AssertNativeHashSet(orderedHashSet, new[] { 1, 2, 3 });
            queryHashSet.Dispose();
            orderedHashSet.Dispose();
        }

        private static void AssertSequence<T>(NativeArray<T> actual, T[] expected)
            where T : unmanaged
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Length));

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That(actual[i], Is.EqualTo(expected[i]));
            }
        }

        private static void AssertSequence<T>(UnsafeList<T> actual, T[] expected)
            where T : unmanaged
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Length));

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.That(actual[i], Is.EqualTo(expected[i]));
            }
        }

        private static NativeArray<Pair> CreatePairs()
        {
            return new NativeArray<Pair>(
                new[]
                {
                    new Pair(2, 200, 1),
                    new Pair(1, 100, 2),
                    new Pair(3, 300, 0),
                },
                Allocator.Temp);
        }

        private static void AssertDictionary(Dictionary<int, int> actual, int[] expectedValues)
        {
            Assert.That(actual.Count, Is.EqualTo(expectedValues.Length));

            for (var i = 0; i < expectedValues.Length; i++)
            {
                Assert.That(actual.TryGetValue(i + 1, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expectedValues[i]));
            }
        }

        private static void AssertNativeHashMap(NativeHashMap<int, int> actual, int[] expectedValues)
        {
            Assert.That(actual.Count, Is.EqualTo(expectedValues.Length));

            for (var i = 0; i < expectedValues.Length; i++)
            {
                Assert.That(actual.TryGetValue(i + 1, out var value), Is.True);
                Assert.That(value, Is.EqualTo(expectedValues[i]));
            }
        }

        private static void AssertNativeHashSet(NativeHashSet<int> actual, int[] expectedValues)
        {
            Assert.That(actual.Count, Is.EqualTo(expectedValues.Length));

            foreach (var value in expectedValues)
            {
                Assert.That(actual.Contains(value), Is.True);
            }
        }

        private struct Pair
        {
            public int Key;
            public int Value;
            public int Sort;

            public Pair(int key, int value, int sort)
            {
                Key = key;
                Value = value;
                Sort = sort;
            }
        }
        
        private static void AssertLookup<TKey, T>(Lookup<TKey, T> actual, IGrouping<TKey, T>[] expected)
            where TKey : unmanaged, IEquatable<TKey>
            where T : unmanaged
        {
            var expectedValueCount = expected.Sum(group => group.Count());
            Assert.That(actual.GroupCount, Is.EqualTo(expected.Length));
            Assert.That(actual.ValueCount, Is.EqualTo(expectedValueCount));

            for (var i = 0; i < expected.Length; i++)
            {
                var expectedGroup = expected[i].ToArray();
                Assert.That(actual[i].Key, Is.EqualTo(expected[i].Key));
                Assert.That(actual[i].Length, Is.EqualTo(expectedGroup.Length));

                for (var j = 0; j < expectedGroup.Length; j++)
                {
                    Assert.That(actual[i][j], Is.EqualTo(expectedGroup[j]));
                }
            }
        }
    }
}
