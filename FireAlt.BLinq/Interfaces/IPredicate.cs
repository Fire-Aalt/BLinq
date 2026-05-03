using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public interface IPredicate<T>
        where T : unmanaged
    {
        bool Match(in T value);
    }
}