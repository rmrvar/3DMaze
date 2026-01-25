using System;

public struct FaceKey : IEquatable<FaceKey>
{
    public readonly int V1;
    public readonly int V2;
    public readonly int V3;

    public FaceKey(int a, int b, int c)
    {
        if (a > b) (a, b) = (b, a);
        if (a > c) (a, c) = (c, a);
        if (b > c) (b, c) = (c, b);

        V1 = a; V2 = b; V3 = c;
    }
    
    public bool Equals(FaceKey other) 
        => V1 == other.V1 && V2 == other.V2 && V3 == other.V3;
    public override bool Equals(object obj) 
        => obj is FaceKey other && Equals(other);
    public override int GetHashCode() 
        => HashCode.Combine(V1, V2, V3);

    public static bool operator ==(FaceKey a, FaceKey b) 
        => a.Equals(b);
    public static bool operator !=(FaceKey a, FaceKey b) 
        => !a.Equals(b);
}