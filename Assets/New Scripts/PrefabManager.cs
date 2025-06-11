using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Prefabin vaihtaminen, sekä prefabin esittäminen UI:ssa.
public class PrefabManager : MonoBehaviour
{
    [SerializeField] private InputActionProperty rightJoystickClick;    // Primary2DAxisClick
    [SerializeField] private GameObject switchPanel;    // SwitchPrefabPanel
    [SerializeField] private Transform buttonContainer; // the Content GameObject
    [SerializeField] private GameObject buttonPrefab;   // PrefabBtn

    public GameObject prefabPanel; // For toggling default UI
    public GameObject PrefabLabel; // For updating default UI

    private GameObject selectedPrefab;
    public List<GameObject> prefabs; // List of all spawnable prefabs.
    private int currentIndex = 0;

    void OnEnable() => rightJoystickClick.action.Enable();
    void OnDisable() => rightJoystickClick.action.Disable();

    void Start()
    {
        UpdateLabel();
        PopulateScrollList();
        switchPanel.SetActive(false);
    }

    void Update()
    {
        if (rightJoystickClick.action.WasPressedThisFrame())
        {
            ToggleMenu();
        }
    }

    public GameObject GetCurrentPrefab()
    {
        return prefabs[currentIndex];
    }

    public void NextPrefab()
    {
        currentIndex = (currentIndex + 1) % prefabs.Count;
        UpdateLabel();
    }

    public void PreviousPrefab()
    {
        currentIndex = (currentIndex - 1 + prefabs.Count) % prefabs.Count;
        UpdateLabel();
    }

    public int GetCurrentIndex() => currentIndex;

    void PopulateScrollList()
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            int index = i;
            GameObject btn = Instantiate(buttonPrefab, buttonContainer);
            btn.GetComponentInChildren<TMP_Text>().text = prefabs[i].name;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                SelectPrefabByIndex(index);
            });
        }
    }

    public void SelectPrefabByIndex(int index)
    {
        if (index < 0 || index >= prefabs.Count)
        {
            Debug.LogWarning("Invalid prefab index selected: " + index);
            return;
        }

        currentIndex = index;
        selectedPrefab = prefabs[currentIndex];
        ToggleMenu();
        UpdateLabel();

        Debug.Log($"Selected prefab: {selectedPrefab.name} at index {currentIndex}");
    }

    // Update default UI
    public void UpdateLabel()
    {
        var label = PrefabLabel.GetComponent<TMP_Text>();
        if (label != null && prefabs.Count > 0)
        {
            label.text = prefabs[currentIndex].name;
        }
    }

    // Toggle default UI
    private void ToggleMenu()
    {
        if (switchPanel != null)
        {
            switchPanel.SetActive(!switchPanel.activeSelf);
        }
        if (prefabPanel != null && prefabPanel.activeSelf)
        {
            prefabPanel.SetActive(false);
        }
        else
        {
            prefabPanel.SetActive(true);
        }
    }
}