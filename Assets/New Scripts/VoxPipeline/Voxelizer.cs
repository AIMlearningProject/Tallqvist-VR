using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Voxelizer
{
    public static VoxData CreateVoxData(List<PlyVertex> vertices, float voxelSize)
    {
        if (vertices == null || vertices.Count == 0)
        {
            Debug.LogError("Vertex list is null or empty.");
            return null;
        }
        if (voxelSize <= 0f)
        {
            Debug.LogError("Voxel size must be greater than 0.");
            return null;
        }

        // Compute bounds
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        foreach (var v in vertices)
            if (IsFinite(v.position))
            {
                min = Vector3.Min(min, v.position);
                max = Vector3.Max(max, v.position);
            }

        Vector3 dims = max - min;
        int sizeX = Mathf.Clamp(Mathf.CeilToInt(dims.x / voxelSize), 1, 256);
        int sizeY = Mathf.Clamp(Mathf.CeilToInt(dims.y / voxelSize), 1, 256);
        int sizeZ = Mathf.Clamp(Mathf.CeilToInt(dims.z / voxelSize), 1, 256);
        Debug.Log($"[Voxelizer] Bounds: Min={min}, Max={max}, GridSize=({sizeX},{sizeY},{sizeZ})");

        // Set up maps and palette
        var voxelMap = new Dictionary<Vector3Int, byte>();
        var colorToIndex = new Dictionary<Color32, byte>();
        var palette = new Color32[256];
        palette[0] = new Color32(0, 0, 0, 0);

        byte nextIndex = 1;
        int skippedInvalid = 0;
        int skippedOut = 0;

        // Populate voxels with colorIndex > 0
        foreach (var v in vertices)
        {
            if (!IsFinite(v.position))
            {
                skippedInvalid++;
                continue;
            }
            if (v.color.a == 0)
                continue;

            Vector3Int grid = Vector3Int.FloorToInt((v.position - min) / voxelSize);

            if (grid.x < 0 || grid.y < 0 || grid.z < 0 ||
                grid.x >= sizeX || grid.y >= sizeY || grid.z >= sizeZ)
            {
                skippedOut++;
                continue;
            }

            Color32 QuantizeColor(Color32 c)
            {
                // Round to 64-color grid (4 bits per channel)
                byte r = (byte)((c.r >> 4) << 4);
                byte g = (byte)((c.g >> 4) << 4);
                byte b = (byte)((c.b >> 4) << 4);
                return new Color32(r, g, b, 255);
            }

            var quantized = QuantizeColor(v.color);
            if (!colorToIndex.TryGetValue(quantized, out byte colorIndex))
            {
                if (nextIndex < 256)
                {
                    colorIndex = nextIndex;
                    colorToIndex[v.color] = colorIndex;
                    palette[nextIndex] = v.color;
                    nextIndex++;
                }
                else
                {
                    Debug.LogWarning($"Palette full—dropping color {v.color}");
                    continue;
                }
            }

            voxelMap[grid] = colorIndex;

            if (voxelMap.Count <= 5)
                Debug.Log($"[Voxelizer] {v.position:F2} → {grid} → idx {colorIndex}");
        }

        if (voxelMap.Count == 0)
        {
            Debug.LogError("No voxels generated.");
            return null;
        }
        if (skippedInvalid > 0)
            Debug.LogWarning($"Skipped {skippedInvalid} invalid vertices.");
        if (skippedOut > 0)
            Debug.LogWarning($"Skipped {skippedOut} voxels out of bounds.");

        // Convert to flat array
        var voxels = voxelMap
    .Where(kvp => kvp.Value != 0)
    .Select(kvp => {
        var color = palette[kvp.Value];
        return new Voxel
        {
            x = kvp.Key.x,
            y = kvp.Key.y,
            z = kvp.Key.z,
            r = color.r,
            g = color.g,
            b = color.b,
            colorIndex = kvp.Value
        };
    })
    .ToArray();

        Debug.Log($"Voxelized {voxels.Length} voxels using {nextIndex - 1} colors.");

        foreach (var v in voxels.Take(5))
            Debug.Log($"Voxel: ({v.x}, {v.y}, {v.z}) → idx {v.colorIndex}");

        return new VoxData(voxels, palette, sizeX, sizeY, sizeZ);
    }

    static bool IsFinite(Vector3 v) =>
        float.IsFinite(v.x) && float.IsFinite(v.y) && float.IsFinite(v.z);
}
