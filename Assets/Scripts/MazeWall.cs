using System;
using System.Collections.Generic;
using UnityEngine;
using Mathf = UnityEngine.Mathf;

public class MazeWall
{
    public MazeWall(int i1, int i2, float height, float thickness, int smoothness, Func<Vector3, Vector3> getNormal)
    {
        // Need to order so that winding order will be guaranteed.
        if (i1 < i2)
        {
            I1 = i1;
            I2 = i2;
        }
        else
        {
            I1 = i2;
            I2 = i1;
        }
        Height = height;
        Thickness = thickness;
        Smoothness = smoothness;
        _getNormal = getNormal;
    }
    
    public int I1 { get; set; }
    public int I2 { get; set; }
    public float Height { get; private set; }
    public float Thickness { get; private set; }
    public int Smoothness { get; private set; }
    public MazeCell Cell1 { get; set; }
    public MazeCell Cell2 { get; set; }

    public MazeCell GetOpposite(MazeCell cell)
     => cell == Cell1 ? Cell2 : Cell1;
    
    public void SetRaisedness(bool raisedness)
    {
        Raisedness = raisedness;
        foreach (var topIndex in _topIndices)
        {
            if (raisedness)
            {
                _vertices[topIndex] = _topIndexToHeights[topIndex].Item2;
            }
            else
            {
                _vertices[topIndex] = _topIndexToHeights[topIndex].Item1;
            }
            
            var prevUv1 = _uvs1[topIndex];
            _uvs1[topIndex] = new Vector2(prevUv1.y, raisedness ? 1 : 0);
        }
    }
    
    public bool Raisedness { get; private set; }

    public void Construct(MeshData meshData, List<Vector3> vertices, List<int> indices, List<Vector2> uvs1, List<Vector3> uvs2)
    {
        _vertices = vertices;
        _uvs1 = uvs1;
        
        var v1 = meshData.Vertices[I1];
        var v2 = meshData.Vertices[I2];
        
        var forward = (v2 - v1).normalized;
        var up1 = _getNormal(v1);
        var up2 = _getNormal(v2);
        var right = Vector3.Cross(up1, forward).normalized;
        
        var topVertices1 = new Queue<Vector3>();
        var topIndices1 = new Queue<int>();
        // var topUvs1_1 = new Queue<Vector2>();
        // var topUvs2_1 = new Queue<Vector3>();
        var topVertices2 = new Queue<Vector3>();
        var topIndices2 = new Queue<int>();
        // var topUvs1_2 = new Queue<Vector2>();
        // var topUvs2_2 = new Queue<Vector3>();
        
        int originalVertexCount = vertices.Count;
        int numSteps = Smoothness;
        // HALF-CYLINDER 1
        for (int i = 0; i <= numSteps; ++i)
        {
            var t = (float) i / numSteps * Mathf.PI;
            var offset = (Mathf.Cos(t) * right + Mathf.Sin(t) * -forward) * Thickness * 0.5F;
            
            var pb = v1 + offset;
            var pt = pb + up1 * Height;

            indices.Add(vertices.Count + 2);  // next pb
            indices.Add(vertices.Count + 3);  // next pt
            indices.Add(vertices.Count + 1);  // pt
            indices.Add(vertices.Count + 0);  // pb
            vertices.Add(pb);
            uvs1.Add(new Vector2(1, 1));  // Raised at start
            uvs2.Add(up1);
            vertices.Add(pt);
            uvs1.Add(new Vector2(1, 1));  // Raised at start
            uvs2.Add(up1);
            
            topVertices1.Enqueue(pt);
            topIndices1.Enqueue(vertices.Count - 1);
            
            _topIndexToHeights.Add(vertices.Count - 1, (pb, pt));
        }
        // HALF-CYLINDER 2
        for (int i = 0; i <= numSteps; ++i)
        {
            var t = (float) i / numSteps * Mathf.PI;
            var offset = (Mathf.Cos(t) * -right + Mathf.Sin(t) * forward) * Thickness * 0.5F;
            
            var pb = v2 + offset;
            var pt = pb + up2 * Height;
            
            if (i == numSteps)
            {
                // Seal the loop.
                indices.Add(vertices.Count + 1);       // pt
                indices.Add(vertices.Count + 0);       // pb
                indices.Add(originalVertexCount + 0);  // Original pb
                indices.Add(originalVertexCount + 1);  // Original pt
            }
            else
            {
                indices.Add(vertices.Count + 2);  // next pb
                indices.Add(vertices.Count + 3);  // next pt
                indices.Add(vertices.Count + 1);  // pt
                indices.Add(vertices.Count + 0);  // pb
            }
            
            vertices.Add(pb);
            uvs1.Add(new Vector2(1, 1));  // Raised at start
            uvs2.Add(up2);
            vertices.Add(pt);
            uvs1.Add(new Vector2(1, 1));  // Raised at start
            uvs2.Add(up2);
            
            topVertices2.Enqueue(pt);
            topIndices2.Enqueue(vertices.Count - 1);
            _topIndexToHeights.Add(vertices.Count - 1, (pb, pt));
        }
        // TOP
        var topVertices = new List<Vector3>();  // TODO: Don't want to share verts with sides.
        var topIndices = new List<int>();
        while (topVertices1.Count > 0)
        {
            topVertices.Add(topVertices1.Dequeue());
            topIndices.Add(topIndices1.Dequeue());
        }
        while (topVertices2.Count > 0)
        {
            topVertices.Add(topVertices2.Dequeue());
            topIndices.Add(topIndices2.Dequeue());
        }
        for (int i = 0; i <= topIndices.Count - 4; ++i)
        {
            indices.Add(topIndices[i + 1]);
            indices.Add(topIndices[i + 2]);
            indices.Add(topIndices[i + 3]);
            indices.Add(topIndices[0]);      // Original top
        }
        _topIndices = topIndices;
    }

    private readonly Func<Vector3, Vector3> _getNormal;
    private readonly Dictionary<int, (Vector3, Vector3)> _topIndexToHeights = new();
    private List<int> _topIndices;
    private List<Vector3> _vertices;
    private List<Vector2> _uvs1;
}
