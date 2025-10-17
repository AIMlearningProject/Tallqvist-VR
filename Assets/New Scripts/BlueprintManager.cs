using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;

public class BlueprintManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buttonPrefab;        // ImportBtn (from prefabs)
    public RectTransform blueprintContentParent; // Scrollview content (BlueprintList)
    public string blueprintDirectoryName = "Blueprints";
    public GameObject blueprintPlane; // Prefabs --> BlueprintPlane

    void Start()
    {
        PopulateBlueprintList();
    }

    // Create the directory for blueprints.
    void Awake()
    {
        blueprintDirectoryName = Path.Combine(Application.persistentDataPath, "Blueprints");
        if (!Directory.Exists(blueprintDirectoryName))
        {
            Directory.CreateDirectory(blueprintDirectoryName);
        }
    }

    public void PopulateBlueprintList()
    {
        // Clear existing buttons.
        foreach (Transform child in blueprintContentParent)
        {
            Destroy(child.gameObject);
        }

        string pictureFolder = Path.Combine(Application.persistentDataPath, blueprintDirectoryName);

        if (!Directory.Exists(pictureFolder))
        {
            Debug.LogWarning($"Blueprints folder not found: {pictureFolder}");
            return;
        }

        string[] pngFiles = Directory.GetFiles(pictureFolder, "*.png");
        string[] PNGFiles = Directory.GetFiles(pictureFolder, "*.PNG");
        string[] jpgFiles = Directory.GetFiles(pictureFolder, "*.jpg");
        string[] jpegFiles = Directory.GetFiles(pictureFolder, "*.jpeg");

        string[] allImages = pngFiles
            .Concat(PNGFiles)
            .Concat(jpgFiles)
            .Concat(jpegFiles)
            .ToArray();

        foreach (string picturePath in allImages)
        {
            // Create a button for each image file.
            string fileName = Path.GetFileName(picturePath);
            GameObject buttonGO = Instantiate(buttonPrefab, blueprintContentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = fileName;

            // Add the click listener.
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                string imagePath = picturePath;
                Texture2D texture = LoadTexture(imagePath);
                if (texture == null)
                {
                    Debug.LogWarning("Failed to load texture: " + imagePath);
                    return;
                }

                GameObject plane = Instantiate(blueprintPlane);

                // Apply texture to the planes material.
                Renderer renderer = plane.GetComponent<Renderer>();
                renderer.material.mainTexture = texture;
                renderer.material.color = new Color(1.15f, 1.15f, 1.15f, 1f); // Brightening the base map.

                // Scale the plane based on image aspect ratio.
                float baseSize = 1f; // Overall size.
                float aspect = (float)texture.width / texture.height;
                plane.transform.localScale = new Vector3(baseSize * aspect, baseSize, 1f);

                // Position the plane in front of the player.
                Transform player = Camera.main.transform;
                Vector3 flatForward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
                Vector3 spawnPos = player.position + flatForward * 2f;
                spawnPos.y = 0.01f;
                plane.transform.position = spawnPos;
                plane.transform.rotation = Quaternion.LookRotation(flatForward, Vector3.up);
            });
        }
    }

    Texture2D LoadTexture(string path)
    {
        if (!File.Exists(path)) return null;

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
            return tex;
        return null;
    }
}