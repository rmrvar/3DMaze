using System.Collections.Generic;
using System.Linq;
using Maze;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static readonly int MazeCenterNameId = Shader.PropertyToID("_MazeCenter");
    private static readonly int MazeRadiusNameId = Shader.PropertyToID("_MazeRadius");
    private static readonly int WallHeightNameId = Shader.PropertyToID("_WallHeight");
    private static readonly int WallRadiusNameId = Shader.PropertyToID("_WallRadius");

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
    public float FullAnimDuration { get; private set; } = 2;

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

    public static GameManager Instance { get; private set; }

    public void RegenerateMaze()
    {
        _animProgress = 0;
        foreach (var wall in _wallMonos)
        {
            wall.ResetAnimProgress();
        }
        _mazeGenerator.DoKruskal();
        _isBottomPole = !_isBottomPole;
    }

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
        _animProgress += Time.deltaTime / FullAnimDuration;

        foreach (var wallMono in _wallMonos)
        {
            float minY = Mathf.Lerp(-MazeRadius, +MazeRadius, _animProgress);
            if (   
                // Start bottom up
                   ( _isBottomPole && wallMono.transform.position.y > +minY) 
                // Start top down
                || (!_isBottomPole && wallMono.transform.position.y < -minY)
              )
            {
                continue; // This wall is not ready to be updated yet.
            }
            wallMono.UpdateAnimProgress(Time.deltaTime);
        }
    }

    private readonly List<MazeWallMono> _wallMonos = new();
    private Topology _mazeTopology;
    private Generator _mazeGenerator;
    private float _animProgress = 1; // Maze starts with walls up/down.
    private bool _isBottomPole = true;
}
