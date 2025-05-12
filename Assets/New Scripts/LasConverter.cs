using System.Diagnostics;
using System.IO;
using UnityEngine;
using System;

// Ajaa las2heightmap.exe ohjelman streaming assets kansiosta. Tehty c++ scriptistä joka muutta las. tiedostoja png "heightmap" muotoon.
// "Lasfiles" kansiossa oleva las tidosto muutetaan heightmapiksi "generated_heightmpa.png" ja sijoitetaan "Heightmaps" kansioon.
// Tiedostopolku: "/AppData/LocalLow/DefaultCopmany/Tallqvist Tyomaa VR/Lasfiles"

public class LasConverter : MonoBehaviour
{
    public string batRelativePath = "las2heightmap/run_las2heightmap.bat";

    private string batFullPath;
    private string inputLasPath;
    private string outputPngPath;
    private string basePath;

    void Start()
    {
        basePath = Application.persistentDataPath;

        batFullPath = Path.Combine(Application.streamingAssetsPath, batRelativePath);
        inputLasPath = Path.Combine(basePath, "Lasfiles", "input.las");
        outputPngPath = Path.Combine(basePath, "Heightmaps", "generated_heightmap.png");

        string lasFolder = Path.Combine(basePath, "Lasfiles");
        Directory.CreateDirectory(lasFolder);

        string[] lasFiles = Directory.GetFiles(lasFolder, "*.las");
        if (lasFiles.Length == 0)
        {
            UnityEngine.Debug.LogWarning("No LAS files found in: " + lasFolder);
            return;
        }
        inputLasPath = lasFiles[0];
    }

    void RunLasToHeightmap()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = batFullPath,
            Arguments = $"--input \"{inputLasPath}\" --output \"{outputPngPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            Process process = Process.Start(startInfo);
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    UnityEngine.Debug.Log("[las2heightmap INFO] " + e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (e.Data.ToLower().Contains("error") || e.Data.ToLower().Contains("failed"))
                    {
                        UnityEngine.Debug.LogError("[las2heightmap ERROR] " + e.Data);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("[las2heightmap STDERR] " + e.Data);
                    }
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();

            UnityEngine.Debug.Log($"Heightmap generated at {outputPngPath}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to run las2heightmap.bat: " + e.Message);
        }
    }

    // Menun nappia varten
    public void ConvertLas()
    {
        RunLasToHeightmap();
    }
}