using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeTile
{
    public const int CYLINDER_DETAIL_AMOUNT = 2;
    public static int NumVerts => 3 * (4 + 2 * (2 + 2 * CYLINDER_DETAIL_AMOUNT));
    
    public void CreateMesh(Triangle triangle, int startIndex, float wallThickness = 0.1F, float wallHeight = 2.0F)
    {
        CreateWall(triangle.Points[0], triangle.Points[1], triangle.Points[2], wallThickness, wallHeight, 0, out var vertices1, out var indices1);
        CreateWall(triangle.Points[1], triangle.Points[2], triangle.Points[0], wallThickness, wallHeight, vertices1.Count, out var vertices2, out var indices2);
        CreateWall(triangle.Points[2], triangle.Points[0], triangle.Points[1], wallThickness, wallHeight, vertices1.Count + vertices2.Count, out var vertices3, out var indices3);
        Points = vertices1.Concat(vertices2).Concat(vertices3).ToArray();
        Indices = indices1.Concat(indices2).Concat(indices3).ToArray();
        for (int i = 0; i < Indices.Length; ++i)
        {
            Indices[i] += startIndex;
        }
    }

    private void CreateWall(
        Vector3 adjacentVertex1, 
        Vector3 adjacentVertex2, 
        Vector3 oppositeVertex, 
        float thickness, 
        float height, 
        int startIndex,
        out List<Vector3> vertices,
        out List<int> indices
      )
    {
        var ab = adjacentVertex2 - adjacentVertex1;
        var ac = oppositeVertex - adjacentVertex1;
        
        var prll = ab.normalized;
        var perp = (ac - Vector3.Dot(ab.normalized, ac) * ab.normalized).normalized;
        
        var abb = adjacentVertex1 + perp * thickness * 0.50F;
        var bab = adjacentVertex2 + perp * thickness * 0.50F;
        var abt = abb + -adjacentVertex1.normalized * height;        
        var bat = bab + -adjacentVertex2.normalized * height;
        
        vertices = new List<Vector3>();
        indices = new List<int>();

        Stack<int> tops1;
        Queue<int> tops2;
        
        int numSteps = CYLINDER_DETAIL_AMOUNT;
        // CYLINDER1
        vertices.Add(abb);
        vertices.Add(abt);
        for (int i = 0; i < numSteps; ++i)
        {
            // Adds the new face, sharing the last two vertices.
            var t2 = ((float) i + 1) / numSteps * Mathf.PI * 0.5F;
            var offset = (Mathf.Cos(t2) * perp + Mathf.Sin(t2) * -prll) * thickness * 0.5F;
            var p2b = adjacentVertex1 + offset;
            var p2t = adjacentVertex1 + -adjacentVertex1.normalized * height + offset;
            indices.Add(startIndex + vertices.Count + 0);  // p2b
            indices.Add(startIndex + vertices.Count + 1);  // p2t
            indices.Add(startIndex + vertices.Count - 1);
            indices.Add(startIndex + vertices.Count - 2);
            vertices.Add(p2b);
            vertices.Add(p2t);
        }
        // CYLINDER2
        vertices.Add(bab);
        vertices.Add(bat);
        for (int i = 0; i < numSteps; ++i)
        {
            // Adds the new face, sharing the last two vertices.
            var t2 = ((float) i + 1) / numSteps * Mathf.PI * 0.5F;
            var offset = (Mathf.Cos(t2) * perp + Mathf.Sin(t2) * prll) * thickness * 0.5F;
            var p2b = adjacentVertex2 + offset;
            var p2t = adjacentVertex2 + -adjacentVertex2.normalized * height + offset;
            indices.Add(startIndex + vertices.Count - 2);
            indices.Add(startIndex + vertices.Count - 1);
            indices.Add(startIndex + vertices.Count + 1);  // p2t
            indices.Add(startIndex + vertices.Count + 0);  // p2b
            vertices.Add(p2b);
            vertices.Add(p2t);
        }
        // QUAD
        indices.Add(startIndex + vertices.Count + 0);
        indices.Add(startIndex + vertices.Count + 1);
        indices.Add(startIndex + vertices.Count + 2);
        indices.Add(startIndex + vertices.Count + 3);
        vertices.Add(abb);
        vertices.Add(abt);
        vertices.Add(bat);
        vertices.Add(bab);
    }
    
    public void SetAdjacency(int i, bool isWall)
    {
        
    }

    public void UpdateMesh()
    {
        
    }
    
    
    public int VertexStartIndex;
    public int TriangleStartIndex;

    public bool[] WallAdjacency;
    
    public Vector3[] WallNormals;

    public MazeTile[] ConnectedTiles;
    
    public void RaiseWall(int index)
    {
                
    }

    public Vector3[] Points;
    public int[] Indices;
}
