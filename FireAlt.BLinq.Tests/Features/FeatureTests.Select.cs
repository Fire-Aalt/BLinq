using System.Linq;
using NUnit.Framework;
using Unity.Collections;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    public partial class FeatureTests
    {
        [Test]
        public void Select_ProjectsValues()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);
            var expected = input.Select(value => value * 2).ToArray();
            var mapped = input
                .AsQuery()
                .Select(value => value * 2)
                .ToNativeList(Allocator.Temp);

            AssertSequence(mapped.AsArray(), expected);
        }
    }
}
