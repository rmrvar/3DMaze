using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangle
{
    public Triangle(Vector3 a, Vector3 b, Vector3 c)
    {
        Points = new List<Vector3> { a, b, c };
    }
    
    public IReadOnlyList<Vector3> Points;

    public bool HasPoint(Vector3 point)
    {
        return Points.Any(p => p == point);
    }
    
    public override bool Equals(object other)
    {
        if (other is not Triangle otherTriangle)
        {
            return false;
        }

        return Points.All(p => otherTriangle.HasPoint(p));
    }
}
