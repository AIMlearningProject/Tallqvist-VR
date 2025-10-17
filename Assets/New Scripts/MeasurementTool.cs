using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Activate a tool for measuring with X button. Place a measurement point with a left trigger. Distance between 2 points indicated by marker prefabs is shown on the UI.

public class MeasurementTool : MonoBehaviour
{
    public Transform leftController; // Left Controller
    public GameObject pointPrefab;   // Prefabs --> Marker
    public GameObject distanceDisplay; // MeasurePanel --> DistanceResult
    public GameObject measurePanel; // MeasurePanel
    public XRRayInteractor xrRayInteractor;

    public MonoBehaviour TeleportUser;

    public InputActionProperty toggleToolAction; // XButton (leftPrimary)
    public InputActionProperty placePointAction; // Left Trigger

    private GameObject pointA;
    private GameObject pointB;

    private bool toolActive = false;
    private bool firstPointPlaced = false;
    private bool wasPressedLastFrame = false;

    void Update()
    {
        bool isPressed = toggleToolAction.action.ReadValue<float>() > 0.5f;
        if (isPressed && !wasPressedLastFrame)
        {
            ToggleTool(new InputAction.CallbackContext());
        }
        wasPressedLastFrame = isPressed;
    }

    void OnEnable()
    {
        placePointAction.action.performed += PlaceOrMovePoint;
        toggleToolAction.action.Enable();
        placePointAction.action.Enable();
    }

    void OnDisable()
    {
        placePointAction.action.performed -= PlaceOrMovePoint;
        toggleToolAction.action.Disable();
        placePointAction.action.Disable();
    }

    void ToggleTool(InputAction.CallbackContext ctx)
    {
        toolActive = !toolActive;

        if (measurePanel != null)
        {
            measurePanel.SetActive(toolActive); // use toolActive directly
        }

        // Disable teleportation when measure tool is active.
        if (TeleportUser != null)
        {
            TeleportUser.enabled = !toolActive;
        }

        if (pointA) pointA.SetActive(toolActive);
        if (pointB) pointB.SetActive(toolActive);
        if (distanceDisplay) distanceDisplay.gameObject.SetActive(toolActive);
    }

    // Move measure points to raycaster hitpoints.
    void PlaceOrMovePoint(InputAction.CallbackContext ctx)
    {
        if (!toolActive) return;

        Vector3 currentPosition;
        if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            currentPosition = hit.point; // Ray hit position
        }
        else // Fallback if raycaster is not hitting anything, place marker a bit further from player and on the terrain level.
        {
            Vector3 forward = xrRayInteractor.transform.forward;
            Vector3 projectedPosition = xrRayInteractor.transform.position + forward * 2f;

            projectedPosition.y = Terrain.activeTerrain
                ? Terrain.activeTerrain.SampleHeight(projectedPosition) + Terrain.activeTerrain.transform.position.y
                : 0f;

            currentPosition = projectedPosition;
        }

        if (!firstPointPlaced)
        {
            // Place both points at the first position
            pointA = Instantiate(pointPrefab, currentPosition, Quaternion.identity);
            pointB = Instantiate(pointPrefab, currentPosition, Quaternion.identity);
            firstPointPlaced = true;
        }
        else
        {
            // Move pointA to where pointB was, and move pointB to new place
            pointA.transform.position = pointB.transform.position;
            pointB.transform.position = currentPosition;
        }

        UpdateDistance();
    }

    void UpdateDistance()
    {
        float dist = Vector3.Distance(pointA.transform.position, pointB.transform.position);
        var label = distanceDisplay.GetComponent<TMP_Text>();
        label.text = $"{dist:F2} metriä";
    }
}