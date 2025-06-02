using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Teleport player to raycaster hitpoint. Hold Lgrip to show crosshair, press Ltrigger to teleport.
public class TeleportUser : MonoBehaviour
{
    public Transform playerRig; // XR Origin (XR Rig)
    public XRRayInteractor xrRayInteractor;
    public GameObject teleportCrosshairPrefab; // TeleportCrosshair
    public InputActionProperty leftGrip; // For showing the crosshair and enabling teleportation.
    public InputActionProperty leftTrigger; // Teleport to raycaster location indicated by TeleportCrosshair prefab.

    private GameObject teleportCrosshair;
    private bool isGripHeld = false;

    void OnEnable()
    {
        leftGrip.action.Enable();
        leftTrigger.action.Enable();
    }
    void OnDisable()
    {
        leftGrip.action.Disable();
        leftTrigger.action.Disable();
    }

    void Start()
    {
        teleportCrosshair = Instantiate(teleportCrosshairPrefab);

        teleportCrosshair.layer = LayerMask.NameToLayer("Ignore Raycast"); // Stop raycaster feedbackloop.

        Collider col = teleportCrosshair.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = teleportCrosshair.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        teleportCrosshair.SetActive(false);
    }

    void Update()
    {
        isGripHeld = leftGrip.action.IsPressed();

        if (isGripHeld)
        {
            if (xrRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                teleportCrosshair.SetActive(true);
                teleportCrosshair.transform.position = hit.point + Vector3.up * 0.05f; ;
                teleportCrosshair.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

                if (leftTrigger.action.WasPressedThisFrame())
                {
                    Vector3 offset = playerRig.position - xrRayInteractor.transform.position;
                    Vector3 targetPosition = hit.point + offset;
                    playerRig.position = new Vector3(targetPosition.x, hit.point.y, targetPosition.z);

                    teleportCrosshair.SetActive(false);
                }
            }
            else
            {
                teleportCrosshair.SetActive(false);
            }
        }
        else
        {
            teleportCrosshair.SetActive(false);
        }
    }
}