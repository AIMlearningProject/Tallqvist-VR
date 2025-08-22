using UnityEngine;
using UnityEngine.XR;

// Asettaa MenuPanelin aktiiviseksi Y nappia painamalla
// Y nappi myˆs sulkee kaikki menut
// Ala menun avaaminen sulkee toiset alamenut

public class OpenMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject loadPanel;
    public GameObject importPanel;
    public GameObject convertPanel;
    public GameObject instructionsPanel;
    public GameObject VoxObjPanel;
    public GameObject QuadPanel;

    private InputDevice leftController;

    private float cooldownTime = 0.3f;
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
        if (convertPanel != null && convertPanel.activeSelf)
        {
            convertPanel.SetActive(false);
        }
        if (VoxObjPanel != null && VoxObjPanel.activeSelf)
        {
            VoxObjPanel.SetActive(false);
        }
        if (QuadPanel != null && QuadPanel.activeSelf)
        {
            QuadPanel.SetActive(false);
        }
    }

    // Funktio "Lataa scene napille", lis‰t‰‰n nappin on click eventtiin kutsuttavaksi.
    public void OpenLoadMenu()
    {
        loadPanel.SetActive(true);
        importPanel.SetActive(false);
        convertPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        VoxObjPanel.SetActive(false);
        QuadPanel.SetActive(false);
    }

    public void OpenImportMenu()
    {
        importPanel.SetActive(true);
        loadPanel.SetActive(false);
        convertPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        VoxObjPanel.SetActive(false);
        QuadPanel.SetActive(false);
    }

    public void OpenConvertMenu()
    {
        convertPanel.SetActive(true);
        importPanel.SetActive(false);
        loadPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        VoxObjPanel.SetActive(false);
        QuadPanel.SetActive(false);
    }

    public void OpenVoxObjMenu()
    {
        VoxObjPanel.SetActive(true);
        convertPanel.SetActive(false);
        importPanel.SetActive(false);
        loadPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        QuadPanel.SetActive(false);
    }

    public void OpenQuadMenu()
    {
        QuadPanel.SetActive(true);
        VoxObjPanel.SetActive(false);
        convertPanel.SetActive(false);
        importPanel.SetActive(false);
        loadPanel.SetActive(false);
        instructionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        //UnityEditor.EditorApplication.ExitPlaymode();
    }
}