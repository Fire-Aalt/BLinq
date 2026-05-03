namespace FireAlt.BLinq
{
    public interface IAggregator<TAccumulate, TSource>
        where TAccumulate : unmanaged
        where TSource : unmanaged
    {
        TAccumulate Aggregate(in TAccumulate aggregate, in TSource value);
    }
}