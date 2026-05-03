namespace FireAlt.BLinq
{
    public interface IAccumulator<T>
        where T : unmanaged
    {
        T Add(in T total, in T value);

        T Divide(in T total, uint count);
    }
}