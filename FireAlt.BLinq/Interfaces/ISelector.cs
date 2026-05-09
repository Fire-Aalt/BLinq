using System.Runtime.CompilerServices;

namespace FireAlt.BLinq
{
    public interface ISelector<TSource, out TResult>
        where TSource : unmanaged
        where TResult : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        TResult Select(in TSource value);
    }
}