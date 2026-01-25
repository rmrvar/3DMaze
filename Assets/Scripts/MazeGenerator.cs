using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField, Range(1, 100)]
    private float _radius;
    [SerializeField, Range(0, 10)]
    private int _numSubdivisions = 0;
    [SerializeField, Range(0, 10)] 
    private float _wallHeight = 2;
    [SerializeField, Range(0, 3)]
    private float _wallThickness = 0.1F;
    [SerializeField, Range(0, 20)] 
    private int _wallSmoothness = 5;
    [SerializeField]
    private Transform _spherePrefab;
    [SerializeField, Range(0, 1)]
    private float _skipThreshold = 0.15F;
    [SerializeField]
    private int _wallsPerSecond = 30;
    [SerializeField]
    private float _animDuration = 0.5F;
    [SerializeField]
    private Material _mazeMaterial;
    
    private void Awake()
    {
        _icosahedron = new Icosahedron(_radius, _numSubdivisions);
        _icosahedron.MeshData.OnTriangleAdded += OnTriangleAdded;
        _icosahedron.GenerateMesh();
        
        foreach (var (_, wall) in _edgeToWall)
        {
            wall.Construct(_icosahedron.MeshData, _vertices, _indices, _uvs1, _uvs2);
        }

        SkipCellsOnPoles();
        
        DoKruskal();

        _mesh = new Mesh();
        _mesh.SetVertices(_vertices);
        _mesh.SetUVs(0, _uvs1);
        _mesh.SetUVs(1, _uvs2);
        _mesh.SetIndices(_indices.ToArray(), MeshTopology.Quads, 0);
        _mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = _mesh;
        GetComponent<MeshCollider>().sharedMesh = _mesh;
        
        var sphere = Instantiate(_spherePrefab);
        sphere.localScale = Vector3.one * (2 * _radius - 1.5F);  // Needs to be a bit less deep so no gaps
        
        _mazeMaterial.SetFloat("_WallHeight", _wallHeight);
    }

    private void OnTriangleAdded(int i1, int i2, int i3)
    {
        var wall1 = RegisterEdge(i1, i2);
        var wall2 = RegisterEdge(i2, i3);
        var wall3 = RegisterEdge(i3, i1);
        var cell = RegisterFace(i1, i2, i3);
        ConnectWallToCell(wall1, cell);
        ConnectWallToCell(wall2, cell);
        ConnectWallToCell(wall3, cell);
    }

    private MazeWall RegisterEdge(int i1, int i2)
    {
        var key = new EdgeKey(i1, i2);
        if (!_edgeToWall.TryGetValue(key, out var wall))
        {
            wall = new MazeWall(key.V1, key.V2, _wallHeight, _wallThickness, _wallSmoothness, v => -v.normalized);
            _edgeToWall.Add(key, wall);
        }
        return wall;
    }

    private MazeCell RegisterFace(int i1, int i2, int i3)
    {
        var key = new FaceKey(i1, i2, i3);
        if (!_faceToCell.TryGetValue(key, out var cell))
        {
            cell = new MazeCell(i1, i2, i3);
            _faceToCell.Add(key, cell);
        }
        return cell;
    }

    private void ConnectWallToCell(MazeWall wall, MazeCell cell)
    {
        if (wall.Cell1 == null)
        {
            wall.Cell1 = cell;
        }
        else
        {
            wall.Cell2 = cell;
        }
        cell.Walls.Add(wall);
    }

    private void DoKruskal()
    {
        var faceToBranch = new Dictionary<FaceKey, int>();
        int faceCounter = 0;
        foreach (var face in _faceToCell.Keys)
        {
            faceToBranch[face] = faceCounter++;
        }

        var edges = _edgeToWall.Keys.ToArray();
        var shuffledIndices = Enumerable.Range(0, edges.Length)
            .OrderBy(x => Random.value)
            .ToArray();

        bool hasFilled1 = false;
        bool hasFilled2 = false;
        
        foreach (var index in shuffledIndices)
        {
            var wall = _edgeToWall[edges[index]];

            var isSkipped1Pole1 = _skippedCellsPole1.Contains(wall.Cell1);
            var isSkipped1Pole2 = _skippedCellsPole2.Contains(wall.Cell1);
            var isSkipped2Pole1 = _skippedCellsPole1.Contains(wall.Cell2);
            var isSkipped2Pole2 = _skippedCellsPole2.Contains(wall.Cell2);
            
            if (isSkipped1Pole1 && isSkipped2Pole1 || isSkipped1Pole2 && isSkipped2Pole2)
            {  // Don't fill the borders.
                wall.SetRaisedness(false);
                continue;
            }
            
            // Handle borders.
            var cell1 = wall.Cell1;
            var cell2 = wall.Cell2;
            if (cell2 == null ||  // Only possible in non-fully connected mazes
                (isSkipped1Pole1 || isSkipped2Pole1) && hasFilled1 ||
                (isSkipped1Pole2 || isSkipped2Pole2) && hasFilled2
              )
            {
                wall.SetRaisedness(true);
                continue;  // Can't divide.    
            }
            
            // Only allow one connection in top and bottom pole
            if (isSkipped1Pole1 || isSkipped2Pole1)
                hasFilled1 = true;
            if (isSkipped1Pole2 || isSkipped2Pole2)
                hasFilled2 = true;
            
            int idA = faceToBranch[new FaceKey(cell1.I1, cell1.I2, cell1.I3)];
            int idB = faceToBranch[new FaceKey(cell2.I1, cell2.I2, cell2.I3)];

            if (idA != idB)
            {
                wall.SetRaisedness(false);

                foreach (var face in faceToBranch.Keys.ToList())
                {
                    if (faceToBranch[face] == idB)
                    {
                        faceToBranch[face] = idA;
                    }
                }
            }
            else
            {
                wall.SetRaisedness(true);
            }
        }
    }

    private IEnumerator IE_DoKruskal(int wallsPerSecond)
    {
        var faceToBranch = new Dictionary<FaceKey, int>();
        int faceCounter = 0;
        foreach (var face in _faceToCell.Keys)
        {
            faceToBranch[face] = faceCounter++;
        }

        var edges = _edgeToWall.Keys.ToArray();
        var shuffledIndices = Enumerable.Range(0, edges.Length)
            .OrderBy(x => Random.value)
            .ToArray();

        bool hasFilled1 = false;
        bool hasFilled2 = false;

        var delay = 1.0F / wallsPerSecond;
        foreach (var index in shuffledIndices)
        {
            var wall = _edgeToWall[edges[index]];

            var isSkipped1Pole1 = _skippedCellsPole1.Contains(wall.Cell1);
            var isSkipped1Pole2 = _skippedCellsPole2.Contains(wall.Cell1);
            var isSkipped2Pole1 = _skippedCellsPole1.Contains(wall.Cell2);
            var isSkipped2Pole2 = _skippedCellsPole2.Contains(wall.Cell2);
            
            if (isSkipped1Pole1 && isSkipped2Pole1 || isSkipped1Pole2 && isSkipped2Pole2)
            {  // Don't fill the borders.
                wall.SetRaisedness(false);
                if (!_shouldFinishInstantly)
                {
                    yield return new WaitForSeconds(delay);   
                }
                continue;
            }
            
            // Handle borders.
            var cell1 = wall.Cell1;
            var cell2 = wall.Cell2;
            if (cell2 == null ||  // Only possible in non-fully connected mazes
                (isSkipped1Pole1 || isSkipped2Pole1) && hasFilled1 ||
                (isSkipped1Pole2 || isSkipped2Pole2) && hasFilled2
              )
            {
                wall.SetRaisedness(true);
                if (!_shouldFinishInstantly)
                {
                    yield return new WaitForSeconds(delay);   
                }
                continue;  // Can't divide.    
            }
            
            // Only allow one connection in top and bottom pole
            if (isSkipped1Pole1 || isSkipped2Pole1)
                hasFilled1 = true;
            if (isSkipped1Pole2 || isSkipped2Pole2)
                hasFilled2 = true;
            
            int idA = faceToBranch[new FaceKey(cell1.I1, cell1.I2, cell1.I3)];
            int idB = faceToBranch[new FaceKey(cell2.I1, cell2.I2, cell2.I3)];

            if (idA != idB)
            {
                wall.SetRaisedness(false);

                foreach (var face in faceToBranch.Keys.ToList())
                {
                    if (faceToBranch[face] == idB)
                    {
                        faceToBranch[face] = idA;
                    }
                }
            }
            else
            {
                wall.SetRaisedness(true);
            }
            if (!_shouldFinishInstantly)
            {
                yield return new WaitForSeconds(delay);   
            }
        } 
        Debug.Log("Finished generating!");
        _mesh.SetVertices(_vertices);
        _mesh.SetUVs(0, _uvs1);
        // _mesh.SetUVs(1, _uvs2);  // Stays the same
        // _mesh.SetIndices(_indices.ToArray(), MeshTopology.Quads, 0);  // Stays the same
        _mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = _mesh;
        GetComponent<MeshCollider>().sharedMesh = _mesh;
        _shouldFinishInstantly = false;
        
        float elapsed = 0;
        while (elapsed < _animDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / _animDuration);
            _mazeMaterial.SetFloat("_AnimProgress", progress);
            yield return null;
        }
        _mazeMaterial.SetFloat("_AnimProgress", 1);
    }
    
    private void SkipCellsOnPoles()
    {
        foreach (var cell in _faceToCell.Values)
        {
            var p1 = _icosahedron.MeshData.Vertices[cell.I1];
            var p2 = _icosahedron.MeshData.Vertices[cell.I2];
            var p3 = _icosahedron.MeshData.Vertices[cell.I3];
            var midpoint = (p1 + p2 + p3) / 3.0F;

            var dot = Vector3.Dot(midpoint.normalized, Vector3.up);

            if (dot > 1 - _skipThreshold)
            {
                _skippedCellsPole1.Add(cell);
            }
            if (dot < -1 + _skipThreshold)
            {
                _skippedCellsPole2.Add(cell);
            }
        }
        Debug.Log("Pole1: Skipped " + _skippedCellsPole1.Count + " faces");
        Debug.Log("Pole2: Skipped " + _skippedCellsPole2.Count + " faces");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Started changing!");
            StartCoroutine(IE_DoKruskal(_wallsPerSecond));
        }
        
    }

    private readonly Dictionary<EdgeKey, MazeWall> _edgeToWall = new();
    private readonly Dictionary<FaceKey, MazeCell> _faceToCell = new();
    private readonly List<Vector3> _vertices = new();
    private readonly List<int> _indices = new();
    private readonly List<Vector2> _uvs1 = new();  // New state/old state pairs
    private readonly List<Vector3> _uvs2 = new();  // Normals
    private readonly HashSet<MazeCell> _skippedCellsPole1 = new();
    private readonly HashSet<MazeCell> _skippedCellsPole2 = new();
    private Icosahedron _icosahedron;
    private Mesh _mesh;
    private bool _shouldFinishInstantly;
}
