using UnityEngine;

public class UserManager : MonoBehaviour
{
    private SaveSystem saveSystem;
    private SaveSystem.SaveData saveData;
    public SaveList saveList;

    //Haettavat funktiot löytyvät täältä, käyttää SaveSystemiä tallentamiseen. Tallennus data "SaveData" on nestattuna SaveSystemissä
    void Start()
    {
        saveSystem = GetComponent<SaveSystem>();

        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem not found in scene. Please add it to a GameObject.");
            return;
        }

        //Sovelluksen avattaessa aukeaa viimeisin tallennus
        string latestSave = saveSystem.GetLatestSaveFileName();
        if (latestSave != null)
        {
            saveData = saveSystem.LoadWithTimestamp(latestSave);

            if (saveData != null)
            {
                transform.position = new Vector3(saveData.playerX, saveData.playerY, saveData.playerZ);
            }
        }
    }

    public void SavePlayer()
    {
        if (saveSystem == null) return;

        saveSystem = GetComponent<SaveSystem>();
        saveData = new SaveSystem.SaveData();
        saveSystem.SaveWithTimestamp(saveData);

        saveList.PopulateSaveList();
    }

    public void LoadLatest()
    {
        string latest = saveSystem.GetLatestSaveFileName();
        LoadPlayer(latest);
    }

    public void LoadPlayer(string timestamp)
    {
        if (saveSystem == null) return;
        Debug.Log("Trying to load save with timestamp: " + timestamp);
        SaveSystem.SaveData data = saveSystem.LoadWithTimestamp(timestamp);
        if (data != null)
        {
            transform.position = new Vector3(data.playerX, data.playerY, data.playerZ);
        }
    }
}