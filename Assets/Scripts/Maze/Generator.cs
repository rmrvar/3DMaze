using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Maze
{
    public class Generator
    {
        public Topology Topology { get; }

        public Generator(Topology topology, Action<Wall> onCreateWall)
        {
            Debug.Assert(topology != null, "Topology is null!");
            Topology = topology;
            _onCreateWall = onCreateWall;
            for (int i = 0; i < topology.Triangles.Count - 2; i += 3)
            {
                ProcessTriangle(
                    topology.Triangles[i],
                    topology.Triangles[i + 1],
                    topology.Triangles[i + 2]
                  );
            }
        }

        public void DoKruskal()
        {
            var faceToBranch = new Dictionary<CellKey, int>();
            int faceCounter = 0;
            foreach (var face in _faceToCell.Keys)
            {
                faceToBranch[face] = faceCounter++;
            }

            var edges = _edgeToWall.Keys.ToArray();
            var shuffledIndices = Enumerable.Range(0, edges.Length)
                .OrderBy(x => Random.value)
                .ToArray();

            foreach (var index in shuffledIndices)
            {
                var wall = _edgeToWall[edges[index]];

                // Handle borders.
                var cell1 = wall.Cell1;
                var cell2 = wall.Cell2;
                if (cell2 == null)
                {
                    wall.SetRaisedness(true);
                    continue; // Can't divide.    
                }

                int idA = faceToBranch[cell1.Key];
                int idB = faceToBranch[cell2.Key];

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

        private void ProcessTriangle(int i1, int i2, int i3)
        {
            var wall1 = RegisterWall(i1, i2);
            var wall2 = RegisterWall(i2, i3);
            var wall3 = RegisterWall(i3, i1);
            var cell = RegisterCell(i1, i2, i3);
            ConnectWallToCell(wall1, cell);
            ConnectWallToCell(wall2, cell);
            ConnectWallToCell(wall3, cell);
        }

        private Wall RegisterWall(int i1, int i2)
        {
            var key = new WallKey(i1, i2);
            if (!_edgeToWall.TryGetValue(key, out var wall))
            {
                wall = new Wall(Topology, key);
                _onCreateWall?.Invoke(wall);
                _edgeToWall.Add(key, wall);
            }

            return wall;
        }

        private Cell RegisterCell(int i1, int i2, int i3)
        {
            var key = new CellKey(i1, i2, i3);
            if (!_faceToCell.TryGetValue(key, out var cell))
            {
                cell = new Cell(key);
                _faceToCell.Add(key, cell);
            }

            return cell;
        }

        private void ConnectWallToCell(Wall wall, Cell cell)
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

        private readonly Action<Wall> _onCreateWall;

        private readonly Dictionary<WallKey, Wall> _edgeToWall = new();
        private readonly Dictionary<CellKey, Cell> _faceToCell = new();
    }
}