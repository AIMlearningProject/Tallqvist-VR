using System.IO;
using System.Linq;
using UnityEngine;

////Terrainin, "pelaaja" datan ja prefab datan tallentaminen ohjelman suorittamisen aikana json muodossa
public class SaveSystem : MonoBehaviour
{
    private string saveDirectory; // Tallennuskansio

    // Tarkastetaan kansion olemassaolo
    void Awake()
    {
        saveDirectory = Path.Combine(Application.persistentDataPath, "SaveFiles");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }
    }

    // Tallenna nimell‰
    public void SaveWithName(SaveData data, string saveName)
    {
        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogError("Save name cannot be empty.");
            return;
        }
        string filePath = Path.Combine(saveDirectory, "save_" + saveName + ".json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Game saved to " + filePath);
    }

    // Lataa nimell‰
    public SaveData LoadWithName(string saveName)
    {
        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogError("LoadWithName called with empty name.");
            return null;
        }

        string filePath = Path.Combine(saveDirectory, "save_" + saveName + ".json");

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            SaveData loadedData = JsonUtility.FromJson<SaveData>(json);
            Debug.Log("Game loaded from " + filePath);
            return loadedData;
        }
        else
        {
            Debug.LogWarning("Save file not found: " + filePath);
            return null;
        }
    }

    //Haetaan kaikki tallennukset --> voidaan esitt‰‰ sek‰ hallita
    public string[] GetAllSaveFiles()
    {
        if (!Directory.Exists(saveDirectory))
        {
            return new string[0];
        }

        return Directory.GetFiles(saveDirectory, "*.json")
                        .Select(path => Path.GetFileNameWithoutExtension(path))
                        .Where(name => name.StartsWith("save_"))
                        .Select(name => name.Substring("save_".Length))
                        .OrderByDescending(f => f)
                        .ToArray();
    }

    public string GetLatestSaveFileName()
    {
        var files = GetAllSaveFiles();
        return files.Length > 0 ? files[0] : null;
    }

    public void DeleteSave(string saveName)
    {
        string filePath = Path.Combine(saveDirectory, "save_" + saveName + ".json");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("Deleted save file: " + filePath);
        }
    }

    //M‰‰ritet‰‰n haluttu tallennus data
    [System.Serializable]
    public class SaveData
    {
        public float playerX, playerY, playerZ;
        public int heightmapResolution;
        public float[] terrainHeights;
    }
}