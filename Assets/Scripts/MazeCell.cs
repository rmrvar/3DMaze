using System.Collections.Generic;
using UnityEngine;

public class MazeCell
{
    public MazeCell(int i1, int i2, int i3)
    {
        I1 = i1;
        I2 = i2;
        I3 = i3;
    }
    
    public int I1 { get; private set; }
    public int I2 { get; private set; }
    public int I3 { get; private set; }
    public List<MazeWall> Walls { get; } = new(3);

    public int GetOpposite(int i1, int i2)
    {
        if (i1 == I1 && i2 == I2 || i1 == I2 && i2 == I1)
        {
            return I3;
        }
        if (i1 == I1 && i2 == I3 || i1 == I3 && i2 == I1)
        {
            return I2;
        } 
        if (i1 == I2 && i2 == I3 || i1 == I3 && i2 == I2)
        {
            return I1;
        }
        Debug.Assert(false, "GetOpposite(): This shouldn't happen!");
        return -1;
    }
}
