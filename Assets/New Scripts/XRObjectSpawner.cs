using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRObjectSpawner : MonoBehaviour
{
    public XRRayInteractor xrRayInteractor;  // Reference to your existing XR Ray Interactor
    public InputActionProperty joystickInput; // RightJoystick Swap prefab
    public InputActionProperty aButton;       // AButton Spawn
    public InputActionProperty bButton;       // BButton Delete
    public InputActionProperty rotateInput; // RightJoystick Rotate prefab when grip is pressed
    public InputActionProperty rotateModeButton;  // Grip button

    public MonoBehaviour prefabHUD;
    public PrefabManager prefabManager;
    public Material previewMaterial;
    private Material[] originalMaterials;

    public float rotationSpeed = 40f; // Degrees per second
    private float joystickThreshold = 0.8f;

    private bool canChange = true;
    private bool isInRotateMode = false;
    bool hasPreviewBeenPlaced = false;
    private bool rotateTogglePressedLastFrame = false;

    private GameObject previewObject = null;

    public GameObject spatialNameKeyboard;

    void Update()
    {
        bool rotateButtonPressed = rotateModeButton.action.IsPressed();

        if (rotateButtonPressed && !rotateTogglePressedLastFrame)
        {
            isInRotateMode = !isInRotateMode; // toggle on press
            Debug.Log("Rotate mode toggled: " + isInRotateMode);

            if (prefabHUD != null)
            {
                prefabHUD.enabled = !isInRotateMode;
            }
        }

        rotateTogglePressedLastFrame = rotateButtonPressed;

        HandlePreviewObject();
        HandlePrefabSwitching();
        HandleSpawn();
        HandleDelete();
        LogInputs();

        void HandlePreviewObject()
        {
            // Spawn preview object if null and Grip button is pressed.
            if (previewObject == null && rotateModeButton.action.WasPressedThisFrame())
            {
                if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    GameObject prefab = prefabManager.GetCurrentPrefab();
                    previewObject = Instantiate(prefab, hit.point, Quaternion.identity);

                    previewObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // Stop raycaster feedbackloop.

                    // Disable collider and rigidbody for preview
                    Collider col = previewObject.GetComponent<Collider>();
                    if (col != null) col.enabled = false;

                    Rigidbody rb = previewObject.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.isKinematic = true; // prevents physics simulation
                        rb.useGravity = false; // in case gravity was pulling it down
                    }

                    // Store original materials for restoring them back when spawning.
                    originalMaterials = previewObject.GetComponentsInChildren<Renderer>()
                        .Select(r => r.material)
                        .ToArray();

                    // Apply preview material.
                    foreach (var renderer in previewObject.GetComponentsInChildren<Renderer>())
                    {
                        renderer.material = previewMaterial;
                    }

                    hasPreviewBeenPlaced = false;
                }
            }

            if (previewObject != null && !hasPreviewBeenPlaced && aButton.action.WasReleasedThisFrame())
            {
                hasPreviewBeenPlaced = true;
            }

            // If preview exists, update its position to raycast hit point
            if (previewObject != null && !hasPreviewBeenPlaced)
            {
                if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    previewObject.transform.position = hit.point;

                    // Rotate preview while in rotate mode
                    if (isInRotateMode)
                    {
                        float rotateValue = rotateInput.action.ReadValue<Vector2>().x;
                        if (Mathf.Abs(rotateValue) > 0.1f)
                        {
                            previewObject.transform.Rotate(Vector3.up, -rotateValue * rotationSpeed * Time.deltaTime);
                        }
                    }
                }
            }
        }

        void HandlePrefabSwitching()
        {
            if (isInRotateMode)
                return; // Disable prefab switching during rotation mode

            float horizontal = joystickInput.action.ReadValue<Vector2>().x;

            if (canChange && Mathf.Abs(horizontal) > joystickThreshold)
            {
                if (horizontal > 0)
                    prefabManager.NextPrefab();
                else
                    prefabManager.PreviousPrefab();

                canChange = false;
            }

            if (Mathf.Abs(horizontal) < 0.1f)
                canChange = true;
        }

        void HandleSpawn()
        {
            if (previewObject != null && hasPreviewBeenPlaced && aButton.action.WasPressedThisFrame())
            {
                // Enable collider, rigidbody, interactable layer and finalize placement.
                Collider col = previewObject.GetComponent<Collider>();
                if (col != null) col.enabled = true;

                Rigidbody rb = previewObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                }
                previewObject.layer = LayerMask.NameToLayer("Interactable");

                // Restore original materials
                var renderers = previewObject.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].material = originalMaterials[i];
                }

                // Tag as a spawnable for delete detection
                previewObject.tag = "Spawnable";

                if (!SpawnedPrefabs.Instance.spawnedObjects.Contains(previewObject))
                {
                    SpawnedPrefabs.Instance.spawnedObjects.Add(previewObject);
                }

                var activator = previewObject.GetComponentInChildren<HoverCanvasActivator>();
                if (activator != null)
                {
                    TMP_InputField input = previewObject.GetComponentInChildren<TMP_InputField>(true);
                    //activator.Initialize(spatialNameKeyboard, input);
                    //activator.AutoAssignCanvasElements(spatialKeyboard); // p‰‰llekk‰isyytt‰, TESTAA
                }
                else
                {
                    Debug.LogWarning("Spawned object missing HoverCanvasActivator!");
                }

                if (prefabHUD != null)
                {
                    prefabHUD.enabled = true;
                }
                previewObject = null;
                isInRotateMode = false;
                hasPreviewBeenPlaced = false; // Reset preview flag.
            }
        }

        void HandleDelete()
        {
            if (bButton.action.WasPressedThisFrame())
            {
                if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    GameObject obj = hit.collider.transform.root.gameObject;

                    if (hit.collider.CompareTag("Spawnable"))
                    {
                        // Remove from the spawned list for saving to work correctly
                        SpawnedPrefabs.Instance.spawnedObjects.Remove(obj);

                        // Destroy the actual object
                        Destroy(obj);
                    }
                }
            }
        }

        void LogInputs()
        {
            Vector2 joy = joystickInput.action.ReadValue<Vector2>();
            if (joy.magnitude > 0.1f)
            {
                Debug.Log("Joystick Input: " + joy);
            }

            if (aButton.action.WasPressedThisFrame())
            {
                Debug.Log("A Button Pressed");
            }

            if (bButton.action.WasPressedThisFrame())
            {
                Debug.Log("B Button Pressed");
            }
        }
    }

    void OnEnable()
    {
        joystickInput.action.Enable();
        aButton.action.Enable();
        bButton.action.Enable();
        rotateModeButton.action.Enable();
        rotateInput.action.Enable();
    }

    void OnDisable()
    {
        joystickInput.action.Disable();
        aButton.action.Disable();
        bButton.action.Disable();
        rotateModeButton.action.Disable();
        rotateInput.action.Disable();
    }
}