using System.Collections.Generic;
using UnityEngine;

public class SphereInverter : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;
    [SerializeField]
    private MeshCollider _meshCollider;
    
    private void Start()
    {
        var mesh = _meshFilter.mesh;
        var triangles = new List<int>();
        mesh.GetTriangles(triangles, 0);
        var newTriangles = new List<int>(triangles.Count);

        for (int i = 0; i < triangles.Count; i += 3)
        {
            newTriangles.Add(triangles[i + 2]);
            newTriangles.Add(triangles[i + 1]);
            newTriangles.Add(triangles[i + 0]);
        }
        var newNormals = new List<Vector3>(mesh.normals);
        for (int i = 0; i < newNormals.Count; i++)
        {
            newNormals[i] = -newNormals[i];
        }
        mesh.SetTriangles(newTriangles, 0);
        mesh.SetNormals(newNormals);
        mesh.RecalculateNormals();
        _meshFilter.sharedMesh = mesh;
        _meshCollider.sharedMesh = null;
        _meshCollider.sharedMesh = mesh;
    }
}
