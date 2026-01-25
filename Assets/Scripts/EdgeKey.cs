using System;

public struct EdgeKey : IEquatable<EdgeKey>
{
    public readonly int V1;
    public readonly int V2;

    public EdgeKey(int a, int b)
    {
        if (a < b)
        {
            V1 = a;
            V2 = b;
        }
        else
        {
            V1 = b;
            V2 = a;
        }
    }

    public bool Equals(EdgeKey other)
        => V1 == other.V1 && V2 == other.V2;
    public override bool Equals(object obj)
        => obj is EdgeKey other && Equals(other);
    public override int GetHashCode()
        => HashCode.Combine(V1, V2);

    public static bool operator ==(EdgeKey a, EdgeKey b)
        => a.Equals(b);
    public static bool operator !=(EdgeKey a, EdgeKey b)
        => !a.Equals(b);
}