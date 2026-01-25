using System.Collections.Generic;
using UnityEngine;

public class MeshData 
{
    public List<Vector3> Vertices = new();
    public List<int> Indices = new();

    public System.Action<int, int, int> OnTriangleAdded;

    public int AddVertex(Vector3 vertex)
    {
        var key = new VertexKey(vertex);
        if (!_vertexToIndex.TryGetValue(key, out int index)) 
        {
            index = Vertices.Count;
            Vertices.Add(vertex);
            _vertexToIndex.Add(key, index);
        }
        return index;
    }

    public void AddTriangle(int i1, int i2, int i3) 
    {
        Indices.Add(i1);
        Indices.Add(i2);
        Indices.Add(i3);
        OnTriangleAdded?.Invoke(i1, i2, i3);
    }
    
    private readonly Dictionary<VertexKey, int> _vertexToIndex = new();
}