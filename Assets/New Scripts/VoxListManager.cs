using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Pcx;

public class VoxListManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentParent;    // ScrollView content (VoxelList)
    public GameObject buttonPrefab;        // Prefab Button (importButton)

    public RectTransform plyContentParent; // Scrollview content (PlyList)
    public string plyDirectoryName = "PlyFiles";

    [Header("Vox Settings")]
    public string voxDirectoryName = "Exports";

    void Start()
    {
        PopulateVoxList();
        PopulatePlyList();
    }

    public void PopulateVoxList()
    {
        // Clear existing buttons
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Build folder path
        string voxFolder = Path.Combine(Application.persistentDataPath, voxDirectoryName);

        if (!Directory.Exists(voxFolder))
        {
            Debug.LogWarning($"Vox folder not found: {voxFolder}");
            return;
        }

        // Find .vox files
        string[] files = Directory.GetFiles(voxFolder, "*.vox");

        // Instantiate a button per file
        foreach (string fullPath in files)
        {
            string fileName = Path.GetFileName(fullPath);

            var buttonGO = Instantiate(buttonPrefab, contentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = fileName;

            // Capture the path for the listener
            string selectedPath = fullPath;
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                LoadVoxel(selectedPath);
            });
        }
    }

    public void PopulatePlyList()
    {
        // Clear existing buttons
        foreach (Transform child in plyContentParent)
        {
            Destroy(child.gameObject);
        }

        string plyFolder = Path.Combine(Application.persistentDataPath, plyDirectoryName);

        if (!Directory.Exists(plyFolder))
        {
            Debug.LogWarning($"PLY folder not found: {plyFolder}");
            return;
        }

        string[] plyPaths = Directory.GetFiles(plyFolder, "*.ply");

        foreach (string plyPath in plyPaths)
        {
            // Create a button for each .ply file
            string fileName = Path.GetFileName(plyPath);
            GameObject buttonGO = Instantiate(buttonPrefab, plyContentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = fileName;

            // Add the click listener
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                // Create a temporary GameObject
                GameObject tempGO = new GameObject("PointCloudConverter");

                // Add required components
                var renderer = tempGO.AddComponent<PointCloudRenderer>();
                var converter = tempGO.AddComponent<ComputeBufferToVox>();

                // Load .ply data into the PointCloudRenderer
                renderer.sourceData = PlyImporterRuntime.Load(plyPath);

                // Start the conversion
                converter.ConvertToVoxels();

                // Update the VoxList
                PopulateVoxList();
            });
        }
    }

    // For calling the Cubizer
    void LoadVoxel(string path)
    {
        // Call VOX loader here!
        // VoxImporter.Load(path);
        Debug.Log($"Loading VOX: {path}");
    }
}
