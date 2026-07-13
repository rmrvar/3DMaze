using System;

namespace Maze
{
    public struct CellKey : IEquatable<CellKey>
    {
        public readonly int V1;
        public readonly int V2;
        public readonly int V3;

        public CellKey(int a, int b, int c)
        {
            if (a > b) (a, b) = (b, a);
            if (a > c) (a, c) = (c, a);
            if (b > c) (b, c) = (c, b);

            V1 = a;
            V2 = b;
            V3 = c;
        }

        public bool Equals(CellKey other)
            => V1 == other.V1 && V2 == other.V2 && V3 == other.V3;

        public override bool Equals(object obj)
            => obj is CellKey other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(V1, V2, V3);

        public static bool operator ==(CellKey a, CellKey b)
            => a.Equals(b);

        public static bool operator !=(CellKey a, CellKey b)
            => !a.Equals(b);
    }
}
