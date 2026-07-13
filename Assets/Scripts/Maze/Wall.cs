using System;
using UnityEngine;

namespace Maze
{
    public class Wall
    {
        public Wall(Topology topology, WallKey key)
        {
            Key = key;
            Position1 = topology.Vertices[key.V1];
            Position2 = topology.Vertices[key.V2];
        }

        public WallKey Key { get; }
        public Cell Cell1 { get; set; }
        public Cell Cell2 { get; set; }

        public Vector3 Position1 { get; }
        public Vector3 Position2 { get; }

        public bool IsRaised { get; private set; } = false;
        public event Action OnRaise = delegate { };
        public event Action OnLower = delegate { };

        public void SetRaisedness(bool isRaised)
        {
            IsRaised = isRaised;
            if (isRaised)
            {
                OnRaise();
            }
            else
            {
                OnLower();
            }
        }
    }
}