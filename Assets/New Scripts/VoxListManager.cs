using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using Pcx;
using Cubizer.Model;

/* Populates a list of point clouds from which the desired point cloud can be converted to Voxels.
Also populates another list with the converted Voxels from which the Voxels can be loaded. */

public class VoxListManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentParent;    // ScrollView content (VoxelList)
    public GameObject buttonPrefab;        // ImportBtn

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
        // Clear existing buttons.
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
                var renderer = tempGO.AddComponent<PointCloudRenderer>();
                var converter = tempGO.AddComponent<ConvertToVox>();

                // Load .ply data into the PointCloudRenderer
                var data = PlyImporterRuntime.Load(plyPath);
                if (data == null)
                {
                    Debug.LogError("Failed to load point cloud data.");
                    Destroy(tempGO);
                    return;
                }
                renderer.sourceData = data;
                converter.ConvertToVoxels();
                PopulateVoxList();
            });
        }
    }

    public void PopulateVoxList()
    {
        // Clear existing buttons.
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Build folder path.
        string voxFolder = Path.Combine(Application.persistentDataPath, voxDirectoryName);

        if (!Directory.Exists(voxFolder))
        {
            Debug.LogWarning($"Vox folder not found: {voxFolder}");
            return;
        }

        // Find .vox files.
        string[] files = Directory.GetFiles(voxFolder, "*.vox");

        // Instantiate a button per file.
        foreach (string fullPath in files)
        {
            string fileName = Path.GetFileName(fullPath);

            var buttonGO = Instantiate(buttonPrefab, contentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = fileName;

            // Capture the path for the listener.
            string selectedPath = fullPath;
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                LoadVoxel(selectedPath, 3); // LOD level 3
            });
        }
    }

    [SerializeField] private Transform voxelParent; // VoxelParent
    [SerializeField] private Material voxelMaterial; // uusin (Point Cloud/Point)

    void LoadVoxel(string path, int lodLevel = 3)
    {
        // Clear previous voxel models.
        foreach (Transform child in voxelParent)
        {
            Destroy(child.gameObject);
        }

        // Load the voxel model with LOD.
        GameObject voxelGO = VoxFileImport.LoadVoxelFileAsGameObjectLOD(path, lodLevel);

        if (voxelGO == null)
        {
            Debug.LogError($"Failed to load voxel model from: {path}");
            return;
        }

        // Parent it under voxelParent.
        voxelGO.transform.SetParent(voxelParent, false);

        // Apply custom scale and position.
        voxelGO.transform.localScale = Vector3.one * 1f; // Might need down scaling
        voxelGO.transform.localPosition = Vector3.zero;

        // Fallback material.
        Material materialToUse = voxelMaterial;

        if (materialToUse == null || materialToUse.shader == null)
        {
            Debug.LogWarning("[VOX] voxelMaterial is missing or invalid. Creating fallback blue material...");

            Shader fallbackShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (fallbackShader == null)
            {
                Debug.LogError("[VOX] Fallback shader not found. Model may render pink.");
            }
            else
            {
                materialToUse = new Material(fallbackShader);
                materialToUse.name = "FallbackBlueMaterial";
                materialToUse.color = Color.blue;
                Debug.Log("[VOX] Fallback blue material created.");
            }
        }

        // Apply material to all renderers.
        var meshRenderers = voxelGO.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in meshRenderers)
        {
            renderer.sharedMaterial = materialToUse;
        }

        // Disable LOD Group.
        var lodGroup = voxelGO.GetComponent<LODGroup>();
        if (lodGroup != null)
        {
            lodGroup.enabled = false;

            var lods = lodGroup.GetLODs();

            // Disable LODs 0 and 1 by clearing their renderers.
            if (lods.Length >= 2)
            {
                lods[0].renderers = new Renderer[0];
                lods[1].renderers = new Renderer[0];
                lodGroup.SetLODs(lods);
            }
        }

        // Add mesh collider to the third LOD.
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
                        meshCollider.convex = false;
                        meshCollider.providesContacts = true;
                        Debug.Log("MeshCollider added to third LOD.");
                    }
                }
            }
        }

        Debug.Log($"Loaded and customized voxel model: {path}");
    }
}
