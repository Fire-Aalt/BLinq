using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public partial struct Query<TEnumerator, T>
        where TEnumerator : unmanaged, IEnumerator<T>
        where T : unmanaged
    {
        private bool TryFirst(out T value)
        {
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

        public T First()
        {
            if (TryFirst(out var value))
            {
                return value;
            }

            throw new InvalidOperationException("The BLinq source contains no elements.");
        }

        public T FirstOrDefault()
        {
            return TryFirst(out var value) ? value : default;
        }
    }
    
    public static partial class BLinqExtensions
    {
        private static bool TryFirst<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate, out T value)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
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
        
        public static T First<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            if (source.TryFirst(predicate, out var value))
            {
                return value;
            }

            throw new InvalidOperationException("The BLinq source contains no elements.");
        }

        
        public static T FirstOrDefault<T, TEnumerator, TPredicate>(this Query<TEnumerator, T> source, TPredicate predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
            where TPredicate : unmanaged, IPredicate<T>
        {
            return source.TryFirst(predicate, out var value) ? value : default;
        }
        
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T First<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return Throw<T>();
        }
        
        [NativeDelegateMethod(typeof(IPredicate<>))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T FirstOrDefault<T, TEnumerator>(this Query<TEnumerator, T> source, Func<T, bool> predicate)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return Throw<T>();
        }
    }
}
