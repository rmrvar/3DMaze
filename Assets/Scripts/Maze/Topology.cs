using System.Collections.Generic;
using UnityEngine;

namespace Maze
{
    public abstract class Topology
    {
        public List<Vector3> Vertices { get; protected set; } = new();
        public List<int> Triangles { get; protected set; } = new();

        protected readonly Dictionary<VertexKey, int> VertexToIndex = new();

        protected int AddVertex(Vector3 vertex)
        {
            var key = new VertexKey(vertex);
            if (!VertexToIndex.TryGetValue(key, out int index))
            {
                index = Vertices.Count;
                Vertices.Add(vertex);
                VertexToIndex.Add(key, index);
            }

            return index;
        }

        protected void AddTriangle(int i1, int i2, int i3)
        {
            Triangles.Add(i1);
            Triangles.Add(i2);
            Triangles.Add(i3);
        }
    }
}
