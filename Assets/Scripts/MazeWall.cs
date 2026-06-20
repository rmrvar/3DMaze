using System;
using System.Collections.Generic;
using Mathf = UnityEngine.Mathf;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MazeWall
{
    public MazeWall(int i1, int i2, float height, float thickness, int linearSmoothness, int radialSmoothness, Func<Vector3, Vector3> getNormal)
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
        LinearSmoothness = linearSmoothness;
        RadialSmoothness = radialSmoothness;
        if (RadialSmoothness % 2 == 0)
        {
            RadialSmoothness++; // Needs to be odd.
        }
        _getNormal = getNormal;
    }
    
    public int I1 { get; set; }
    public int I2 { get; set; }
    public float Height { get; private set; }
    public float Thickness { get; private set; }
    public int LinearSmoothness { get; private set; }
    public int RadialSmoothness { get; private set; }
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

        List<Vector3> radialDeltas1 = GetDeterministicRadialDeltas(v1, RadialSmoothness);
        List<Vector3> radialDeltas2 = GetDeterministicRadialDeltas(v2, RadialSmoothness);


        
        var forward = (v2 - v1).normalized;
        var right = Vector3.Cross(_getNormal(v1), forward).normalized;
        
        var topVertices1 = new Queue<Vector3>();
        var topIndices1 = new Queue<int>();
        var topUvs1_1 = new Queue<Vector2>();
        var topUvs2_1 = new Queue<Vector3>();
        var topVertices2 = new Queue<Vector3>();
        var topIndices2 = new Queue<int>();
        var topUvs1_2 = new Queue<Vector2>();
        var topUvs2_2 = new Queue<Vector3>();
        
        int originalVertexCount = vertices.Count;
        int numSteps = Mathf.CeilToInt(RadialSmoothness * 0.5F);
        // HALF-CYLINDER 1
        for (int i = 0; i <= numSteps; ++i)
        {
            var t = (float) i / numSteps * Mathf.PI;

            var delta = Mathf.Cos(t) * right + Mathf.Sin(t) * -forward;
            GetNearestDelta(delta, radialDeltas1, out delta);

            var offset = delta * Thickness * 0.5F;


            var pb = (v1 + offset).normalized * v1.magnitude;
            var normal = _getNormal(pb);
            var pt = pb + normal * Height;

            indices.Add(vertices.Count + 2);  // next pb
            indices.Add(vertices.Count + 3);  // next pt
            indices.Add(vertices.Count + 1);  // pt
            indices.Add(vertices.Count + 0);  // pb
            vertices.Add(pb);
            uvs1.Add(new Vector2(1, 1));  // Raised at start
            uvs2.Add(normal);
            vertices.Add(pt);
            uvs1.Add(new Vector2(1, 1));  // Raised at start
            uvs2.Add(normal);
            
            topVertices1.Enqueue(pt);
            topUvs1_1.Enqueue(new Vector2(1, 1));  // Raised at start
            topUvs2_1.Enqueue(normal);
            topIndices1.Enqueue(vertices.Count - 1);
            
            _topIndexToHeights.Add(vertices.Count - 1, (pb, pt));
        }
        // HALF-CYLINDER 2
        for (int i = 0; i <= numSteps; ++i)
        {
            var t = (float) i / numSteps * Mathf.PI;

            var delta = Mathf.Cos(t) * -right + Mathf.Sin(t) * forward;
            GetNearestDelta(delta, radialDeltas2, out delta);

            var offset = delta * Thickness * 0.5F;

            var pb = (v2 + offset).normalized * v2.magnitude;
            var normal = _getNormal(pb);
            var pt = pb + normal * Height;
            
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
            uvs2.Add(normal);
            vertices.Add(pt);
            uvs1.Add(new Vector2(1, 1));  // Raised at start
            uvs2.Add(normal);
            
            topVertices2.Enqueue(pt);
            topUvs1_2.Enqueue(new Vector2(1, 1));  // Raised at start
            topUvs2_2.Enqueue(normal);
            topIndices2.Enqueue(vertices.Count - 1);
            _topIndexToHeights.Add(vertices.Count - 1, (pb, pt));
        }
        // TOP
        var topVertices = new List<Vector3>();
        var topUvs1 = new List<Vector2>();
        var topUvs2 = new List<Vector3>();
        var topIndices = new List<int>();
        while (topVertices1.Count > 0)
        {
            topVertices.Add(topVertices1.Dequeue());
            topUvs1.Add(topUvs1_1.Dequeue());
            topUvs2.Add(topUvs2_1.Dequeue());
            topIndices.Add(topIndices1.Dequeue());
        }
        while (topVertices2.Count > 0)
        {
            topVertices.Add(topVertices2.Dequeue());
            topUvs1.Add(topUvs1_2.Dequeue());
            topUvs2.Add(topUvs2_2.Dequeue());
            topIndices.Add(topIndices2.Dequeue());
        }

        originalVertexCount = vertices.Count;
        for (int i = 0; i < topVertices.Count; ++i)
        {
            vertices.Add(topVertices[i]);
            uvs1.Add(topUvs1[i]);
            uvs2.Add(topUvs2[i]);
            topIndices.Add(originalVertexCount + i);
            _topIndexToHeights.Add(vertices.Count - 1, (topVertices[i] - topUvs2[i] * Height, topVertices[i]));
        }
        for (int i = 0; i <= topVertices.Count - 4; ++i)
        {
            indices.Add(originalVertexCount + i + 1);
            indices.Add(originalVertexCount + i + 2);
            indices.Add(originalVertexCount + i + 3);
            indices.Add(originalVertexCount);          // Original top
        }
        _topIndices = topIndices;
    }

    private static List<Vector3> GetDeterministicRadialDeltas(Vector3 center, int radialSmoothness)
    {
        Vector3 normal = -center.normalized;
        Vector3 up;
        Vector3 right;
        if (Vector3.Distance(normal, Vector3.right) > 0.5F)
        {
            up = Vector3.Cross(normal, Vector3.right);
            right = Vector3.Cross(normal, up);
        }
        else
        {
            up = Vector3.Cross(normal, Vector3.up);
            right = Vector3.Cross(normal, up);
        }

        float radiansPerStep = (360 * Mathf.Deg2Rad) / radialSmoothness;

        List<Vector3> deltas = new(radialSmoothness);
        for (int i = 0; i < radialSmoothness; ++i)
        {
            float theta = i * radiansPerStep;

            Vector3 delta = (Mathf.Cos(theta) * up + Mathf.Sin(theta) * right).normalized;
            deltas.Add(delta);
        }

        return deltas;
    }

    private static int GetNearestDelta(Vector3 delta, IReadOnlyList<Vector3> deltas, out Vector3 nearestDelta)
    {
        nearestDelta = Vector3.zero;
        int minIndex = -1;
        float minTheta = float.MaxValue;
        for (int i = 0; i < deltas.Count; ++i)
        {
            Vector3 delta2 = deltas[i];
            float theta = Vector3.Angle(delta, delta2);
            if (theta < minTheta)
            {
                minTheta = theta;
                minIndex = i;
                nearestDelta = delta2;
            }
        }
        return minIndex;
    }

    private readonly Func<Vector3, Vector3> _getNormal;
    private readonly Dictionary<int, (Vector3, Vector3)> _topIndexToHeights = new();
    private List<int> _topIndices;
    private List<Vector3> _vertices;
    private List<Vector2> _uvs1;
}
