using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the sum of the query using the default accumulator.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The sum of all elements.</returns>
        public static T Sum<T, TEnumerator, TAccumulator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TAccumulator : unmanaged, IAccumulator<T>
        {
            return BLinqUtilities.Sum<T, TEnumerator, TAccumulator>(source.GetEnumerator(), default);
        }

        /// <summary>
        /// Returns the sum of the projected values using the default accumulator.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">Selector used to project each element before summing.</param>
        /// <returns>The sum of the projected values.</returns>
        public static TResult Sum<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
            this Query<TEnumerator, TSource> source, TSelector selector)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<TSource>
            where TSelector : unmanaged, ISelector<TSource, TResult>
            where TAccumulator : unmanaged, IAccumulator<TResult>
        {
            return BLinqUtilities.Sum<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
                source.GetEnumerator(),
                selector,
                default);
        }

        /// <summary>
        /// Returns the average of the query using the default accumulator.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The average of all elements.</returns>
        public static T Average<T, TEnumerator, TAccumulator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TAccumulator : unmanaged, IAccumulator<T>
        {
            return BLinqUtilities.Average<T, TEnumerator, TAccumulator>(source.GetEnumerator(), default);
        }

        /// <summary>
        /// Returns the average of the projected values using the default accumulator.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">Selector used to project each element before averaging.</param>
        /// <returns>The average of the projected values.</returns>
        public static TResult Average<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
            this Query<TEnumerator, TSource> source, TSelector selector)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<TSource>
            where TSelector : unmanaged, ISelector<TSource, TResult>
            where TAccumulator : unmanaged, IAccumulator<TResult>
        {
            return BLinqUtilities.Average<TSource, TResult, TEnumerator, TSelector, TAccumulator>(
                source.GetEnumerator(),
                selector,
                default);
        }
        
        /// <summary>
        /// Returns the sum of the projected values using a managed selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">Selector used to project each element before summing.</param>
        /// <param name="_">Unused default parameter used to guide overload resolution.</param>
        /// <returns>The sum of the projected values.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult Sum<TSource, TResult, TEnumerator>(
            this Query<TEnumerator, TSource> source, Func<TSource, TResult> selector, TResult _ = default)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<TSource>
        {
            return ThrowCodeGen<TResult>();
        }

        /// <summary>
        /// Returns the average of the projected values using a managed selector.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="selector">Selector used to project each element before averaging.</param>
        /// <param name="_">Unused default parameter used to guide overload resolution.</param>
        /// <returns>The average of the projected values.</returns>
        [NativeDelegateMethod(typeof(ISelector<,>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TResult Average<TSource, TResult, TEnumerator>(
            this Query<TEnumerator, TSource> source, Func<TSource, TResult> selector, TResult _ = default)
            where TSource : unmanaged
            where TResult : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<TSource>
        {
            return ThrowCodeGen<TResult>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static T Sum<T, TEnumerator, TAccumulator>(TEnumerator enumerator, TAccumulator accumulator)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
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
            where TEnumerator : unmanaged, IQueryEnumerator<TSource>
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
            where TEnumerator : unmanaged, IQueryEnumerator<T>
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
            where TEnumerator : unmanaged, IQueryEnumerator<TSource>
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
