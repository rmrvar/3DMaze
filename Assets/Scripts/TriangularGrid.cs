using System.Collections.Generic;
using UnityEngine;

public class TriangularGrid
{
    public MeshData MeshData { get; }
    
    public TriangularGrid(Vector2Int size)
    {
        _size = size;
        MeshData = new MeshData
        {
            Vertices = new List<Vector3>(_size.x * _size.y * 2),
            Indices = new List<int>(_size.x * _size.y * 2 * 3)
        };
    }
    
    public void GenerateMesh()
    {
        var c = Mathf.Sin(60 * Mathf.Deg2Rad);
        for (int x = 0; x < _size.x; ++x)
        for (int z = 0; z < _size.y; ++z)
        {
            var i1 = MeshData.AddVertex(new Vector3(z * 0.5F + x       , 0, z * c    ));
            var i2 = MeshData.AddVertex(new Vector3(z * 0.5F + x + 0.5F, 0, z * c + c));
            var i3 = MeshData.AddVertex(new Vector3(z * 0.5F + x + 1.0F, 0, z * c    ));
            var i4 = MeshData.AddVertex(new Vector3(z * 0.5F + x + 1.5F, 0, z * c + c));
            MeshData.AddTriangle(i1, i2, i3);
            MeshData.AddTriangle(i2, i3, i4);
        }
    }
    
    private readonly Vector2Int _size;
}
