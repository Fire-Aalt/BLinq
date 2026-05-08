namespace FireAlt.BLinq
{
    public interface IZipResultSelector<TFirst, TSecond, out TResult>
        where TFirst : unmanaged
        where TSecond : unmanaged
        where TResult : unmanaged
    {
        TResult Select(in TFirst first, in TSecond second);
    }
}
