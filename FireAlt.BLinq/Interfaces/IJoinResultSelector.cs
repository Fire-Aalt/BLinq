namespace FireAlt.BLinq
{
    public interface IJoinResultSelector<TOuter, TInner, out TResult>
        where TOuter : unmanaged
        where TInner : unmanaged
        where TResult : unmanaged
    {
        TResult Select(in TOuter outer, in TInner inner);
    }
}
