using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverCanvasActivator : MonoBehaviour
{
    [SerializeField] public GameObject canvasObject;

    [SerializeField] public TMP_Text infoDisplayText;
    public TMP_InputField saveInfoInput;
    public GameObject spatialKeyboard;

    public string infoText;
    private bool wasKeyboardOpen = false;

    public void Initialize(GameObject keyboard, TMP_InputField inputField)
    {
        Debug.Log("Initializing HoverCanvasActivator with keyboard: " + keyboard.name);

        spatialKeyboard = keyboard;
        saveInfoInput = inputField;

        if (saveInfoInput != null)
        {
            saveInfoInput.onSelect.AddListener(OnInputSelected);
            saveInfoInput.onDeselect.AddListener(OnInputDeselected);
        }
    }

    void Update()
    {
        if (spatialKeyboard == null)
            return;

        if (wasKeyboardOpen && !spatialKeyboard.activeSelf)
        {
            Debug.Log("Keyboard was just closed. Triggering newInfo.");
            OnInfoNameEntered(saveInfoInput.text);
            wasKeyboardOpen = false;
        }
        if (spatialKeyboard.activeSelf)
        {
            wasKeyboardOpen = true;
        }
    }

    private void Awake()
    {
        if (canvasObject != null)
        {
            canvasObject.SetActive(false); // Make sure it's off initially
        }
    }

    public void OnHoverEntered(HoverEnterEventArgs args)
    {
        if (canvasObject != null)
        {
            canvasObject.SetActive(true);
        }
    }

    public void OnHoverExited(HoverExitEventArgs args)
    {
        if (canvasObject != null)
        {
            canvasObject.SetActive(false);
        }
    }

    void OnInputSelected(string _)
    {
        Debug.Log("Input field focused — showing keyboard");
        spatialKeyboard.SetActive(true);
        EventSystem.current.SetSelectedGameObject(saveInfoInput.gameObject);
    }
    void OnInputDeselected(string _)
    {
        Debug.Log("Input field unfocused — keyboard closed");
        OnInfoNameEntered(saveInfoInput.text);
    }

    public void OnClickNewInfo()
    {
        if (spatialKeyboard != null)
        {
            infoText = infoDisplayText.text; // Start with current value
            spatialKeyboard.SetActive(true);
        }
        saveInfoInput.text = "";
        saveInfoInput.ForceLabelUpdate();
        saveInfoInput.ActivateInputField();
        saveInfoInput.caretPosition = 0;
        saveInfoInput.selectionAnchorPosition = 0;
        saveInfoInput.selectionFocusPosition = 0;
        EventSystem.current.SetSelectedGameObject(saveInfoInput.gameObject);
        spatialKeyboard.SetActive(true);
    }

    public void OnInfoNameEntered(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Debug.LogWarning("Save name is empty!");
            return;
        }
        infoText = input;
        newInfo(infoText);
        spatialKeyboard.SetActive(false);
        saveInfoInput.text = "";
        saveInfoInput.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void newInfo(string newInfo)
    {
        if (infoDisplayText != null)
        {
            infoDisplayText.text = newInfo;
            Debug.Log("Updated info display to: " + newInfo);
        }
        else
        {
            Debug.LogWarning("Info display text reference not set!");
        }
    }

    public void AutoAssignCanvasElements(GameObject spatialKeyboardFromScene)
    {
        // Automatically find canvas and UI components inside this prefab
        canvasObject = GetComponentInChildren<Canvas>(true)?.gameObject;
        infoDisplayText = GetComponentInChildren<TMPro.TMP_Text>(true);
        saveInfoInput = GetComponentInChildren<TMPro.TMP_InputField>(true);
        if (spatialKeyboard == null)
            spatialKeyboard = spatialKeyboardFromScene;
    }
}