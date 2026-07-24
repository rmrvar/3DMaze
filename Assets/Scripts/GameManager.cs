using System.Collections.Generic;
using Maze;
using UnityEngine;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    private static readonly int MazeCenterNameId = Shader.PropertyToID("_MazeCenter");
    private static readonly int MazeRadiusNameId = Shader.PropertyToID("_MazeRadius");
    private static readonly int WallHeightNameId = Shader.PropertyToID("_WallHeight");
    private static readonly int WallRadiusNameId = Shader.PropertyToID("_WallRadius");
    private static readonly int AnimProgressNameId = Shader.PropertyToID("_AnimProgress");

    [Header("Maze Settings")]
    [field: SerializeField]
    public float MazeRadius { get; private set; }
    [field: SerializeField]
    public int SubdivisionCount { get; private set; }
    [field: SerializeField]
    public float HoleAngle { get; private set; }
    [field: SerializeField]
    public float WallHeight { get; private set; }
    [field: SerializeField]
    public float WallRadius { get; private set; }
    [field: SerializeField]
    public float AnimSpeed { get; private set; }

    [field: SerializeField]
    public Material WallMaterial { get; private set; }
    [field: SerializeField]
    public Material FloorMaterial { get; private set; }

    [field: SerializeField]
    public Color WallTopColor { get; private set; }
    [field: SerializeField]
    public Color WallSideColor { get; private set; }
    [field: SerializeField]
    public Color FloorColor { get; private set; }

    [field: SerializeField]
    public MazeWallMono WallPrefab { get; private set; }
    [field: SerializeField]
    public MazeFloorMono FloorPrefab { get; private set; }

    public static GameManager Instance { get; private set; }

    public void RegenerateMaze()
    {
        _animProgress = 0;
        _mazeGenerator.DoKruskal();
    }

    private void Awake()
    {
        Instance = this;

        Instantiate(FloorPrefab);

        _mazeTopology = new Icosahedron(MazeRadius, SubdivisionCount, HoleAngle);
        _mazeGenerator = new Generator(_mazeTopology, OnCreateWall);

        _mazeGenerator.DoKruskal();

        WallMaterial.SetVector(MazeCenterNameId, Vector3.zero);
        WallMaterial.SetFloat(MazeRadiusNameId, MazeRadius);
        WallMaterial.SetFloat(WallHeightNameId, WallHeight);
        WallMaterial.SetFloat(WallRadiusNameId, WallRadius);
        WallMaterial.SetVector("_TopColor", WallTopColor);
        WallMaterial.SetVector("_SideColor", WallSideColor);
    }

    private void OnCreateWall(Wall wall)
    {
        Debug.Log("Creating wall");
        var wallMono = Instantiate(WallPrefab);
        wallMono.Init(wall);
        _wallMonos.Add(wallMono);
    }

    private void Update()
    {
        _animProgress += Time.deltaTime * AnimSpeed;
        WallMaterial.SetFloat(AnimProgressNameId, _animProgress);

        foreach (var wallMono in _wallMonos)
        {
            wallMono.SetAnimProgress(_animProgress);
        }
    }

    private readonly List<MazeWallMono> _wallMonos = new();
    private Topology _mazeTopology;
    private Generator _mazeGenerator;
    private float _animProgress = 1; // Maze starts with walls up/down.
}
