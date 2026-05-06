using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        /// <summary>
        /// Returns the number of elements in the query.
        /// </summary>
        /// <returns>The number of elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of elements is larger than <see cref="int.MaxValue"/>.</exception>
        public int Count()
        {
            return BLinqUtilities.Count<T, TEnumerator>(GetEnumerator());
        }

        /// <summary>
        /// Returns the number of elements in the query as a 64-bit integer.
        /// </summary>
        /// <returns>The number of elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of elements is larger than <see cref="long.MaxValue"/>.</exception>
        public long LongCount()
        {
            return BLinqUtilities.LongCount<T, TEnumerator>(GetEnumerator());
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
            where TEnumerator : unmanaged, IEnumerator<T>
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
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return BLinqUtilities.Count<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate);
        }

        /// <summary>
        /// Returns the number of elements in the query as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <returns>The number of elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of elements is larger than <see cref="long.MaxValue"/>.</exception>
        public static long LongCount<T, TEnumerator>(this Query<TEnumerator, T> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return source.LongCount();
        }

        /// <summary>
        /// Returns the number of elements in the query that match <paramref name="predicate"/> as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="long.MaxValue"/>.</exception>
        public static long LongCount<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return BLinqUtilities.LongCount<T, TEnumerator, TPredicate>(source.GetEnumerator(), predicate);
        }

        /// <summary>
        /// Returns the number of elements in the ordered query.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <returns>The number of elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of elements is larger than <see cref="int.MaxValue"/>.</exception>
        public static int Count<T, TEnumerator, TComparer>(this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return BLinqUtilities.Count<T, OrderedQueryEnumerator<T, TEnumerator, TComparer>>(source.GetEnumerator());
        }

        /// <summary>
        /// Returns the number of elements in the ordered query that match <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="int.MaxValue"/>.</exception>
        public static int Count<T, TEnumerator, TComparer, TPredicate>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return BLinqUtilities.Count<T, OrderedQueryEnumerator<T, TEnumerator, TComparer>, TPredicate>(
                source.GetEnumerator(),
                predicate);
        }

        /// <summary>
        /// Returns the number of elements in the ordered query as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <returns>The number of elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of elements is larger than <see cref="long.MaxValue"/>.</exception>
        public static long LongCount<T, TEnumerator, TComparer>(this OrderedQuery<TEnumerator, T, TComparer> source)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return BLinqUtilities.LongCount<T, OrderedQueryEnumerator<T, TEnumerator, TComparer>>(source.GetEnumerator());
        }

        /// <summary>
        /// Returns the number of elements in the ordered query that match <paramref name="predicate"/> as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="long.MaxValue"/>.</exception>
        public static long LongCount<T, TEnumerator, TComparer, TPredicate>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return BLinqUtilities.LongCount<T, OrderedQueryEnumerator<T, TEnumerator, TComparer>, TPredicate>(
                source.GetEnumerator(),
                predicate);
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
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<int>();
        }

        /// <summary>
        /// Returns the number of elements in the query that match <paramref name="predicate"/> as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="long.MaxValue"/>.</exception>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long LongCount<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return ThrowCodeGen<long>();
        }

        /// <summary>
        /// Returns the number of elements in the ordered query that match <paramref name="predicate"/>.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="int.MaxValue"/>.</exception>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int Count<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<int>();
        }

        /// <summary>
        /// Returns the number of elements in the ordered query that match <paramref name="predicate"/> as a 64-bit integer.
        /// </summary>
        /// <param name="source">Source ordered query.</param>
        /// <param name="predicate">The predicate used to test each element.</param>
        /// <returns>The number of matching elements in the sequence.</returns>
        /// <exception cref="OverflowException">The number of matching elements is larger than <see cref="long.MaxValue"/>.</exception>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static long LongCount<T, TEnumerator, TComparer>(
            this OrderedQuery<TEnumerator, T, TComparer> source,
            Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TComparer : unmanaged, IComparer<T>
        {
            return ThrowCodeGen<long>();
        }
    }

    internal static partial class BLinqUtilities
    {
        public static int Count<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
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
            where TEnumerator : unmanaged, IEnumerator<T>
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

        public static long LongCount<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            var count = 0L;
            while (enumerator.MoveNext())
            {
                if (count == long.MaxValue)
                {
                    enumerator.Dispose();
                    throw new OverflowException();
                }

                count++;
            }

            enumerator.Dispose();
            return count;
        }

        public static long LongCount<T, TEnumerator, TPredicate>(TEnumerator enumerator, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
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

                if (count == long.MaxValue)
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
