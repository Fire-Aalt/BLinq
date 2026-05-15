using System;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace FireAlt.BLinq.Tests
{
    [BurstCompile]
    public class BurstedDelegatesTests
    {
        [Test]
        public void DelegatePipeline_WhereSelectSum_UsesUnmanagedCapturedValues()
        {
            var input = new NativeArray<int>(new[] { 0, 1, 2, 3 }, Allocator.Temp);

            var result = Execute_WhereSelectSum_UsesUnmanagedCapturedValues(input);

            Assert.That(result, Is.EqualTo(19));
        }

        [BurstCompile(CompileSynchronously = true)]
        private static float Execute_WhereSelectSum_UsesUnmanagedCapturedValues(in NativeArray<int> input)
        {
            var min = 1;
            var factor = 3;
            var offset = 2;

            var result = input
                .AsQuery()
                .Where(value => value > min)
                .Select(value => (float)(value * factor))
                .Sum(value => value + offset);

            return result;
        }

        [Test]
        public void DelegatePipeline_PreservesDistinctSameShapeAdapters()
        {
            var input = new NativeArray<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7 }, Allocator.Temp);
            var output = new NativeArray<int>(3, Allocator.Temp);

            BurstDelegatePipeline_PreservesDistinctSameShapeAdapters(input, ref output);

            Assert.That(output[0], Is.EqualTo(13));
            Assert.That(output[1], Is.EqualTo(0));
            Assert.That(output[2], Is.EqualTo(2));
        }

        [BurstCompile(CompileSynchronously = true)]
        private static void BurstDelegatePipeline_PreservesDistinctSameShapeAdapters(
            in NativeArray<int> input,
            ref NativeArray<int> output)
        {
            var left = input.AsQuery().Where(Left);
            var right = input.AsQuery().Where(Right);
            var rightKeys = right.Select(Key);

            output[0] = left.Union(right).Sum(Identity);
            output[1] = left.IntersectBy(rightKeys, Key).Sum(Identity);
            output[2] = left.ExceptBy(rightKeys, Key).Sum(Identity);
        }

        private static bool Left(int value)
        {
            return (value & 1) == 0;
        }

        private static bool Right(int value)
        {
            return value == 1;
        }

        private static int Key(int value)
        {
            return value & 3;
        }

        private static int Identity(int value)
        {
            return value;
        }

        [Test]
        public void DelegatePipeline_TupleInputAndReturn_RewritesInBurst()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            var result = BurstDelegatePipeline_TupleInputAndReturn(input);

            Assert.That(result, Is.EqualTo(51));
        }

        [BurstCompile(CompileSynchronously = true)]
        private static int BurstDelegatePipeline_TupleInputAndReturn(in NativeArray<int> input)
        {
            var offset = 5;

            return input
                .AsQuery()
                .Index()
                .Select(tuple => (Left: tuple.Index, Right: tuple.Item + offset))
                .Sum(tuple => tuple.Left * 10 + tuple.Right);
        }

        [Test]
        public void DelegatePipeline_TupleLocals_RewriteInBurst()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3 }, Allocator.Temp);

            var result = BurstDelegatePipeline_TupleLocals(input);

            Assert.That(result, Is.EqualTo(12));
        }

        [BurstCompile(CompileSynchronously = true)]
        private static int BurstDelegatePipeline_TupleLocals(in NativeArray<int> input)
        {
            var offset = 4;

            return input
                .AsQuery()
                .Select(value =>
                {
                    var pair = (Doubled: value * 2, Shifted: value + offset);
                    return (Delta: pair.Doubled - pair.Shifted, Total: pair.Doubled + pair.Shifted);
                })
                .Sum(tuple => tuple.Delta * 3 + tuple.Total);
        }

        [Test]
        public void DelegatePipeline_NestedTupleInputReturnAndCapture_RewriteInBurst()
        {
            var input = new NativeArray<int>(new[] { 2, 3, 4, 5 }, Allocator.Temp);

            var result = BurstDelegatePipeline_NestedTupleInputReturnAndCapture(input);

            Assert.That(result, Is.EqualTo(2090));
        }

        [BurstCompile(CompileSynchronously = true)]
        private static int BurstDelegatePipeline_NestedTupleInputReturnAndCapture(in NativeArray<int> input)
        {
            var capture = (Threshold: 1, Bonus: 3);

            return input
                .AsQuery()
                .Index()
                .Select(tuple => (
                    Key: (Left: tuple.Index + capture.Threshold, Right: tuple.Item - capture.Threshold),
                    Score: tuple.Index * tuple.Item + capture.Bonus))
                .Where(tuple => ((tuple.Key.Left + tuple.Key.Right + tuple.Score) & 1) == 0)
                .Aggregate(
                    (Count: 0, Total: 0),
                    (accumulator, tuple) => (
                        Count: accumulator.Count + 1,
                        Total: accumulator.Total + tuple.Key.Left * 10 + tuple.Key.Right + tuple.Score),
                    accumulator => accumulator.Count * 1000 + accumulator.Total);
        }
        
        [Test]
        public void DelegateAggregateBy_UsesMultipleNonAdjacentDelegates()
        {
            var input = new NativeArray<int>(new[] { 1, 2, 3, 4 }, Allocator.Temp);
            var output = new NativeArray<int>(5, Allocator.Temp);

            BurstDelegateAggregateBy(input, ref output);
            
            Assert.That(output[0], Is.EqualTo(2));
            Assert.That(output[1], Is.EqualTo(1));
            Assert.That(output[2], Is.EqualTo(16));
            Assert.That(output[3], Is.EqualTo(0));
            Assert.That(output[4], Is.EqualTo(18));
        }
        
        [BurstCompile(CompileSynchronously = true)]
        private static void BurstDelegateAggregateBy(in NativeArray<int> input, ref NativeArray<int> output)
        {
            var keyMask = 1;
            var addend = 1;

            var aggregates = input
                .AsQuery()
                .AggregateBy(
                    value => (byte)(value & keyMask),
                    10,
                    (aggregate, value) => aggregate + value + addend)
                .ToNativeList(Allocator.Temp);

            output[0] = aggregates.Length;
            output[1] = aggregates[0].Key;
            output[2] = aggregates[0].Value;
            output[3] = aggregates[1].Key;
            output[4] = aggregates[1].Value;

            aggregates.Dispose();
        }
    }
}
