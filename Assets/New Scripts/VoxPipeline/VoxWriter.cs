using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public static class VoxWriter
{
    public static void Save(string path, VoxData data)
    {
        using var fs = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        // Helpers
        void W(string s) => writer.Write(Encoding.ASCII.GetBytes(s));
        void I(int i) => writer.Write(i);

        // VOX header, cubizer needs vox version 150
        W("VOX "); I(150);

        // MAIN chunk header
        W("MAIN"); I(0);
        long sizePos = fs.Position;
        I(0);

        // Build body in memory
        using var ms = new MemoryStream();
        using var cw = new BinaryWriter(ms);

        // PACK --> 1 model
        cw.Write(Encoding.ASCII.GetBytes("PACK"));
        cw.Write(4); cw.Write(0); cw.Write(1);

        // Group colors by voxel grid cell
        var voxelColorMap = new Dictionary<Vector3Int, List<Color32>>();
        foreach (var v in data.voxels)
        {
            var cell = new Vector3Int(v.x, v.y, v.z);
            if (!voxelColorMap.ContainsKey(cell))
                voxelColorMap[cell] = new List<Color32>();
            voxelColorMap[cell].Add(new Color32(v.r, v.g, v.b, 255));
        }

        // Assign averaged colors to voxels.
        var palette = BuildPalette332();
        var averagedVoxels = new List<Voxel>();

        foreach (var kvp in voxelColorMap)
        {
            var avg = AverageColor(kvp.Value);
            byte idx = GetClosestPaletteIndex(avg, palette);

            averagedVoxels.Add(new Voxel
            {
                x = kvp.Key.x,
                y = kvp.Key.y,
                z = kvp.Key.z,
                r = avg.r,
                g = avg.g,
                b = avg.b,
                colorIndex = idx
            });
        }

        data.voxels = averagedVoxels.ToArray();

        // SIZE + XYZI → model 0
        Write_SIZE(cw, data.sizeX, data.sizeY, data.sizeZ);

        for (int i = 0; i < data.voxels.Length; i++)
        {
            var v = data.voxels[i];
            data.voxels[i] = v;
        }

        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"Voxel {i}: rgb=({data.voxels[i].r},{data.voxels[i].g},{data.voxels[i].b}) → index={data.voxels[i].colorIndex}");
        }

        Write_XYZI(cw, data.voxels);

        for (int i = 0; i < data.voxels.Length; i++)
        {
            var v = data.voxels[i];
            Color32 voxelColor = new Color32(v.r, v.g, v.b, 255);
            v.colorIndex = GetClosestPaletteIndex(voxelColor, palette);
            data.voxels[i] = v;
        }

        Write_RGBA(cw, palette);

        // 3d Scene graph for model 0
        // Shape node #0 → model 0
        Write_nSHP(cw, 0, 0);

        // Transform node #1 → shape #0 @ (0,0,0)
        Write_nTRN(cw, 1, 0, Vector3Int.zero);

        // Group node #2 → children: [1]
        Write_nGRP(cw, 2, new[] { 1 });

        // Root transform #3 → group #2 @ (0,0,0)
        Write_nTRN(cw, 3, 2, Vector3Int.zero);

        // Finalize MAIN chunk
        byte[] body = ms.ToArray();
        fs.Seek(sizePos, SeekOrigin.Begin);
        writer.Write(body.Length);
        fs.Seek(0, SeekOrigin.End);
        writer.Write(body);
    }

    static void Write_SIZE(BinaryWriter bw, int x, int y, int z)
    {
        bw.Write(Encoding.ASCII.GetBytes("SIZE"));
        bw.Write(12); bw.Write(0);
        bw.Write(x); bw.Write(y); bw.Write(z);
    }

    static void Write_XYZI(BinaryWriter bw, Voxel[] voxels)
    {
        int count = voxels.Length;
        bw.Write(Encoding.ASCII.GetBytes("XYZI"));
        bw.Write(4 + count * 4); bw.Write(0);
        bw.Write(count);
        foreach (var v in voxels)
        {
            bw.Write((byte)v.x);
            bw.Write((byte)v.y);
            bw.Write((byte)v.z);
            bw.Write((byte)Mathf.Clamp(v.colorIndex, 1, 255));
        }
    }

    // Trying to get better colors. (Gray colors for now)
    static Color32 AverageColor(List<Color32> colors)
    {
        int r = 0, g = 0, b = 0;
        foreach (var c in colors)
        {
            r += c.r; g += c.g; b += c.b;
        }
        int count = colors.Count;

        // Darken by scaling down RGB values.
        float darkenFactor = 0.85f;

        return new Color32(
            (byte)(r / count * darkenFactor),
            (byte)(g / count * darkenFactor),
            (byte)(b / count * darkenFactor),
            255
            );
    }

    static byte GetClosestPaletteIndex(Color32 input, Color32[] palette)
    {
        byte bestIndex = 1; // Start at 1 (skip transparent index 0)
        int smallestDistance = int.MaxValue;

        for (int i = 1; i < palette.Length; i++)
        {
            var c = palette[i];
            int dr = input.r - c.r;
            int dg = input.g - c.g;
            int db = input.b - c.b;
            int dist = dr * dr + dg * dg + db * db;

            if (dist < smallestDistance)
            {
                smallestDistance = dist;
                bestIndex = (byte)i;
            }
        }

        return bestIndex;
    }

    static Color32[] BuildPalette332()
    {
        var palette = new Color32[256];

        // Slot 0 is empty / transparent
        palette[0] = new Color32(0, 0, 0, 0);

        // Grayscale colors.
        for (int i = 1; i < 256; i++)
        {
            byte shade = (byte)i;
            palette[i] = new Color32(shade, shade, shade, 255);
        }

        return palette;
    }

    static void Write_RGBA(BinaryWriter bw, Color32[] palette)
    {
        bw.Write(Encoding.ASCII.GetBytes("RGBA"));
        bw.Write(1024); bw.Write(0);
        for (int i = 0; i < 256; i++)
        {
            Color32 c = (i < palette.Length)
                ? palette[i]
                : new Color32(0, 0, 0, 0);
            bw.Write(c.r); bw.Write(c.g);
            bw.Write(c.b); bw.Write(c.a);
        }
    }

    static void Write_nSHP(BinaryWriter bw, int id, int modelId)
    {
        var dict = new Dictionary<string, string>();
        byte[] attr = WriteDict(dict);

        bw.Write(Encoding.ASCII.GetBytes("nSHP"));
        using var ms = new MemoryStream();
        using var tmp = new BinaryWriter(ms);

        tmp.Write(id);
        tmp.Write(attr.Length);
        tmp.Write(attr);
        tmp.Write(1);      // one model
        tmp.Write(modelId);
        tmp.Write(0);      // no model attributes

        byte[] chunk = ms.ToArray();
        bw.Write(chunk.Length);
        bw.Write(0);
        bw.Write(chunk);
    }

    static void Write_nTRN(BinaryWriter bw, int id, int childId, Vector3Int pos)
    {
        // node attr = translation
        var node = new Dictionary<string, string> { { "_t", $"{pos.x} {pos.y} {pos.z}" } };
        byte[] nBytes = WriteDict(node);
        byte[] fBytes = WriteDict(node);

        bw.Write(Encoding.ASCII.GetBytes("nTRN"));
        using var ms = new MemoryStream();
        using var tmp = new BinaryWriter(ms);

        tmp.Write(id);
        tmp.Write(nBytes.Length);
        tmp.Write(nBytes);

        tmp.Write(1);       // one child
        tmp.Write(childId);

        tmp.Write(-1);      // reserved layer
        tmp.Write(1);       // one frame

        tmp.Write(fBytes.Length);
        tmp.Write(fBytes);

        byte[] chunk = ms.ToArray();
        bw.Write(chunk.Length);
        bw.Write(0);
        bw.Write(chunk);
    }

    static void Write_nGRP(BinaryWriter bw, int id, int[] children)
    {
        byte[] attr = WriteDict(new Dictionary<string, string>());

        bw.Write(Encoding.ASCII.GetBytes("nGRP"));
        using var ms = new MemoryStream();
        using var tmp = new BinaryWriter(ms);

        tmp.Write(id);
        tmp.Write(attr.Length);
        tmp.Write(attr);
        tmp.Write(children.Length);
        foreach (var c in children) tmp.Write(c);

        byte[] chunk = ms.ToArray();
        bw.Write(chunk.Length);
        bw.Write(0);
        bw.Write(chunk);
    }

    static byte[] WriteDict(Dictionary<string, string> dict)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(dict.Count);
        foreach (var kv in dict)
        {
            var keyBytes = Encoding.UTF8.GetBytes(kv.Key);
            var valueBytes = Encoding.UTF8.GetBytes(kv.Value);
            bw.Write(keyBytes.Length);
            bw.Write(keyBytes);
            bw.Write(valueBytes.Length);
            bw.Write(valueBytes);
        }
        return ms.ToArray();
    }
}
