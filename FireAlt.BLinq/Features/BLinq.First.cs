using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IQueryEnumerator<T>
        where T : unmanaged
    {
        private bool TryFirst(out T value)
        {
            if (TryGetLength(out var length) && length == 0)
            {
                value = default;
                return false;
            }

            if (TryGetElementAt(0, out value))
            {
                return true;
            }

            var enumerator = GetEnumerator();
            if (enumerator.MoveNext())
            {
                value = enumerator.Current;
                enumerator.Dispose();
                return true;
            }

            value = default;
            enumerator.Dispose();
            return false;
        }

        /// <summary>
        /// Returns the first element of the query, or throws if the query is empty.
        /// </summary>
        /// <returns>The first element in the sequence.</returns>
        public T First()
        {
            if (TryFirst(out var value))
            {
                return value;
            }

            throw new InvalidOperationException("The BLinq source contains no elements.");
        }

        /// <summary>
        /// Returns the first element of the query, or the default value if the query is empty.
        /// </summary>
        /// <returns>The first element in the sequence, or default when the sequence is empty.</returns>
        public T FirstOrDefault()
        {
            return TryFirst(out var value) ? value : default;
        }
    }
    
    public static partial class BLinqExtensions
    {
        private static bool TryFirst<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate, out T value)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            if (source.TryGetLength(out var length) && length == 0)
            {
                value = default;
                return false;
            }

            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                value = enumerator.Current;
                if (predicate.Match(in value))
                {
                    enumerator.Dispose();
                    return true;
                }
            }

            value = default;
            enumerator.Dispose();
            return false;
        }
        
        /// <summary>
        /// Returns the first element that matches <paramref name="predicate"/>, or throws if no element matches.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The first matching element in the sequence.</returns>
        public static T First<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            if (source.TryFirst(predicate, out var value))
            {
                return value;
            }

            throw new InvalidOperationException("The BLinq source contains no elements.");
        }
        
        /// <summary>
        /// Returns the first element that matches <paramref name="predicate"/>, or the default value if none match.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The first matching element, or default when no matching element exists.</returns>
        public static T FirstOrDefault<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return source.TryFirst(predicate, out var value) ? value : default;
        }
        
        /// <summary>
        /// Returns the first element that matches <paramref name="predicate"/>, or throws if no element matches.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The first matching element in the sequence.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T First<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }
        
        /// <summary>
        /// Returns the first element that matches <paramref name="predicate"/>, or the default value if none match.
        /// </summary>
        /// <param name="source">Source query.</param>
        /// <param name="predicate">The predicate used to find a matching element.</param>
        /// <returns>The first matching element, or default when no matching element exists.</returns>
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T FirstOrDefault<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IQueryEnumerator<T>
        {
            return ThrowCodeGen<T>();
        }
    }
}
