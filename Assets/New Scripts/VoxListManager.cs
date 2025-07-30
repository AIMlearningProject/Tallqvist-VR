using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class VoxListManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentParent;    // ScrollView content panel
    public GameObject buttonPrefab;        // Prefab Button

    [Header("Vox Settings")]
    public string voxDirectoryName = "Exports";

    void Start()
    {
        PopulateVoxList();
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

    void LoadVoxel(string path)
    {
        // Call VOX loader here!
        // VoxImporter.Load(path);
        Debug.Log($"Loading VOX: {path}");
    }
}
