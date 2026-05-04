using NUnit.Framework;
using Unity.Collections;
// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Materialize_ReturnsRequestedCollectionTypes()
        {
            var input = new NativeArray<int>(new[] { 0, 1, 2, 3 }, Allocator.Temp);
            var array = input.AsQuery().ToNativeArray(Allocator.Temp);
            var unsafeArray = input.AsQuery().ToUnsafeArray(Allocator.Temp);
            var unsafeList = input.AsQuery().ToUnsafeList(Allocator.Temp);
            var managedArray = input.AsQuery().ToManagedArray();
            var managedList = input.AsQuery().ToManagedList();

            Assert.That(array.Length, Is.EqualTo(4));
            Assert.That(array[0], Is.EqualTo(0));
            Assert.That(array[1], Is.EqualTo(1));
            Assert.That(array[2], Is.EqualTo(2));
            Assert.That(array[3], Is.EqualTo(3));

            Assert.That(unsafeArray.Length, Is.EqualTo(4));
            Assert.That(unsafeArray[0], Is.EqualTo(0));
            Assert.That(unsafeArray[1], Is.EqualTo(1));
            Assert.That(unsafeArray[2], Is.EqualTo(2));
            Assert.That(unsafeArray[3], Is.EqualTo(3));

            Assert.That(unsafeList.Length, Is.EqualTo(4));
            Assert.That(unsafeList[0], Is.EqualTo(0));
            Assert.That(unsafeList[1], Is.EqualTo(1));
            Assert.That(unsafeList[2], Is.EqualTo(2));
            Assert.That(unsafeList[3], Is.EqualTo(3));

            Assert.That(managedArray.Length, Is.EqualTo(4));
            Assert.That(managedArray[0], Is.EqualTo(0));
            Assert.That(managedArray[1], Is.EqualTo(1));
            Assert.That(managedArray[2], Is.EqualTo(2));
            Assert.That(managedArray[3], Is.EqualTo(3));

            Assert.That(managedList.Count, Is.EqualTo(4));
            Assert.That(managedList[0], Is.EqualTo(0));
            Assert.That(managedList[1], Is.EqualTo(1));
            Assert.That(managedList[2], Is.EqualTo(2));
            Assert.That(managedList[3], Is.EqualTo(3));
        }
    }
}
