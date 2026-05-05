using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void First_ReturnsFirstElement()
        {
            var input = new NativeArray<int>(new[] { 9, 10 }, Allocator.Temp);
            var expected = input.First();

            Assert.That(input.AsQuery().First(), Is.EqualTo(expected));
        }

        [Test]
        public void FirstOrDefault_ReturnsDefaultForEmptySource()
        {
            var input = new NativeArray<int>(0, Allocator.Temp);
            var expected = input.FirstOrDefault();

            Assert.That(input.AsQuery().FirstOrDefault(), Is.EqualTo(expected));
        }

        [Test]
        public void FirstOrDefault_WithPredicate_ReturnsFirstMatch()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expectedMatch = input.FirstOrDefault(value => value > 1);
            var expectedNoMatch = input.FirstOrDefault(value => value > 10);

            Assert.That(input.AsQuery().FirstOrDefault(value => value > 1), Is.EqualTo(expectedMatch));
            Assert.That(input.AsQuery().FirstOrDefault(value => value > 10), Is.EqualTo(expectedNoMatch));
        }
    }
}
