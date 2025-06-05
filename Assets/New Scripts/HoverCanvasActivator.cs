using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Activates canvas elements on prefabs when pointing at them with raycaster.
// Functionality to rewrite canvas info texts on run time. Now can only edit currently spawned prefab, no "old" ones.

public class HoverCanvasActivator : MonoBehaviour
{
    public InputActionProperty Trigger; // RightHand XRcontroller Trigger Button
    [SerializeField] public GameObject canvasObject; // Prefabs -> "Prefab" -> "Prefab"Canvas
    [SerializeField] public TMP_Text infoDisplayText; // Prefabs -> "Prefab" -> "Prefab"Canvas -> text(TMP)

    [SerializeField] private TMP_InputField inputField;
    private XRRayInteractor xrRayInteractor;
    public GameObject spatialKeyboard;

    public static HoverCanvasActivator currentActive;
    private bool wasKeyboardOpen = false;

    void Start()
    {
        if (xrRayInteractor == null)
        {
            xrRayInteractor = FindFirstObjectByType<XRRayInteractor>();
        }
        if (canvasObject != null)
        {
            canvasObject.SetActive(false);
        }

        if (infoDisplayText == null)
            Debug.LogError("infoDisplayText is NOT assigned!");
        else
            Debug.Log("infoDisplayText assigned: " + infoDisplayText.name);
    }

    void OnEnable()
    {
        Trigger.action.Enable();
    }

    void OnDisable()
    {
        Trigger.action.Disable();
    }

    void Update()
    {
        if (spatialKeyboard == null)
            return;

        if (wasKeyboardOpen && !spatialKeyboard.activeSelf)
        {
            Debug.Log("Keyboard was just closed.");
            wasKeyboardOpen = false;

            if (HoverCanvasActivator.currentActive == this && inputField != null) // Calls OnInputFieldSubmit.
            {
                inputField.onEndEdit.Invoke(inputField.text);
            }
        }

        if (spatialKeyboard.activeSelf)
        {
            wasKeyboardOpen = true;
        }

        if (Trigger.action.WasPressedThisFrame())
        {
            if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                // Check if the raycast hit info canvas.
                var canvas = hit.collider.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    OnClickNewInfo();
                }
            }
        }
    }

    // For canvas to activate when prefab is pointed with raycaster.
    // Assigned to XR Grab Interactable -> Interactable Events.
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

    public void ReceiveKeyboardInput(string input)
    {
        Debug.Log($"ReceiveKeyboardInput called with input: '{input}'");

        if (string.IsNullOrWhiteSpace(input))
        {
            Debug.LogWarning("Keyboard input is empty.");
            return;
        }

        infoDisplayText.text = input;
        infoDisplayText.ForceMeshUpdate();

        Debug.Log("infoDisplayText is child of: " + infoDisplayText.transform.parent.name);
        Debug.Log("This HoverCanvasActivator is on: " + gameObject.name);

        spatialKeyboard?.SetActive(false);
        StartCoroutine(ClearSelectedGameObjectNextFrame());
    }

    public void OnInputFieldSubmit(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        ReceiveKeyboardInput(inputField.text);

        spatialKeyboard?.SetActive(false);
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnClickNewInfo()
    {
        if (spatialKeyboard == null || inputField == null)
        {
            return;
        }

        if (inputField == null)
        {
            Debug.LogWarning("inputField is null!");
            return;
        }

        if (currentActive != null && currentActive != this) // Trying to get old canvases editable, not yet working.
        {
            currentActive.DeactivateInput();
        }
        currentActive = this;

        spatialKeyboard.SetActive(true);
        StartCoroutine(FocusInputFieldNextFrame());
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        inputField.ActivateInputField();
        inputField.Select();
    }

    // Initialize is callde from XRObjectSpawner when spawning prefabs -> HandleSpawn()
    public void Initialize(GameObject spatialKeyboardFromScene, TMP_InputField inputFieldFromSpatialKeyboard)
    {
        spatialKeyboard = spatialKeyboardFromScene;
        inputField = inputFieldFromSpatialKeyboard;

        // Assign canvasObject if null or unassigned.
        if (canvasObject == null)
        {
            canvasObject = GetComponentInChildren<Canvas>(true)?.gameObject;
            if (canvasObject == null)
            {
                Debug.LogWarning("Canvas object not found in children during Initialize.");
            }
        }

        // Assign infoDisplayText if null or unassigned.
        if (infoDisplayText == null)
        {
            infoDisplayText = GetComponentInChildren<TMPro.TMP_Text>(true);
            if (infoDisplayText == null)
            {
                Debug.LogWarning("TMP_Text (infoDisplayText) not found in children during Initialize.");
            }
        }

        if (inputField != null)
        {
            inputField.onEndEdit.RemoveAllListeners();
            inputField.onEndEdit.AddListener(OnInputFieldSubmit);
        }

        if (spatialKeyboard == null)
        {
            Debug.LogWarning("SpatialKeyboard passed to Initialize is null.");
        }

        Debug.Log($"Initialize completed. spatialKeyboard: {spatialKeyboard?.name}, inputField: {inputField?.name}, canvasObject: {canvasObject?.name}, infoDisplayText: {infoDisplayText?.name}");
    }

    // Trying to get old canvases editable, not working yet.
    public void DeactivateInput()
    {
        if (inputField != null)
        {
            inputField.onEndEdit.Invoke(inputField.text);
            inputField.DeactivateInputField();
        }

        if (canvasObject != null)
        {
            canvasObject.SetActive(false);
        }
    }

    // So events wont be triggered at the same time.
    private IEnumerator ClearSelectedGameObjectNextFrame()
    {
        yield return null; // Wait for one frame.
        EventSystem.current.SetSelectedGameObject(null);
    }

    private IEnumerator FocusInputFieldNextFrame()
    {
        yield return null; // Wait for one frame.
        EventSystem.current.SetSelectedGameObject(null);
        yield return null; // Wait for two frames.
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        inputField.ActivateInputField();
        inputField.Select();
    }
}