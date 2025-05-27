using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

//Aseta heighmap.png terrainin heightmapiksi

public class ImportHeightMap : MonoBehaviour
{
    public GameObject itemPrefab; // ImportBtn
    public Transform contentParent; // Scroll --> Content
    public Terrain targetTerrain; // Terrain
    public GameObject player; // XR Origin (XR Rig)

    private string heightmapFolder;

    void Start()
    {
        heightmapFolder = Path.Combine(Application.persistentDataPath, "Heightmaps");
        if (!Directory.Exists(heightmapFolder))
            Directory.CreateDirectory(heightmapFolder);

        PopulateHeightmapList();
    }

    // Täyttä menun heightmap listan , ja asettaa nappeihin oikeat funktiot
    public void PopulateHeightmapList()
    {
        // Listan putsaus
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Hae kaikki .png tiedostot kansiosta
        string[] files = Directory.GetFiles(heightmapFolder, "*.png");

        foreach (string filePath in files)
        {
            string filename = Path.GetFileName(filePath);

            GameObject item = Instantiate(itemPrefab, contentParent);
            TMP_Text textComponent = item.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
                textComponent.text = filename;

            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    Texture2D heightmap = LoadHeightmap(filePath);
                    ApplyHeightmapToTerrain(heightmap, targetTerrain);
                });
            }
        }
    }

    Texture2D LoadHeightmap(string filePath)
    {
        byte[] imageBytes = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageBytes); // Tekstuurin koon automaattinen sovittaminen
        return tex;
    }

    void ApplyHeightmapToTerrain(Texture2D heightmap, Terrain terrain)
    {
        int width = heightmap.width;
        int height = heightmap.height;

        float[,] heights = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = heightmap.GetPixel(x, y);
                heights[y, x] = pixel.grayscale; // tai .r
            }
        }

        terrain.terrainData.heightmapResolution = width;
        terrain.terrainData.size = new Vector3(width, 100, height);
        terrain.terrainData.SetHeights(0, 0, heights);
        RaisePlayerAboveTerrain();
    }

    //Tee tasainen height map
    public void ResetTerrain(Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        int resolution = terrainData.heightmapResolution;
        float[,] flatHeights = new float[resolution, resolution];

        terrain.terrainData.SetHeights(0, 0, flatHeights);
        RaisePlayerAboveTerrain();
    }

    // Nosta käyttäjä terrainin yläpuolelle. Estää käyttäjää tippumasta terrainin läpi.
    void RaisePlayerAboveTerrain()
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 terrainPosition = targetTerrain.transform.position;

        float terrainHeight = targetTerrain.SampleHeight(playerPosition) + terrainPosition.y;
        float safeY = terrainHeight + 1.0f;

        player.transform.position = new Vector3(playerPosition.x, safeY, playerPosition.z);
    }
}