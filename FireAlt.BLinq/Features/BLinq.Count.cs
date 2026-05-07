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
        /// Returns the number of elements in the query.
        /// </summary>
        /// <returns>The number of elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of elements is larger than <see cref="int.MaxValue"/>.</exception>
        public int Count()
        {
            if (TryGetLength(out var length))
            {
                return length;
            }

            return BLinqUtilities.Count<T, TEnumerator>(GetEnumerator());
        }
    }

    public static partial class BLinqExtensions
    {
        /// <summary>
        /// Returns the number of elements in the query.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The number of elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of elements is larger than <see cref="int.MaxValue"/>.</exception>
        public static int Count<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return source.Count();
        }

        /// <summary>
        /// Returns the number of elements in the query that match <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="int.MaxValue"/>.</exception>
        public static int Count<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            if (source.TryGetLength(out var length) && length == 0)
            {
                return 0;
            }

            return BLinqUtilities.Count<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate);
        }

        /// <summary>
        /// Returns the number of elements in the query that match <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="int.MaxValue"/>.</exception>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Count<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<int>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static int Count<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            var count = 0;
            while (enumerator.MoveNext())
            {
                if (count == int.MaxValue)
                {
                    enumerator.Dispose();
                    throw new OverflowException();
                }

                count++;
            }

            enumerator.Dispose();
            return count;
        }

        public static int Count<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            var count = 0;
            while (enumerator.MoveNext())
            {
                var value = enumerator.Current;
                if (!predicate.Match(in value))
                {
                    continue;
                }

                if (count == int.MaxValue)
                {
                    enumerator.Dispose();
                    throw new OverflowException();
                }

                count++;
            }

            enumerator.Dispose();
            return count;
        }
    }
}
