using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Pcx;

/* Populates a list of point clouds from which the 
 PointCloudToQuads function can be called with the desired point cloud. */

public class QuadManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buttonPrefab;        // ImportBtn (from UIElements)
    public RectTransform quadContentParent; // Scrollview content (QuadPanel, PlyList)
    public string plyDirectoryName = "PlyFiles";
    private GameObject currentRenderGO;
    [SerializeField] private Material quadMaterial; // QuadMaterial

    void Start()
    {
        PopulateQuadList();
    }

    public void PopulateQuadList()
    {
        // Clear existing buttons.
        foreach (Transform child in quadContentParent)
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
            // Create a button for each .ply file.
            string fileName = Path.GetFileName(plyPath);
            GameObject buttonGO = Instantiate(buttonPrefab, quadContentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = fileName;

            // Add the click listener.
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                // Destroy previous render if it exists.
                if (currentRenderGO != null)
                {
                    Destroy(currentRenderGO);
                    currentRenderGO = null;
                }

                // Create a new GameObject for the point cloud.
                currentRenderGO = new GameObject("PointCloudConverter");

                // Set custom transform.
                currentRenderGO.transform.position = new Vector3(-700f, -5f, 50f);
                currentRenderGO.transform.rotation = Quaternion.Euler(-90f, 0f, -180f);
                currentRenderGO.transform.localScale = new Vector3(-1f, 1f, 1f);

                var renderer = currentRenderGO.AddComponent<PointCloudRenderer>();

                // Load .ply data into the PointCloudRenderer.
                var data = PlyImporterRuntime.Load(plyPath);
                if (data == null)
                {
                    Debug.LogError("Failed to load point cloud data.");
                    Destroy(currentRenderGO);
                    currentRenderGO = null;
                    return;
                }

                renderer.sourceData = data;

                var quadMesh = currentRenderGO.AddComponent<PointCloudToQuadMesh>();
                quadMesh.pointCloud = data;
                quadMesh.quadMaterial = quadMaterial;
                quadMesh.ConvertToQuads();

                // Add collider. (Not very practical at the moment.)
                /*var mf = currentRenderGO.GetComponent<MeshFilter>();
                if (mf != null && mf.mesh != null)
                {
                    var col = currentRenderGO.AddComponent<MeshCollider>();
                    col.sharedMesh = mf.mesh;
                    col.convex = false;
                }*/
            });
        }
    }
}
