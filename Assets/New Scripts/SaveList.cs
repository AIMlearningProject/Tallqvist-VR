using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Täyttää Lataa listan tallennetuilla tiedostoilla

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
        // Tyhjennä lista
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Hae kaikki tallennukset aikaleimalla
        string[] saves = saveSystem.GetAllSaveFiles();

        foreach (string timestamp in saves)
        {
            //Lataus nappi
            GameObject buttonGO = Instantiate(buttonPrefab, contentParent);
            buttonGO.GetComponentInChildren<TMP_Text>().text = timestamp;

            string ts = timestamp;
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                userController.LoadPlayer(ts);
            });

            //Poisto nappi
            GameObject deleteButton = buttonGO.transform.Find("DeleteBtn").gameObject;
            deleteButton.GetComponent<Button>().onClick.AddListener(() =>
            {
                saveSystem.DeleteSave(ts);
                PopulateSaveList();
            });
        }
    }
}