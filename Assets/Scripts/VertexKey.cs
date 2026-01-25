using System;
using UnityEngine;

public struct VertexKey : IEquatable<VertexKey>
{
    private const float Scale = 10000.0F;
    private const float InvScale = 1 / Scale;
    
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public VertexKey(float x, float y, float z)
    {
        X = Mathf.RoundToInt(x * Scale);
        Y = Mathf.RoundToInt(y * Scale);
        Z = Mathf.RoundToInt(z * Scale);
    }

    public VertexKey(Vector3 vertex)
    : this(vertex.x, vertex.y, vertex.z)
    {
    }

    public bool Equals(VertexKey other)
        => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object obj) 
        => obj is VertexKey other && Equals(other);
    public override int GetHashCode()
        => HashCode.Combine(X, Y, Z);

    public static bool operator ==(VertexKey lhs, VertexKey rhs)
        => lhs.Equals(rhs);
    public static bool operator !=(VertexKey lhs, VertexKey rhs)
        => !lhs.Equals(rhs);

    public Vector3 ToVector3() => new(X * InvScale, Y * InvScale, Z * InvScale);
}