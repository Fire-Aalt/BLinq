namespace FireAlt.BLinq
{
    public interface IGroupJoinResultSelector<TOuter, TGroup, out TResult>
        where TOuter : unmanaged
        where TGroup : unmanaged
        where TResult : unmanaged
    {
        TResult Select(in TOuter outer, in TGroup group);
    }
}
