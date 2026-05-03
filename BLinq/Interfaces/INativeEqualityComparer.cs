namespace FireAlt.BLinq
{
    public interface INativeEqualityComparer<T>
        where T : unmanaged
    {
        bool Equals(in T left, in T right);
    }
}