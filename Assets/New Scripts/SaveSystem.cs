using System.IO;
using System.Linq;
using UnityEngine;

//Scenen ja "pelaaja" datan tallentaminen ohjelman suorittamisen aikana json muodossa
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

    // Tallenna scene aikaleimalla --> uniikki tiedostonimi
    public void SaveWithTimestamp(SaveData data)
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Path.Combine(saveDirectory, "save_" + timestamp + ".json");

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
        Debug.Log("Game saved to " + filePath);
    }

    //Lataa aikaleimalla tallennettu scene
    public SaveData LoadWithTimestamp(string timestamp)
    {
        if (string.IsNullOrWhiteSpace(timestamp))
        {
            Debug.LogError("LoadWithTimestamp called with empty timestamp.");
            return null;
        }

        string filePath = Path.Combine(saveDirectory, "save_" + timestamp + ".json");
        Debug.Log("Trying to load: " + filePath);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("File does not exist: " + filePath);
            return null;
        }

        string json = File.ReadAllText(filePath);
        Debug.Log("Loaded JSON: " + json);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError("Loaded file is empty.");
            return null;
        }

        return JsonUtility.FromJson<SaveData>(json);
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

    public void DeleteSave(string timestamp)
    {
        string filePath = Path.Combine(saveDirectory, "save_" + timestamp + ".json");

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
        public string sceneName;
        public float playerX;
        public float playerY;
        public float playerZ;
    }
}

//Vaihtoehtoinen tallentaminen ja lataaminen scenen nimen avulla (nime‰‰kˆ k‰ytt‰j‰ scenej‰?)

/*using UnityEngine.SceneManagement;

public SaveData CreateSaveData()
{
    SaveData data = new SaveData();
    data.sceneName = SceneManager.GetActiveScene().name;
    data.playerX = transform.position.x;
    data.playerY = transform.position.y;
    data.playerZ = transform.position.z;

    return data;
}*/