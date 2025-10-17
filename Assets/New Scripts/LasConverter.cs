using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;

// Run las2heighgtmap.exe from streaming assets. It is a c++ script for converting .las files to grayscale .png "heightmaps".
// Filepath: "/AppData/LocalLow/DefaultCopmany/Tallqvist Tyomaa VR/Lasfiles"

public class LasConverter : MonoBehaviour
{
    public GameObject buttonPrefab; // ScrollBtn
    public Transform contentParent; // ScrollView --> ViewPort --> Content
    public string lasDirectoryName = "Lasfiles";

    public string batRelativePath = "las2heightmap/run_las2heightmap.bat";

    private string batFullPath;
    private string lasFilePath;
    private string outputPngPath;
    private string basePath;

    void Start()
    {
        basePath = Application.persistentDataPath;

        batFullPath = Path.Combine(Application.streamingAssetsPath, batRelativePath);
        lasFilePath = Path.Combine(basePath, "Lasfiles", "input.las");
        //outputPngPath = Path.Combine(basePath, "Heightmaps", "generated_heightmap.png");

        string lasFolder = Path.Combine(basePath, "Lasfiles");
        Directory.CreateDirectory(lasFolder);

        string[] lasFiles = Directory.GetFiles(lasFolder, "*.las");
        if (lasFiles.Length == 0)
        {
            UnityEngine.Debug.LogWarning("No LAS files found in: " + lasFolder);
            return;
        }
        lasFilePath = lasFiles[0];

        PopulateLasList();
    }

    void RunLasToHeightmap(string lasFilePath)
    {
        if (string.IsNullOrEmpty(basePath))
            basePath = Application.persistentDataPath;

        string lasFileNameWithoutExt = Path.GetFileNameWithoutExtension(lasFilePath);
        string outputFileName = $"heightmap_{lasFileNameWithoutExt}.png";
        string outputPngPath = Path.Combine(basePath, "Heightmaps", outputFileName);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = batFullPath,
            Arguments = $"--input \"{lasFilePath}\" --output \"{outputPngPath}\"",
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

    // Populate list with .las files.
    public void PopulateLasList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        string lasFolder = Path.Combine(Application.persistentDataPath, lasDirectoryName);

        if (!Directory.Exists(lasFolder))
        {
            UnityEngine.Debug.LogWarning("LAS folder not found: " + lasFolder);
            return;
        }

        string[] lasFiles = Directory.GetFiles(lasFolder, "*.las");
        string[] lazFiles = Directory.GetFiles(lasFolder, "*.laz");
        string[] files = lasFiles.Concat(lazFiles).ToArray();

        foreach (string fullPath in files)
        {
            string fileName = Path.GetFileName(fullPath);

            // Button for converting the file.
            GameObject buttonGO = Instantiate(buttonPrefab, contentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = fileName;

            string selectedPath = fullPath;
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                RunLasToHeightmap(selectedPath);
            });
        }
    }
}