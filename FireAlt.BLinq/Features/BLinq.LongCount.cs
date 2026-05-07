using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the number of elements in the query as a 64-bit integer.
        /// </summary>
        /// <returns>The number of elements in the sequence.</returns>
        public long LongCount()
        {
            if (TryGetLength(out var length))
            {
                return length;
            }

            return BLinqUtilities.LongCount<T, TEnumerator>(GetEnumerator());
        }
    }
    
    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the number of elements in the query as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The number of elements in the sequence.</returns>
        public static long LongCount<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.LongCount();
        }

        /// <summary>
        /// Returns the number of elements in the query that match <paramref name="predicate"/> as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        public static long LongCount<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            if (source.TryGetLength(out var length) && length == 0)
            {
                return 0;
            }

            return BLinqUtilities.LongCount<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate);
        }

        /// <summary>
        /// Returns the number of elements in the query that match <paramref name="predicate"/> as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long LongCount<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<long>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static long LongCount<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            var count = 0L;
            while (enumerator.MoveNext())
            {
                count++;
            }

            enumerator.Dispose();
            return count;
        }

        public static long LongCount<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            var count = 0L;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (!predicate.Match(in value))
                {
                    continue;
                }
                count++;
            }

            enumerator.Dispose();
            return count;
        }
    }
}