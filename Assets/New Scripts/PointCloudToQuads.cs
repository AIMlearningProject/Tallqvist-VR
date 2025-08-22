using UnityEngine;
using Pcx;

/* RuntimeImporter has problems with colors!!!
 Older editor version used Pcx import .ply with container type ComputeBuffer. 
 Use a PointCloudData made by Pcx to make quads of the points. Quad size can be changed.
 With Quest 2: Works for pointclouds with around 300K points, if rendering both sides of the quad. */

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PointCloudToQuadMesh : MonoBehaviour
{
    public PointCloudData pointCloud;
    public float quadSize = 0.8f;
    public Material quadMaterial;

    public void ConvertToQuads()
    {
        if (pointCloud == null || pointCloud.pointCount == 0)
        {
            Debug.LogWarning("No point cloud data assigned or point count is 0.");
            return;
        }

        // Read data from point cloud.
        var rawData = new Vector4[pointCloud.pointCount];
        pointCloud.computeBuffer.GetData(rawData);
        Debug.Log("First raw buffer point: " + rawData[0]); //debug
        int count = rawData.Length;

        Vector3[] vertices = new Vector3[4 * count];
        Color[] colors = new Color[4 * count];
        int[] indices = new int[6 * count];

        Camera cam = Camera.main;

        for (int i = 0; i < count; i++)
        {
            int vi = i * 4;
            int ti = i * 6;

            Vector3 pos = new Vector3(rawData[i].x, rawData[i].y, rawData[i].z);
            uint encodedColor = System.BitConverter.ToUInt32(System.BitConverter.GetBytes(rawData[i].w), 0);
            Color col = DecodeColor(encodedColor);

            // Quads Face the camera starting point.
            Vector3 toCamera = (cam.transform.position - pos).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, toCamera).normalized * quadSize * 0.5f;
            Vector3 up = Vector3.Cross(toCamera, right).normalized * quadSize * 0.5f;

            // Quad vertices around point.
            vertices[vi + 0] = pos - right - up;
            vertices[vi + 1] = pos + right - up;
            vertices[vi + 2] = pos + right + up;
            vertices[vi + 3] = pos - right + up;

            // Assign same color to all quad vertices.
            colors[vi + 0] = col;
            colors[vi + 1] = col;
            colors[vi + 2] = col;
            colors[vi + 3] = col;

            // Define two triangles per quad.
            indices[ti + 0] = vi + 0;
            indices[ti + 1] = vi + 1;
            indices[ti + 2] = vi + 2;
            indices[ti + 3] = vi + 2;
            indices[ti + 4] = vi + 3;
            indices[ti + 5] = vi + 0;

            if (i < 5)
                Debug.Log($"Point {i}: pos={pos} encodedColor=0x{encodedColor:X8} decoded={col}");
        }

        // Build mesh.
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().mesh = mesh;

        var meshRenderer = GetComponent<MeshRenderer>();
        if (quadMaterial != null)
            meshRenderer.sharedMaterial = quadMaterial;
    }

    Color DecodeColor(uint rgba)
    {
        const float kMaxBrightness = 16.0f;

        float r = (rgba >> 0) & 0xFF;
        float g = (rgba >> 8) & 0xFF;
        float b = (rgba >> 16) & 0xFF;
        float y = (rgba >> 24) & 0xFF;

        if (y == 0) return Color.black;

        float scale = (y * kMaxBrightness) / (255.0f * 255.0f);

        return new Color(r * scale, g * scale, b * scale, 1.0f);
    }
}