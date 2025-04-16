using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRObjectSpawner : MonoBehaviour
{
    public XRRayInteractor xrRayInteractor;  // Reference to your existing XR Ray Interactor
    public InputActionReference joystickInput; // RightJoystick
    public InputActionReference aButton;       // AButton
    public InputActionReference bButton;       // BButton

    public PrefabManager prefabManager;

    private float joystickThreshold = 0.8f;
    private bool canChange = true;

    void Update()
    {
        // Joystick Left/Right to change prefab
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

        // A Button: Spawn
        if (aButton.action.WasPressedThisFrame())
        {
            if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                GameObject prefab = prefabManager.GetCurrentPrefab();
                Instantiate(prefab, hit.point, Quaternion.identity);
            }
        }

        // B Button: Delete
        if (bButton.action.WasPressedThisFrame())
        {
            if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Spawnable"))
                {
                    Destroy(hit.collider.transform.root.gameObject);
                }
            }
        }

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

    void OnEnable()
    {
        joystickInput.action.Enable();
        aButton.action.Enable();
        bButton.action.Enable();
    }

    void OnDisable()
    {
        joystickInput.action.Disable();
        aButton.action.Disable();
        bButton.action.Disable();
    }

}
