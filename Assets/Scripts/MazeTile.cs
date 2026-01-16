using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeTile
{
    public const int CYLINDER_QUAD_COUNT = 5;
    public const int NUM_VERTICES = 3 * (
        (2 + (2 * CYLINDER_QUAD_COUNT)) +    // Cylinder 1
        (2 + (2 * CYLINDER_QUAD_COUNT)) +    // Cylinder 2
        4 +                                  // Quad
        (2 * (CYLINDER_QUAD_COUNT + 1))      // Top
      );
    public const int NUM_INDICES = 3 * (
        (CYLINDER_QUAD_COUNT * 4) +                       // Cylinder 1
        (CYLINDER_QUAD_COUNT * 4) +                       // Cylinder 2
        4 +                                               // Quad
        (((2 * (CYLINDER_QUAD_COUNT + 1)) - 3) * 4)       // Top
      );
    
    public void CreateMesh(
        Triangle triangle, 
        Vector3[] vertices, 
        int[] indices, 
        ref int vIndex, 
        ref int iIndex, 
        float wallThickness = 0.1F, 
        float wallHeight = 2.0F
      )
    {
        CreateWall(triangle.Points[0], triangle.Points[1], triangle.Points[2], vertices, indices, ref vIndex, ref iIndex, wallThickness, wallHeight);
        CreateWall(triangle.Points[1], triangle.Points[2], triangle.Points[0], vertices, indices, ref vIndex, ref iIndex, wallThickness, wallHeight);
        CreateWall(triangle.Points[2], triangle.Points[0], triangle.Points[1], vertices, indices, ref vIndex, ref iIndex, wallThickness, wallHeight);
    }

    private void CreateWall(
        Vector3 adjacentVertex1, 
        Vector3 adjacentVertex2, 
        Vector3 oppositeVertex, 
        Vector3[] vertices,
        int[] indices,
        ref int vIndex,
        ref int iIndex,
        float thickness, 
        float height
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
        
        var tops1 = new Stack<Vector3>();
        var tops2 = new Queue<Vector3>();
        
        int numSteps = CYLINDER_QUAD_COUNT;
        // CYLINDER1
        vertices[vIndex++] = abb;
        vertices[vIndex++] = abt;
        tops1.Push(abt);
        for (int i = 0; i < numSteps; ++i)
        {
            // Adds the new face, sharing the last two vertices.
            var t2 = ((float) i + 1) / numSteps * Mathf.PI * 0.5F;
            var offset = (Mathf.Cos(t2) * perp + Mathf.Sin(t2) * -prll) * thickness * 0.5F;
            var p2b = adjacentVertex1 + offset;
            var p2t = adjacentVertex1 + -adjacentVertex1.normalized * height + offset;
            indices[iIndex++] = vIndex;      // p2b
            indices[iIndex++] = vIndex + 1;  // p2t
            indices[iIndex++] = vIndex - 1;
            indices[iIndex++] = vIndex - 2;
            vertices[vIndex++] = p2b;
            vertices[vIndex++] = p2t;
            tops1.Push(p2t);
        }
        // CYLINDER2
        vertices[vIndex++] = bab;
        vertices[vIndex++] = bat;
        tops2.Enqueue(bat);
        for (int i = 0; i < numSteps; ++i)
        {
            // Adds the new face, sharing the last two vertices.
            var t2 = ((float) i + 1) / numSteps * Mathf.PI * 0.5F;
            var offset = (Mathf.Cos(t2) * perp + Mathf.Sin(t2) * prll) * thickness * 0.5F;
            var p2b = adjacentVertex2 + offset;
            var p2t = adjacentVertex2 + -adjacentVertex2.normalized * height + offset;
            indices[iIndex++] = vIndex - 2;
            indices[iIndex++] = vIndex - 1;
            indices[iIndex++] = vIndex + 1;  // p2t
            indices[iIndex++] = vIndex;      // p2b
            vertices[vIndex++] = p2b;
            vertices[vIndex++] = p2t;
            tops2.Enqueue(p2t);
        }
        // QUAD
        indices[iIndex++] = vIndex;
        indices[iIndex++] = vIndex + 1;
        indices[iIndex++] = vIndex + 2;
        indices[iIndex++] = vIndex + 3;
        vertices[vIndex++] = abb;
        vertices[vIndex++] = abt;
        vertices[vIndex++] = bat;
        vertices[vIndex++] = bab;
        // TOP
        var tops = new List<Vector3>();
        while (tops1.Count > 0) tops.Add(tops1.Pop());
        while (tops2.Count > 0) tops.Add(tops2.Dequeue());
        for (int i = 0; i <= tops.Count - 4; ++i)
        {
            indices[iIndex++] = vIndex;
            indices[iIndex++] = vIndex + i + 3;
            indices[iIndex++] = vIndex + i + 2;
            indices[iIndex++] = vIndex + i + 1;
        }
        for (int i = 0; i < tops.Count; ++i)
        {
            vertices[vIndex++] = tops[i];
        }
    }
}
