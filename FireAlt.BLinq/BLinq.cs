using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public static class BLinq
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Query<TEnumerator, T> From<T, TEnumerator>(TEnumerator enumerator)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return new Query<TEnumerator, T>(enumerator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Query<TEnumerator, T> From<T, TEnumerator>(TEnumerator enumerator, int length)
            where T : unmanaged
            where TEnumerator : unmanaged, IEnumerator<T>
        {
            return new Query<TEnumerator, T>(enumerator, length);
        }
    }
    
    public static partial class BLinqExtensions
    {
        internal static void ThrowCodeGen()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
        
        internal static T ThrowCodeGen<T>()
        {
            throw new InvalidOperationException("BLinq delegate query was not IL-woven.");
        }
    }
}
