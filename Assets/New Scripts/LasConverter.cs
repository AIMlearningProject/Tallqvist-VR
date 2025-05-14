using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

// Ajaa las2heightmap.exe ohjelman streaming assets kansiosta. Tehty c++ scriptistä joka muutta las. tiedostoja png "heightmap" muotoon.
// "Lasfiles" kansiossa oleva las tidosto muutetaan heightmapiksi "generated_heightmpa.png" ja sijoitetaan "Heightmaps" kansioon.
// Tiedostopolku: "/AppData/LocalLow/DefaultCopmany/Tallqvist Tyomaa VR/Lasfiles"

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

    // Täytä lista .ls tiedostoilla (+ poisto mahdollisuus)
    public void PopulateLasList()
    {
        // Tyhjennä lista
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Hae kaikki tallennukset aikaleimalla
        string lasFolder = Path.Combine(Application.persistentDataPath, lasDirectoryName);

        if (!Directory.Exists(lasFolder))
        {
            UnityEngine.Debug.LogWarning("LAS folder not found: " + lasFolder);
            return;
        }

        string[] lasFiles = Directory.GetFiles(lasFolder, "*.las");

        foreach (string fullPath in lasFiles)
        {
            string fileName = Path.GetFileName(fullPath);

            //Muunto nappi
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