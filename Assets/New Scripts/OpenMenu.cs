using UnityEngine;
using UnityEngine.XR;

//Asettaa MenuPanelin aktiiviseksi Y nappia painamalla

public class OpenMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject loadPanel;
    public GameObject importPanel;

    private InputDevice leftController;

    private float cooldownTime = 0.2f;
    private float lastInputTime = 0f;

    void Start()
    {
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        InputDevice leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        bool secondaryButtonPressed;
        if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed) && secondaryButtonPressed)
        {
            if (Time.time - lastInputTime >= cooldownTime)
            {
                ToggleMenu();
                lastInputTime = Time.time;
            }
        }
    }

    private void ToggleMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
        if (loadPanel != null && loadPanel.activeSelf)
        {
            loadPanel.SetActive(false);
        }
        if (importPanel != null && importPanel.activeSelf)
        {
            importPanel.SetActive(false);
        }
    }

    // Funktio "Lataa scene napille", lis‰t‰‰n nappin on click eventtiin kutsuttavaksi.
    public void OpenLoadMenu()
    {
        loadPanel.SetActive(true);
        importPanel.SetActive(false);
    }

    public void OpenImportMenu()
    {
        importPanel.SetActive(true);
        loadPanel.SetActive(false);
    }

    public void QuitGame()
    {
        //Application.Quit();
        UnityEditor.EditorApplication.ExitPlaymode();
    }
}
