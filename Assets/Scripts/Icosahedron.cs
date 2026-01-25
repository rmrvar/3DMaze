using System.Collections.Generic;
using UnityEngine;

public class Icosahedron
{
    public MeshData MeshData { get; }
    public float Radius { get; }
    public int SubdivisionCount { get; }
    
    public Icosahedron(float radius, int subdivisionCount)
    {
        Radius = radius;
        SubdivisionCount = subdivisionCount;
        MeshData = new MeshData
        {
            Vertices = new List<Vector3>(12),
            Indices = new List<int>(60)
        };
    }

    public void GenerateMesh()
    {
        // The vertical offset for the two rings of 5 vertices
        // This value ensures the triangles remain equilateral
        float lat = Mathf.Atan(0.5f); 
        
        // 1. Top Pole
        _initialVertices.Add(new Vector3(0, Radius, 0)); // Index 0
        
        
        // 2. Upper Ring (5 vertices)
        for (int i = 0; i < 5; i++)
        {
            float angle = i * 72 * Mathf.Deg2Rad;
            float x = Mathf.Cos(lat) * Mathf.Sin(angle);
            float y = Mathf.Sin(lat);
            float z = Mathf.Cos(lat) * Mathf.Cos(angle);
            _initialVertices.Add(new Vector3(x, y, z) * Radius);
        }

        // 3. Lower Ring (5 vertices, offset by 36 degrees)
        for (int i = 0; i < 5; i++)
        {
            float angle = (i * 72 + 36) * Mathf.Deg2Rad;
            float x = Mathf.Cos(lat) * Mathf.Sin(angle);
            float y = -Mathf.Sin(lat);
            float z = Mathf.Cos(lat) * Mathf.Cos(angle);
            _initialVertices.Add(new Vector3(x, y, z) * Radius);
        }

        // 4. Bottom Pole
        _initialVertices.Add(new Vector3(0, -Radius, 0)); // Index 11

        // --- TRIANGLES ---

        // Top Cap (Connects Index 0 to Upper Ring 1-5)
        for (int i = 1; i <= 5; i++)
        {
            int next = (i % 5) + 1;
            _initialIndices.Add(0);
            _initialIndices.Add(next);
            _initialIndices.Add(i);
        }

        // Upper Mid-Belt
        for (int i = 1; i <= 5; i++)
        {
            int nextUpper = (i % 5) + 1;
            int lower = i + 5;
            _initialIndices.Add(i);
            _initialIndices.Add(nextUpper);
            _initialIndices.Add(lower);
        }

        // Lower Mid-Belt
        for (int i = 1; i <= 5; i++)
        {
            int nextUpper = (i % 5) + 1;
            int lower = i + 5;
            int nextLower = (i == 5) ? 6 : lower + 1;
            _initialIndices.Add(lower);
            _initialIndices.Add(nextUpper);
            _initialIndices.Add(nextLower);
        }

        // Bottom Cap (Connects Index 11 to Lower Ring 6-10)
        for (int i = 6; i <= 10; i++)
        {
            int next = (i == 10) ? 6 : i + 1;
            _initialIndices.Add(11);
            _initialIndices.Add(i);
            _initialIndices.Add(next);
        }

        Subdivide();
        // foreach (var vertex in _initialVertices)
        // {
        //     MeshData.AddVertex(vertex);
        // }
        // for (int i = 0; i < _initialIndices.Count; i += 3)
        // {
        //     MeshData.AddTriangle(_initialIndices[i], _initialIndices[i + 1], _initialIndices[i + 2]);
        // }
    }

    private void Subdivide()
    {
        for (int currInitialIndex = 0; currInitialIndex < _initialIndices.Count; currInitialIndex += 3)
        {
            var initialPoint1 = _initialVertices[_initialIndices[currInitialIndex]];
            var initialPoint2 = _initialVertices[_initialIndices[currInitialIndex + 1]];
            var initialPoint3 = _initialVertices[_initialIndices[currInitialIndex + 2]];
            var a = initialPoint2 - initialPoint1;
            var b = initialPoint3 - initialPoint1;

            for (int i = 0; i <= SubdivisionCount; ++i)
            for (int j = 0; j <= (SubdivisionCount - i); ++j)
            {
                var tx = i / (SubdivisionCount + 1.0F);
                var ty = j / (SubdivisionCount + 1.0F);
                var txp1 = (i + 1) / (SubdivisionCount + 1.0F);
                var typ1 = (j + 1) / (SubdivisionCount + 1.0F);
                
                var p1 = (tx * a + ty * b + initialPoint1).normalized * Radius;
                var p2 = (tx * a + typ1 * b + initialPoint1).normalized * Radius;
                var p3 = (txp1 * a + ty * b + initialPoint1).normalized * Radius;
                var p4 = (txp1 * a + typ1 * b + initialPoint1).normalized * Radius;

                var i1 = MeshData.AddVertex(p1);
                var i2 = MeshData.AddVertex(p2);
                var i3 = MeshData.AddVertex(p3);
                
                MeshData.AddTriangle(i1, i2, i3);
                
                if (j < SubdivisionCount - i)
                {
                    var i4 = MeshData.AddVertex(p4);
                    
                    MeshData.AddTriangle(i2, i3, i4);
                }
            }
        }
    }

    private readonly List<Vector3> _initialVertices = new(12);
    private readonly List<int> _initialIndices = new(60);
}
