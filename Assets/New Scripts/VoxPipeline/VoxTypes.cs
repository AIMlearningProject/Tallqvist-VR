using UnityEngine;

public class Voxel
{
    public int x, y, z;
    public byte r, g, b;
    public byte colorIndex;
}

public class VoxData
{
    public Voxel[] voxels;
    public Color32[] palette; // Fixed length, 256 colors.
    public int sizeX, sizeY, sizeZ;

    public VoxData(Voxel[] voxels, Color32[] palette, int sx, int sy, int sz)
    {
        this.voxels = voxels;
        this.palette = palette;
        this.sizeX = sx;
        this.sizeY = sy;
        this.sizeZ = sz;
    }
}
