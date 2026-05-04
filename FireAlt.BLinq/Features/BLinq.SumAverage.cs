using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        public static T Sum<T, TEnumerator, TAccumulator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TAccumulator : unmanaged, IAccumulator<T>
        {
            return BLinqUtilities.Sum<T, TEnumerator, TAccumulator>(source.GetEnumerator(), default);
        }

        public static TResult Sum<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
            this Query<TEnumerator, TSource> source, TSelector selector)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TSelector : unmanaged, ISelector<TSource, TResult>
            where TAccumulator : unmanaged, IAccumulator<TResult>
        {
            return BLinqUtilities.Sum<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
                source.GetEnumerator(),
                selector,
                default);
        }

        public static T Average<T, TEnumerator, TAccumulator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TAccumulator : unmanaged, IAccumulator<T>
        {
            return BLinqUtilities.Average<T, TEnumerator, TAccumulator>(source.GetEnumerator(), default);
        }

        public static TResult Average<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
            this Query<TEnumerator, TSource> source, TSelector selector)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TSelector : unmanaged, ISelector<TSource, TResult>
            where TAccumulator : unmanaged, IAccumulator<TResult>
        {
            return BLinqUtilities.Average<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
                source.GetEnumerator(),
                selector,
                default);
        }
        
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult Sum<TSource, TResult, TEnumerator>(
            this Query<TEnumerator, TSource> source, Func<TSource, TResult> selector, TResult _ = default)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
        {
            return ThrowCodeGen<TResult>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static T Sum<T, TEnumerator, TAccumulator>(TEnumerator enumerator, TAccumulator accumulator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TAccumulator : unmanaged, IAccumulator<T>
        {
            var total = default(T);
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                total = accumulator.Add(in total, in value);
            }

            enumerator.Dispose();
            return total;
        }

        public static TResult Sum<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
            TEnumerator enumerator, TSelector selector, TAccumulator accumulator)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TSelector : unmanaged, ISelector<TSource, TResult>
            where TAccumulator : unmanaged, IAccumulator<TResult>
        {
            var total = default(TResult);
            while (enumerator.MoveNext())
            {
                var sourceValue = enumerator.Current;
                var value = selector.Select(in sourceValue);
                total = accumulator.Add(in total, in value);
            }

            enumerator.Dispose();
            return total;
        }

        public static T Average<T, TEnumerator, TAccumulator>(TEnumerator enumerator, TAccumulator accumulator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TAccumulator : unmanaged, IAccumulator<T>
        {
            var total = default(T);
            var count = 0u;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                total = accumulator.Add(in total, in value);
                count++;
            }

            enumerator.Dispose();
            return count == 0u ? default : accumulator.Divide(in total, count);
        }

        public static TResult Average<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
            TEnumerator enumerator, TSelector selector, TAccumulator accumulator)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IEnumerator<TSource>
            where TSelector : unmanaged, ISelector<TSource, TResult>
            where TAccumulator : unmanaged, IAccumulator<TResult>
        {
            var total = default(TResult);
            var count = 0u;
            while (enumerator.MoveNext())
            {
                var sourceValue = enumerator.Current;
                var value = selector.Select(in sourceValue);
                total = accumulator.Add(in total, in value);
                count++;
            }

            enumerator.Dispose();
            return count == 0u ? default : accumulator.Divide(in total, count);
        }
    }
}
