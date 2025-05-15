using UnityEngine;
using TMPro;

public class UserManager : MonoBehaviour
{
    private SaveSystem saveSystem;
    private SaveSystem.SaveData saveData;
    public SaveList saveList;
    public TMP_InputField saveNameInput;
    public GameObject inputPanel;

    public string saveName;

    //Haettavat funktiot löytyvät täältä, käyttää SaveSystemiä tallentamiseen. Tallennus data "SaveData" on nestattuna SaveSystemissä
    void Start()
    {
        saveSystem = GetComponent<SaveSystem>();

        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem not found in scene. Please add it to a GameObject.");
            return;
        }
      
        saveNameInput.onEndEdit.AddListener(OnSaveNameEntered); //Tekstin syöttökentän syötön lopettamiseen kuuntelija tallentamista varten
        
        LoadLatest(); //Sovelluksen avattaessa aukeaa viimeisin tallennus
    }

    public void SavePlayer(string saveName)
    {
        if (saveSystem == null) return;

        saveData = new SaveSystem.SaveData();

        saveData.playerX = transform.position.x;
        saveData.playerY = transform.position.y;
        saveData.playerZ = transform.position.z;

        // Tallenna terrain heightmap
        Terrain terrain = Terrain.activeTerrain;
        TerrainData tData = terrain.terrainData;
        int res = tData.heightmapResolution;
        saveData.heightmapResolution = res;

        float[,] heights = tData.GetHeights(0, 0, res, res);
        saveData.terrainHeights = Flatten(heights);

        saveSystem = GetComponent<SaveSystem>();
        saveSystem.SaveWithName(saveData, saveName);

        saveList.PopulateSaveList();
        inputPanel.SetActive(false);
    }

    //Tallennuksen nimeäminen, avaa syöttökentän
    public void OnClickSave()
    {
        inputPanel.SetActive(true);
        saveNameInput.text = "";
        saveNameInput.ActivateInputField();
    }

    //Luetaan savename syötön jälkeen ja suoritetaan tallennus
    void OnSaveNameEntered(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Debug.LogWarning("Save name is empty!");
            return;
        }
        saveName = input;
        SavePlayer(saveName);
        inputPanel.SetActive(false);
    }

    public void LoadLatest()
    {
        string latest = saveSystem.GetLatestSaveFileName();
        LoadPlayer(latest);
    }

    public void LoadPlayer(string saveName)
    {
        if (saveSystem == null) return;
            Debug.Log("Trying to load save with saveName: " + saveName);
        SaveSystem.SaveData data = saveSystem.LoadWithName(saveName);
        if (data != null)
        {
            transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
        }
        //Lataa terrain heightmap
        if (data.terrainHeights != null && data.terrainHeights.Length > 0)
        {
            Terrain terrain = Terrain.activeTerrain;
            TerrainData tData = terrain.terrainData;
            float[,] restored = Unflatten(data.terrainHeights, data.heightmapResolution);
            tData.SetHeights(0, 0, restored);
        }
    }
    //Funktiot heightmappien tallentamista varten
    private float[] Flatten(float[,] array)
    {
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        float[] flat = new float[width * height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                flat[y * width + x] = array[x, y];
        return flat;
    }

    private float[,] Unflatten(float[] flat, int res)
    {
        float[,] result = new float[res, res];
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
                result[x, y] = flat[y * res + x];
        return result;
    }
}