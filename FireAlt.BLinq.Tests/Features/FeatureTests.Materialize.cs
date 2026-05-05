using System.Linq;
using NUnit.Framework;
using FireAlt.BLinq;
using KrasCore;
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
            var unsafeArray = input.AsQuery().ToUnsafeArray(Allocator.Temp);
            var unsafeList = input.AsQuery().ToUnsafeList(Allocator.Temp);
            var managedArray = input.AsQuery().ToManagedArray();
            var managedList = input.AsQuery().ToManagedList();

            AssertSequence(array, expected);
            AssertSequence(unsafeArray, expected);
            AssertSequence(unsafeList, expected);
            CollectionAssert.AreEqual(expected, managedArray);
            CollectionAssert.AreEqual(expected, managedList);
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

        private static void AssertSequence<T>(NativeList<T> actual, T[] expected)
            where T : unmanaged
        {
            AssertSequence(actual.AsArray(), expected);
        }

        private static void AssertSequence<T>(UnsafeArray<T> actual, T[] expected)
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

        private static void AssertLookup<TKey, T>(Lookup<TKey, T> actual, System.Linq.IGrouping<TKey, T>[] expected)
            where TKey : unmanaged, System.IEquatable<TKey>
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
