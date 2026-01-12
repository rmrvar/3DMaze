using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField, Range(0, 5)] private float _speed = 1;
    
    private void Start()
    {
        _icosahedron = new Icosahedron();
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
        Debug.Log(_icosahedron.Triangles.Count);
    }

    private Icosahedron _icosahedron;
    private float _t;
}
