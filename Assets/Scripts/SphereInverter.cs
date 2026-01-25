using System.Collections.Generic;
using UnityEngine;

public class SphereInverter : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;
    
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
        mesh.SetTriangles(newTriangles, 0);
        mesh.RecalculateNormals();
    }
}
