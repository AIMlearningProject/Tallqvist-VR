using Pcx;
using UnityEngine;
using System.IO;

public static class PlyImporterRuntime
{
    public static PointCloudData Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"PLY file not found: {filePath}");
            return null;
        }

        try
        {
            var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var header = PlyImporter.ReadDataHeader(new StreamReader(stream));
            var body = PlyImporter.ReadDataBody(header, new BinaryReader(stream));

            var data = ScriptableObject.CreateInstance<PointCloudData>();
            data.Initialize(body.vertices, body.colors);
            data.name = Path.GetFileNameWithoutExtension(filePath);
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading PLY: {filePath} \n{e.Message}");
            return null;
        }
    }
}
