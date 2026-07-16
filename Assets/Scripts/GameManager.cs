using System.Collections.Generic;
using Maze;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly int MazeCenterNameId = Shader.PropertyToID("_MazeCenter");
    private static readonly int MazeRadiusNameId = Shader.PropertyToID("_MazeRadius");
    private static readonly int WallHeightNameId = Shader.PropertyToID("_WallHeight");
    private static readonly int WallRadiusNameId = Shader.PropertyToID("_WallRadius");
    private static readonly int AnimProgressNameId = Shader.PropertyToID("_AnimProgress");

    [Header("Maze Settings")]
    [field: SerializeField]
    public float MazeRadius;
    [field: SerializeField]
    public int SubdivisionCount;
    [field: SerializeField]
    public float HoleAngle;
    [field: SerializeField]
    public float WallHeight;
    [field: SerializeField]
    public float WallRadius;
    [field: SerializeField]
    public float AnimSpeed;

    [field: SerializeField]
    public Material WallMaterial;
    [field: SerializeField]
    public Material FloorMaterial;

    [field: SerializeField]
    public MazeWallMono _wallPrefab;

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        _mazeTopology = new Icosahedron(MazeRadius, SubdivisionCount, HoleAngle);
        _mazeGenerator = new Generator(_mazeTopology, OnCreateWall);

        _mazeGenerator.DoKruskal();

        WallMaterial.SetVector(MazeCenterNameId, Vector3.zero);
        WallMaterial.SetFloat(MazeRadiusNameId, MazeRadius);
        WallMaterial.SetFloat(WallHeightNameId, WallHeight);
        WallMaterial.SetFloat(WallRadiusNameId, WallRadius);


        FloorMaterial.SetVector(MazeCenterNameId, Vector3.zero);
        FloorMaterial.SetFloat(MazeRadiusNameId, MazeRadius);

        // TODO: Put the code from the old sphere inverter here.
    }

    private void OnCreateWall(Wall wall)
    {
        Debug.Log("Creating wall");
        var wallMono = Instantiate(_wallPrefab);
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
