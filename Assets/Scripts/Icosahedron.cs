using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Icosahedron
{
    public Icosahedron(float radius, int subdivisions = 0)
    {
        Radius = radius;
        Generate(subdivisions);
    }

    private void Generate(int subdivisions)
    {
        GenerateVertices();
        GenerateTriangles();
        Subdivide(subdivisions);
    }

    private void GenerateVertices()
    {
        var theta = (1 + Mathf.Sqrt(5)) * 0.5F;
        Vertices = new List<Vector3>()
        {
            new(0, +1, +theta),
            new(0, +1, -theta),
            new(0, -1, +theta),
            new(0, -1, -theta),
            new(+1, +theta, 0),
            new(+1, -theta, 0),
            new(-1, +theta, 0),
            new(-1, -theta, 0),
            new(+theta, 0, +1),
            new(+theta, 0, -1),
            new(-theta, 0, +1),
            new(-theta, 0, -1),
        };
    }

    private void GenerateTriangles()
    {
        _triangles = new List<Triangle>();
        // Get the minimum length (edge length).
        var minLength = Mathf.Infinity;
        for (var i = 1; i < Vertices.Count; ++i)
        {
            var length = (Vertices[0] - Vertices[i]).magnitude;
            if (length < minLength)
            {
                minLength = length;
            }
        }

        foreach (var vertex1 in Vertices) 
        {
            var neighbors = new HashSet<Vector3>(Vertices.Where(v =>
                v != vertex1 &&
                (v - vertex1).magnitude <= minLength + 0.05F
              ));
            var chainHead = neighbors.First();
            var chain = new List<Vector3> { chainHead };
            neighbors.Remove(chainHead);
            while (neighbors.Count > 0)
            {
                var vertex2 = chain[^1];
                // The problem is since we dequeue vertices we can remove part of chain...
                var vertex3 = neighbors.OrderBy(v => (v - vertex2).sqrMagnitude).First();
                chain.Add(vertex3);
                neighbors.Remove(vertex3);
            }
            chain.Add(chainHead);
            
            for (int i = 0; i < chain.Count - 1; ++i)
            {
                var vertex2 = chain[i];
                var vertex3 = chain[i + 1];
                
                var newTriangle = new Triangle(vertex1, vertex2, vertex3);
                if (!_triangles.Any(t => t.Equals(newTriangle)))
                {
                    _triangles.Add(newTriangle);   
                }
            }
        }
    }

    private void Subdivide(int subdivisions)
    {
        Triangles2 = new List<Triangle>();
        foreach (var triangle in _triangles)
        {
            var a = triangle.Points[1] - triangle.Points[0];
            var b = triangle.Points[2] - triangle.Points[0];

            for (int i = 0; i <= subdivisions; ++i)
            for (int j = 0; j <= (subdivisions - i); ++j)
            {
                var tx = i / (subdivisions + 1.0F);
                var ty = j / (subdivisions + 1.0F);
                var txp1 = (i + 1) / (subdivisions + 1.0F);
                var typ1 = (j + 1) / (subdivisions + 1.0F);
                
                var p1 = (tx * a + ty * b + triangle.Points[0]).normalized * Radius;
                var p2 = (tx * a + typ1 * b + triangle.Points[0]).normalized * Radius;
                var p3 = (txp1 * a + ty * b + triangle.Points[0]).normalized * Radius;
                var p4 = (txp1 * a + typ1 * b + triangle.Points[0]).normalized * Radius;

                
                var toMiddle1 = -(p1 + p2 + p3) / 3.0F;
                var crossProduct1 = Vector3.Cross(p2 - p1, p3 - p1).normalized;
                var dot1 = Vector3.Dot(crossProduct1, toMiddle1);
                var shouldInvert1 = dot1 < 0;
                if (shouldInvert1)
                {
                    Triangles2.Add(new Triangle(p3, p2, p1));
                }
                else
                {
                    Triangles2.Add(new Triangle(p1, p2, p3));
                }

                if (j < subdivisions - i)
                {
                    var toMiddle2 = -(p2 + p3 + p4) / 3.0F;
                    var crossProduct2 = Vector3.Cross(p3 - p2, p4 - p2).normalized;
                    var dot2 = Vector3.Dot(crossProduct2, toMiddle2);
                    var shouldInvert2 = dot2 < 0;
                    if (shouldInvert2)
                    {
                        Triangles2.Add(new Triangle(p4, p3, p2));
                    }
                    else
                    {
                        Triangles2.Add(new Triangle(p2, p3, p4));
                    }
                }
            }
        }
    }

    public IReadOnlyList<Vector3> Vertices;
    public IReadOnlyList<Triangle> Triangles => _triangles;
    
    private List<Triangle> _triangles;
    public List<Triangle> Triangles2;
    
    public float Radius { get; private set; }
}
