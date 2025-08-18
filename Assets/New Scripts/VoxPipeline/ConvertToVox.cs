using Cubizer.Model;
using Pcx;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PointCloudRenderer))]
public class ConvertToVox : MonoBehaviour
{
    private PointCloudData pointCloud;
    public float voxelSize = 2f;

    [Header("Export Settings")]
    private string fileName;
    private string timestamp;

    public void ConvertToVoxels()
    {
        // Get the PCX renderer component.
        var pcxRenderer = GetComponent<PointCloudRenderer>();
        if (pcxRenderer == null || pcxRenderer.sourceData == null)
        {
            Debug.LogError("Missing PointCloudRenderer or no data assigned.");
            enabled = false;
            return;
        }

        // Pull out the raw PointCloudData, and then start the coroutine.
        pointCloud = pcxRenderer.sourceData;
        StartCoroutine(ProcessPointCloud());
    }

    IEnumerator ProcessPointCloud()
    {
        // Wait until the computeBuffer is ready.
        while (pointCloud == null || pointCloud.computeBuffer == null || pointCloud.pointCount == 0)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log("Waiting for PointCloudData to initialize...");
            yield return null;
        }

        Debug.Log($"PointCloud loaded with {pointCloud.pointCount} points");

        var pointData = new PointCloudPoint[pointCloud.pointCount];
        pointCloud.computeBuffer.GetData(pointData);

        List<PlyVertex> vertices = new();
        for (int i = 0; i < pointData.Length; i++)
        {
            Vector3 pos = new Vector3(pointData[i].x, pointData[i].y, pointData[i].z);
            Color32 col = new Color32(pointData[i].r, pointData[i].g, pointData[i].b, 255);

            vertices.Add(new PlyVertex { position = pos, color = col });

            if (i < 5)
                Debug.Log($"Point {i}: pos={pos}, color={col}");
        }

        // Voxelize.
        VoxData voxData = Voxelizer.CreateVoxData(vertices, voxelSize);
        if (voxData == null || voxData.voxels.Length == 0)
        {
            Debug.LogError("No voxels generated.");
            yield break;
        }

        Debug.Log($"Voxelized: {voxData.voxels.Length} voxels");

        // Export.
        Vector3Int bounds = ComputeModelBounds(voxData.voxels.ToList());

        timestamp = DateTime.Now.ToString("dd_HHmmss");
        fileName = $"voxel_{timestamp}.vox";

        string folder = Path.Combine(Application.persistentDataPath, "VoxExports");
        Directory.CreateDirectory(folder);
        string exportPath = Path.Combine(folder, fileName);
        VoxWriter.Save(exportPath, voxData);

        Debug.Log($"Exported to: {exportPath}");
    }

    Vector3Int ComputeModelBounds(List<Voxel> voxels)
    {
        int maxX = 0, maxY = 0, maxZ = 0;
        foreach (var v in voxels)
        {
            if (v.x > maxX) maxX = v.x;
            if (v.y > maxY) maxY = v.y;
            if (v.z > maxZ) maxZ = v.z;
        }
        return new Vector3Int(maxX + 1, maxY + 1, maxZ + 1);
    }
}

public struct PlyVertex
{
    public Vector3 position;
    public Color32 color;
}

struct PointCloudPoint
{
    public float x;
    public float y;
    public float z;
    public byte r;
    public byte g;
    public byte b;
}