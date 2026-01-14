using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField, Range(0, 5)] private float _speed = 1;
    [SerializeField, Range(0, 5)] private int _subdivisions = 1;
    [SerializeField, Range(1, 20)] private int _radius = 10;
    
    private void Start()
    {
        _icosahedron = new Icosahedron(_radius, _subdivisions);
        _meshFilter = GetComponent<MeshFilter>();
        
        
        var mesh = new Mesh();
        var verts = new List<Vector3>();
        var indices = new List<int>();

        for (int triangleCount = 0; triangleCount < _icosahedron.Triangles2.Count; ++triangleCount)
        {
            var triangle = _icosahedron.Triangles2[triangleCount];
            for (int i = 0; i < triangle.Points.Count; ++i)
            {
                var pointIndex = triangleCount * 3 + i;
                
                verts.Add(triangle.Points[i]);
                indices.Add(pointIndex);
            }
        }
        mesh.vertices = verts.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
    }

    private void Update()
    {
        // _t += Time.deltaTime * _speed;
    }
    
    private void OnDrawGizmosSelected()
    {
        foreach (var vertex in _icosahedron.Vertices)
        {
            Gizmos.DrawSphere(vertex, 0.1F);   
        }
        
        var index = Mathf.FloorToInt(_t) % 20;
        var triangle = _icosahedron.Triangles[index];
        for (int i = 0; i < 3; ++i)
        {
            var point = triangle.Points[i];
            Gizmos.color = i == 0 ? Color.red : Color.blue;
            Gizmos.DrawSphere(point, 0.2F);
        }
        // Debug.Log(_icosahedron.Triangles.Count);
        //
        // Gizmos.color = Color.green;
        // foreach (var triangle2 in _icosahedron.Triangles2)
        // // var triangle2 = _icosahedron.Triangles2[index * 4];
        // {
        //     foreach (var point in triangle2.Points)
        //     {
        //         Gizmos.DrawSphere(point, 0.3F);
        //     }
        // }
    }

    private MeshFilter _meshFilter;
    private Icosahedron _icosahedron;
    private float _t;
}
