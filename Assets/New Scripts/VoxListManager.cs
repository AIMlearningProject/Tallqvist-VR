using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Pcx;
using Cubizer.Model;
using System.Linq;

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
        PopulatePlyList();
        PopulateVoxList();
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
                var converter = tempGO.AddComponent<ConvertToVox>();

                // Load .ply data into the PointCloudRenderer
                renderer.sourceData = PlyImporterRuntime.Load(plyPath);

                // Start the conversion
                converter.ConvertToVoxels();

                // Update the VoxList
                PopulateVoxList();
            });
        }
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
                LoadVoxel(selectedPath, 3); // LOD level 3
            });
        }
    }

    //Call Cubizer, and after that add needed attributes and change values.
    [SerializeField] private Transform voxelParent; // VoxelParent
    [SerializeField] private Material voxelMaterial; // Custom shader: Unlit_VertexColorURP

    void LoadVoxel(string path, int lodLevel = 3)
    {
        // Clear previous voxel models
        foreach (Transform child in voxelParent)
        {
            Destroy(child.gameObject);
        }

        // Load the voxel model with LOD
        GameObject voxelGO = VoxFileImport.LoadVoxelFileAsGameObjectLOD(path, lodLevel);

        if (voxelGO == null)
        {
            Debug.LogError($"Failed to load voxel model from: {path}");
            return;
        }

        // Parent it under voxelParent
        voxelGO.transform.SetParent(voxelParent, false);

        // Apply custom scale and position
        voxelGO.transform.localScale = Vector3.one * 0.3f; // Might need down scaling
        voxelGO.transform.localPosition = Vector3.zero;

        // Apply custom shader to all LOD meshes
        var meshRenderers = voxelGO.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in meshRenderers)
        {
            renderer.material = voxelMaterial;
        }

        // Disable LOD Group
        var lodGroup = voxelGO.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            lodGroup.enabled = false;
        }

        // Add mesh collider to the third LOD
        if (lodGroup != null)
        {
            lodGroup.enabled = false;

            var lods = lodGroup.GetLODs();
            if (lods.Length >= 3)
            {
                var thirdLODRenderer = lods[2].renderers.FirstOrDefault();
                if (thirdLODRenderer != null)
                {
                    var meshFilter = thirdLODRenderer.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        var colliderGO = thirdLODRenderer.gameObject;
                        var meshCollider = colliderGO.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = meshFilter.sharedMesh;
                        meshCollider.convex = false; // Set to true for dynamic physics
                        meshCollider.providesContacts = true;
                        Debug.Log("MeshCollider added to third LOD.");
                    }
                }
            }
        }

        Debug.Log($"Loaded and customized voxel model: {path}");
    }
}
