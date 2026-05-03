namespace FireAlt.BLinq
{
    public interface ISelector<TSource, out TResult>
        where TSource : unmanaged
        where TResult : unmanaged
    {
        TResult Select(in TSource value);
    }
}