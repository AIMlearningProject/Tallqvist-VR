using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class HoverCanvasActivator : MonoBehaviour
{
    [SerializeField] private GameObject canvasObject;

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
}
