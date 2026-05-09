using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public interface IPredicate<T>
        where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool Match(in T value);
    }
}