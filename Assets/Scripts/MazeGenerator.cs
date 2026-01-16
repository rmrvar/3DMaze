using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField, Range(0, 5)] private float _speed = 1;
    [SerializeField, Range(0, 10)] private int _subdivisions = 1;
    [SerializeField, Range(1, 20)] private int _radius = 10;
    [SerializeField, Range(0, 3)] private float _wallHeight = 2;
    [SerializeField, Range(0, 1)] private float _wallThickness = 0.1F;
    
    
    [SerializeField] 
    private MeshFilter _floorMeshFilter;
    [SerializeField]
    private MeshFilter _wallsMeshFilter;
    
    private void Start()
    {
        _icosahedron = new Icosahedron(_radius, _subdivisions);
        
        var floorMesh = new Mesh();
        var verts = new List<Vector3>();
        var indices = new List<int>();

        for (int triangleIndex = 0; triangleIndex < _icosahedron.Triangles2.Count; ++triangleIndex)
        {
            var triangle = _icosahedron.Triangles2[triangleIndex];
            for (int i = 0; i < triangle.Points.Count; ++i)
            {
                var pointIndex = triangleIndex * 3 + i;
                
                verts.Add(triangle.Points[i]);
                indices.Add(pointIndex);
            }
        }
        
        floorMesh.vertices = verts.ToArray();
        floorMesh.triangles = indices.ToArray();
        floorMesh.RecalculateNormals();
        _floorMeshFilter.mesh = floorMesh;
        
        
        // Preallocate Vector3[] verts, int[] indices, ref vOffset ref iOffset (in and out in ref)  
        var verts2 = new Vector3[_icosahedron.Triangles2.Count * MazeTile.NUM_VERTICES];
        var indices2 = new int[_icosahedron.Triangles2.Count * MazeTile.NUM_INDICES];
        int vIndex = 0;
        int iIndex = 0;
        for (int i = 0; i < _icosahedron.Triangles2.Count; ++i)
        // for (int i = 0; i < 3; ++i)
        {       
            var mazeTile = new MazeTile();
            mazeTile.CreateMesh(_icosahedron.Triangles2[i], verts2, indices2, ref vIndex, ref iIndex, _wallThickness, _wallHeight);
        }
        var wallsMesh = new Mesh();
        wallsMesh.MarkDynamic();
        wallsMesh.SetVertices(verts2);
        wallsMesh.SetIndices(indices2, MeshTopology.Quads, 0);
        wallsMesh.RecalculateNormals();  
        _wallsMeshFilter.mesh = wallsMesh;
    }
    
    private void Update()
    {
        _t += Time.deltaTime * _speed;
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
    
    private Icosahedron _icosahedron;
    private float _t;
}
