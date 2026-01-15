using UnityEngine;

public class MazeTile
{
    public void CreateMesh(Triangle triangle, int startIndex, float wallThickness = 0.1F, float wallHeight = 2.0F)
    {
        var ab = triangle.Points[1] - triangle.Points[0];
        var ac = triangle.Points[2] - triangle.Points[0];
        var ba = triangle.Points[0] - triangle.Points[1];
        var bc = triangle.Points[2] - triangle.Points[1];
        
        var perp1 = (ac - Vector3.Dot(ab.normalized, ac) * ab.normalized).normalized * wallThickness * 0.5F;
        var perp2 = (bc - Vector3.Dot(ba.normalized, bc) * ba.normalized).normalized * wallThickness * 0.5F;
        // var perp3 = 0;
        
        // var center = triangle.Center;
        // var normal = -center.normalized;
        var abb = triangle.Points[0] + perp1;
        var bab = triangle.Points[1] + perp2;
        var abt = abb + -triangle.Points[0].normalized * wallHeight;        
        var bat = bab + -triangle.Points[1].normalized * wallHeight;
        Points = new[] { abb, bab, abt, bat };
        Indices = new[]
        {
            // Walls
            // abb, abt, bat, bab
            0, 2, 3, 1
            // acb, act, cat, cab
            // bcb, bct, cbt, cbb
        };
        
        
        // var center = triangle.Center;
        // var normal = -center.normalized;
        // var abb = triangle.Points[0] + (triangle.Points[2] - triangle.Points[0]) * wallThickness * 0.5F;
        // var acb = triangle.Points[0] + (triangle.Points[1] - triangle.Points[0]) * wallThickness * 0.5F;
        // var bab = triangle.Points[1] + (triangle.Points[2] - triangle.Points[1]) * wallThickness * 0.5F;
        // var bcb = triangle.Points[1] + (triangle.Points[0] - triangle.Points[1]) * wallThickness * 0.5F;
        // var cab = triangle.Points[2] + (triangle.Points[1] - triangle.Points[2]) * wallThickness * 0.5F;
        // var cbb = triangle.Points[2] + (triangle.Points[0] - triangle.Points[2]) * wallThickness * 0.5F;
        // var abt = abb + normal * wallHeight;        
        // var act = acb + normal * wallHeight;
        // var bat = bab + normal * wallHeight;        
        // var bct = bcb + normal * wallHeight;
        // var cat = cab + normal * wallHeight;        
        // var cbt = cbb + normal * wallHeight;
        // Points = new[] { abb, acb, bab, bcb, cab, cbb, abt, act, bat, bct, cat, cbt };
        // Indices = new[]
        // {
        //     // Walls
        //     // abb, abt, bat, bab
        //     0, 6, 8, 2
        //     // acb, act, cat, cab
        //     // bcb, bct, cbt, cbb
        // };
        
        
        
        
        // // bottom 3 points
        // var b1 = (center - triangle.Points[0]).normalized * wallThickness + triangle.Points[0];
        // var b2 = (center - triangle.Points[1]).normalized * wallThickness + triangle.Points[1];
        // var b3 = (center - triangle.Points[2]).normalized * wallThickness + triangle.Points[2];
        // // top 3 points (inner)
        // var ti1 = b1 + normal * wallHeight;
        // var ti2 = b2 + normal * wallHeight;
        // var ti3 = b3 + normal * wallHeight;
        // // top 3 points (outer)
        // var to1 = triangle.Points[0] + normal * wallHeight;
        // var to2 = triangle.Points[1] + normal * wallHeight;
        // var to3 = triangle.Points[2] + normal * wallHeight;
        //
        // Points = new[] { b1, b2, b3, ti1, ti2, ti3, to1, to2, to3 };
        // Indices = new[]
        // {
        //     // Walls
        //     // b1, ti1, ti2, b2
        //     0, 3, 4, 1,
        //     // b2, ti2, ti3, b3
        //     1, 4, 5, 2,
        //     // b3, ti3, ti1, b1
        //     2, 5, 3, 0,
        //     
        //     // Top floor
        //     
        //     
        // };
        for (int i = 0; i < Indices.Length; ++i)
        {
            Indices[i] += startIndex;
        }
    }

    public void SetAdjacency(int i, bool isWall)
    {
        
    }

    public void UpdateMesh()
    {
        
    }
    
    
    public int VertexStartIndex;
    public int TriangleStartIndex;

    public bool[] WallAdjacency;
    
    public Vector3[] WallNormals;

    public MazeTile[] ConnectedTiles;
    
    public void RaiseWall(int index)
    {
                
    }

    public Vector3[] Points;
    public int[] Indices;
}
