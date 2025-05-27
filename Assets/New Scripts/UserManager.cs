using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Unity.XR.CoreUtils;

public class UserManager : MonoBehaviour
{
    private SaveSystem saveSystem;
    private SaveSystem.SaveData saveData;
    public SaveList saveList;
    public TMP_InputField saveNameInput;
    public GameObject spatialKeyboard;
    private SaveSystem.PrefabObjectData prefabObjectData;
    public XROrigin xrOrigin;

    private string saveName;
    private bool wasKeyboardOpen = false;

    //Haettavat funktiot löytyvät täältä, käyttää SaveSystemiä tallentamiseen. Tallennus data "SaveData" on nestattuna SaveSystemissä
    void Start()
    {
        saveSystem = GetComponent<SaveSystem>();
        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem not found in scene. Please add it to a GameObject.");
            return;
        }

        saveNameInput.onSelect.AddListener(OnInputSelected);
        saveNameInput.onDeselect.AddListener(OnInputDeselected);

        //LoadLatest(); //Sovelluksen avattaessa aukeaa viimeisin tallennus
    }

    void Update()
    {
        if (wasKeyboardOpen && !spatialKeyboard.activeSelf)
        {
            Debug.Log("Keyboard was just closed. Triggering save.");
            OnSaveNameEntered(saveNameInput.text);
            wasKeyboardOpen = false;
        }
        if (spatialKeyboard.activeSelf)
        {
            wasKeyboardOpen = true;
        }
    }

    void OnInputSelected(string _)
    {
        Debug.Log("Input field focused — keyboard shown");
    }

    void OnInputDeselected(string _)
    {
        Debug.Log("Input field unfocused — keyboard closed");
        OnSaveNameEntered(saveNameInput.text);
    }

    public void SavePlayer(string saveName)
    {
        if (saveSystem == null) return;

        saveData = new SaveSystem.SaveData();
        SaveSystem.PrefabObjectData objData = new SaveSystem.PrefabObjectData();

        Vector3 cameraPos = xrOrigin.Camera.transform.position;
        saveData.playerX = cameraPos.x;
        saveData.playerY = cameraPos.y;
        saveData.playerZ = cameraPos.z;

        //prefabien tallennus
        foreach (GameObject obj in SpawnedPrefabs.Instance.spawnedObjects)
        {
            if (obj == null) continue;

            var data = new SaveSystem.PrefabObjectData
            {
                prefabID = obj.name.Replace("(Clone)", ""),
                x = obj.transform.position.x,
                y = obj.transform.position.y,
                z = obj.transform.position.z,
                rotation = obj.transform.rotation
            };
            saveData.prefabObjects.Add(data);
            Debug.Log("Saving prefab: " + data.prefabID + " at " + data.x + "," + data.y + "," + data.z);
        }

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
        spatialKeyboard.SetActive(false);
    }

    //Tallennuksen nimeäminen, avaa syöttökentän
    public void OnClickSave()
    {
        saveNameInput.text = "";
        saveNameInput.ForceLabelUpdate();
        saveNameInput.ActivateInputField();
        saveNameInput.caretPosition = 0;
        saveNameInput.selectionAnchorPosition = 0;
        saveNameInput.selectionFocusPosition = 0;
        EventSystem.current.SetSelectedGameObject(saveNameInput.gameObject);
        spatialKeyboard.SetActive(true);
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
        spatialKeyboard.SetActive(false);
        saveNameInput.text = "";
        saveNameInput.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
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
            Vector3 pos = new Vector3(data.playerX, data.playerY, data.playerZ);
            xrOrigin.MoveCameraToWorldLocation(pos);
        }

        //prefabien puhdistus ja lataaminen
        foreach (var obj in SpawnedPrefabs.Instance.spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        SpawnedPrefabs.Instance.spawnedObjects.Clear();
        SpawnedPrefabs.Instance.CleanDestroyedReferences();

        foreach (var objData in data.prefabObjects)
        {
            GameObject prefab = SpawnedPrefabs.Instance.GetPrefabByID(objData.prefabID);
            if (prefab != null)
            {
                Vector3 pos = new Vector3(objData.x, objData.y, objData.z);
                Quaternion rot = objData.rotation;
                var obj = Instantiate(prefab, pos, rot);
                obj.name = prefab.name;
                obj.tag = "Spawnable";
                SpawnedPrefabs.Instance.spawnedObjects.Add(obj);
            }
            Debug.Log("Trying to load prefab: " + objData.prefabID);
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