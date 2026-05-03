using System;
using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public struct NativeEqualityComparer<T> : INativeEqualityComparer<T>
        where T : unmanaged, IEquatable<T>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(in T left, in T right)
        {
            return left.Equals(right);
        }
    }
}