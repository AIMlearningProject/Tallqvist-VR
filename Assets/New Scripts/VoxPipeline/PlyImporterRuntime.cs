using Pcx;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;

public static class PlyImporterRuntime
{
    public class PlyProperty
    {
        public string type;
        public string name;
    }

    public class PlyHeader
    {
        public int vertexCount;
        public List<PlyProperty> properties = new List<PlyProperty>();
        public bool isBinary = false;
    }

    public class PlyBody
    {
        public Vector3[] vertices;
        public Color32[] colors;
    }

    public static PlyHeader ReadHeader(Stream stream, out long headerByteCount)
    {
        var header = new PlyHeader();
        headerByteCount = 0;
        bool inVertex = false;

        // Wrap in BinaryReader to see every byte.
        using var br = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        var lineBytes = new List<byte>();

        while (true)
        {
            byte b = br.ReadByte();
            headerByteCount++;
            lineBytes.Add(b);

            if (b != '\n')
                continue;

            var line = Encoding.ASCII
                .GetString(lineBytes.ToArray())
                .TrimEnd('\r', '\n');
            lineBytes.Clear();

            if (line == "end_header")
                break;

            var parts = line.Split();
            switch (parts[0])
            {
                case "format":
                    header.isBinary = parts[1]
                        .StartsWith("binary_little_endian");
                    break;

                case "element":
                    inVertex = parts[1] == "vertex";
                    if (inVertex)
                        header.vertexCount = int.Parse(parts[2]);
                    break;

                case "property" when inVertex:
                    header.properties.Add(new PlyProperty
                    {
                        type = parts[1],
                        name = parts[2]
                    });
                    break;
            }
        }

        return header;
    }

    public static PlyBody ReadBody(PlyHeader header, BinaryReader reader)
    {
        var body = new PlyBody
        {
            vertices = new Vector3[header.vertexCount],
            colors = new Color32[header.vertexCount]
        };

        for (int i = 0; i < header.vertexCount; i++)
        {
            float x = 0, y = 0, z = 0;
            byte r = 255, g = 255, b = 255;

            foreach (var prop in header.properties)
            {
                switch (prop.name)
                {
                    case "x": x = ReadFloat(prop.type, reader); break;
                    case "y": y = ReadFloat(prop.type, reader); break;
                    case "z": z = ReadFloat(prop.type, reader); break;
                    case "red": r = ReadByte(prop.type, reader); break;
                    case "green": g = ReadByte(prop.type, reader); break;
                    case "blue": b = ReadByte(prop.type, reader); break;
                    default:
                        SkipProperty(prop.type, reader);
                        break;
                }
            }

            if (i < 5)
                Debug.Log($"[Debug] Vertex {i}: x={x}, y={y}, z={z}, color=({r},{g},{b})");

            body.vertices[i] = new Vector3(x, y, z);
            body.colors[i] = new Color32(r, g, b, 255);
        }

        return body;
    }

    private static float ReadFloat(string type, BinaryReader reader)
    {
        return type switch
        {
            "float" => reader.ReadSingle(),
            "double" => (float)reader.ReadDouble(),
            _ => throw new Exception($"Unsupported float type: {type}")
        };
    }

    private static byte ReadByte(string type, BinaryReader reader)
    {
        return type switch
        {
            "uchar" => reader.ReadByte(),
            "char" => (byte)reader.ReadSByte(),
            "ushort" => (byte)reader.ReadUInt16(),
            "short" => (byte)reader.ReadInt16(),
            "uint" => (byte)reader.ReadUInt32(),
            "int" => (byte)reader.ReadInt32(),
            _ => throw new Exception($"Unsupported byte type: {type}")
        };
    }

    private static void SkipProperty(string type, BinaryReader reader)
    {
        switch (type)
        {
            case "char":   //  1 byte
            case "uchar":
            case "int8":
            case "uint8":
                reader.BaseStream.Position += 1;
                break;

            case "short":  //  2 bytes
            case "ushort":
            case "int16":
            case "uint16":
                reader.BaseStream.Position += 2;
                break;

            case "int":    //  4 bytes
            case "uint":
            case "float":
            case "int32":
            case "uint32":
            case "float32":
                reader.BaseStream.Position += 4;
                break;

            case "int64":  //  8 bytes
            case "uint64":
            case "double":
            case "float64":
                reader.BaseStream.Position += 8;
                break;

            default:
                throw new ArgumentException(
                    $"Unknown skip type '{type}'");
        }
    }

    public static PointCloudData Load(string filePath)
    {
        using var fs = File.OpenRead(filePath);

        // Parse header, get its length in bytes
        long headerByteCount;
        var header = ReadHeader(fs, out headerByteCount);

        if (!header.isBinary)
            throw new NotSupportedException("Only binary_little_endian PLY is supported.");

        // Seek to the first data byte
        fs.Seek(headerByteCount, SeekOrigin.Begin);

        // Read the binary body
        using var br = new BinaryReader(fs, Encoding.ASCII, leaveOpen: true);
        var body = ReadBody(header, br);

        // Create the ScriptableObject
        var data = ScriptableObject.CreateInstance<PointCloudData>();
        data.SetPointData(body.vertices, body.colors); // Testing with different color approaches.
        data.name = Path.GetFileNameWithoutExtension(filePath);
        return data;
    }
}
