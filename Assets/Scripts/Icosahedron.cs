using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Icosahedron
{
    public Icosahedron(int subdivisions = 0)
    {
        Generate();
    }

    private void Generate()
    {
        GenerateVertices();
        GenerateTriangles();
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
    

    public IReadOnlyList<Vector3> Vertices;
    public IReadOnlyList<Triangle> Triangles => _triangles;
    
    private List<Triangle> _triangles;
}
