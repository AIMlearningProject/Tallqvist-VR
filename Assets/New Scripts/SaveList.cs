using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Populates a "load list" with save data.

public class SaveList : MonoBehaviour
{
    public GameObject buttonPrefab; // ScrollBtn
    public Transform contentParent; // ScrollView --> ViewPort --> Content
    public UserManager userController;

    public SaveSystem saveSystem;

    void Start()
    {
        PopulateSaveList();
    }

    public void PopulateSaveList()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        string[] saves = saveSystem.GetAllSaveFiles();

        foreach (string timestamp in saves)
        {
            // Button for loading a save.
            GameObject buttonGO = Instantiate(buttonPrefab, contentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = timestamp;

            string ts = timestamp;
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                userController.LoadPlayer(ts);
            });

            // Button for deleting a save file.
            GameObject deleteButton = buttonGO.transform.Find("DeleteBtn").gameObject;
            deleteButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                saveSystem.DeleteSave(ts);
                PopulateSaveList();
            });
        }
    }
}