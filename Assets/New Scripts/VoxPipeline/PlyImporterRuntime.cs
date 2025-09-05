using Pcx;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Text;

// EDITOR TOOLS FOR RUNTIME USE!
// 2 Versions, for actual runtime, and for "editor runtime".
// UNCOMMENT a line at the end of this script AND inside PointCloudData according to the chosen use of the tools.

// "editor runtime" version cannot be built to a app.
// Actual buildable runtime version is not working 100%, atleast colors are bad. (wrong colors in a wrong order).

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

    public static void ReadBody(PlyHeader header, BinaryReader reader,
    out List<Vector3> positions, out List<Color32> colors)
    {
        positions = new List<Vector3>(header.vertexCount);
        colors = new List<Color32>(header.vertexCount);

        for (int i = 0; i < header.vertexCount; i++)
        {
            float x = 0, y = 0, z = 0;
            byte r = 255, g = 255, b = 255, a = 255;
            byte intensity = 255;

            foreach (var prop in header.properties)
            {
                switch (prop.name)
                {
                    // Positions
                    case "x": x = ReadFloat(prop.type, reader); break;
                    case "y": y = ReadFloat(prop.type, reader); break;
                    case "z": z = ReadFloat(prop.type, reader); break;

                    // Colors
                    case "red":
                    case "diffuse_red": r = ReadColorComponent(prop.type, reader); break;
                    case "green":
                    case "diffuse_green": g = ReadColorComponent(prop.type, reader); break;
                    case "blue":
                    case "diffuse_blue": b = ReadColorComponent(prop.type, reader); break;

                    // Opacity/alpha
                    case "alpha":
                    case "opacity": a = ReadColorComponent(prop.type, reader); break;

                    // Brightness/intensity
                    case "intensity": intensity = ReadColorComponent(prop.type, reader); break;

                    default:
                        SkipProperty(prop.type, reader);
                        break;
                }
            }
            positions.Add(new Vector3(x, y, z));
            colors.Add(new Color32(r, g, b, a));
        }
    }

    private static byte ReadColorComponent(string type, BinaryReader reader)
    {
        return type switch
        {
            "float" => (byte)Mathf.Clamp(reader.ReadSingle() * 255f, 0, 255),
            "double" => (byte)Mathf.Clamp((float)reader.ReadDouble() * 255f, 0, 255),
            _ => ReadByte(type, reader)
        };
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

        // Parse header and get its length in bytes.
        long headerByteCount;
        var header = ReadHeader(fs, out headerByteCount);

        if (!header.isBinary)
            throw new NotSupportedException("Only binary_little_endian PLY is supported.");

        // Seek to the first data byte.
        fs.Seek(headerByteCount, SeekOrigin.Begin);

        // Read the binary body.
        using var br = new BinaryReader(fs, System.Text.Encoding.ASCII, leaveOpen: true);

        // Prepare lists for positions and colors.
        var positions = new List<Vector3>(header.vertexCount);
        var colors = new List<Color32>(header.vertexCount);

        // Read every vertex
        for (int i = 0; i < header.vertexCount; i++)
        {
            float px = 0, py = 0, pz = 0;
            byte r = 255, g = 255, b = 255;
            byte intensity = 255;  // Will become alpha.

            // Pull in each PLY property.
            foreach (var prop in header.properties)
            {
                switch (prop.name)
                {
                    // Position
                    case "x": px = ReadFloat(prop.type, br); break;
                    case "y": py = ReadFloat(prop.type, br); break;
                    case "z": pz = ReadFloat(prop.type, br); break;

                    // Raw colour channels
                    case "red":
                    case "diffuse_red": r = ReadColorComponent(prop.type, br); break;
                    case "green":
                    case "diffuse_green": g = ReadColorComponent(prop.type, br); break;
                    case "blue":
                    case "diffuse_blue": b = ReadColorComponent(prop.type, br); break;

                    // Opacity
                    case "alpha":
                    case "opacity": br.ReadByte(); /*discard*/           break;

                    // Brightness / intensity → alpha.
                    case "intensity": intensity = ReadColorComponent(prop.type, br); break;

                    // Anything else, skip its bytes.
                    default:
                        SkipProperty(prop.type, br);
                        break;
                }
            }

            // Store raw data: r,g,b are colours, alpha= intensity.
            positions.Add(new Vector3(px, py, pz));
            colors.Add(new Color32(r, g, b, intensity));
        }

        // Debug first few entries to confirm the raw RGBA.
        for (int i = 0; i < Mathf.Min(5, colors.Count); i++)
        {
            var c = colors[i];
            Debug.Log($"[PLY→PCX Debug] #{i} pos={positions[i]}  RGBA=({c.r},{c.g},{c.b},{c.a})");
        }

        // Create the PointCloudData asset and pack it.
        var data = ScriptableObject.CreateInstance<PointCloudData>();
        //data.SetPointData(positions.ToArray(), colors.ToArray());                             // UNCOMMENT FOR ACTUAL RUNTIME TOOL USE!
        //data.InitializeRuntime(positions, colors);                                            // UNCOMMENT FOR EDITOR USE OF "RUNTIME" TOOLS!
        data.name = Path.GetFileNameWithoutExtension(filePath);
        return data;
    }
}
