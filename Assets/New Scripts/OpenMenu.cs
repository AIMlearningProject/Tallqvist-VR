using UnityEngine;
using UnityEngine.XR;

// Toggle all menu panels with Y button. Opening a submenu closes other submenus.

public class OpenMenu : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject loadPanel;
    public GameObject importPanel;
    public GameObject convertPanel;
    public GameObject instructionsPanel;
    public GameObject voxObjPanel;
    public GameObject quadPanel;
    public GameObject blueprintPanel;

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
        if (voxObjPanel != null && voxObjPanel.activeSelf)
        {
            voxObjPanel.SetActive(false);
        }
        if (quadPanel != null && quadPanel.activeSelf)
        {
            quadPanel.SetActive(false);
        }
        if (blueprintPanel != null && blueprintPanel.activeSelf)
        {
            blueprintPanel.SetActive(false);
        }
    }

    public void OpenLoadMenu()
    {
        loadPanel.SetActive(true);
        importPanel.SetActive(false);
        convertPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        voxObjPanel.SetActive(false);
        quadPanel.SetActive(false);
        blueprintPanel.SetActive(false);
    }

    public void OpenImportMenu()
    {
        importPanel.SetActive(true);
        loadPanel.SetActive(false);
        convertPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        voxObjPanel.SetActive(false);
        quadPanel.SetActive(false);
        blueprintPanel.SetActive(false);
    }

    public void OpenConvertMenu()
    {
        convertPanel.SetActive(true);
        importPanel.SetActive(false);
        loadPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        voxObjPanel.SetActive(false);
        quadPanel.SetActive(false);
        blueprintPanel.SetActive(false);
    }

    public void OpenVoxObjMenu()
    {
        voxObjPanel.SetActive(true);
        convertPanel.SetActive(false);
        importPanel.SetActive(false);
        loadPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        quadPanel.SetActive(false);
        blueprintPanel.SetActive(false);
    }

    public void OpenQuadMenu()
    {
        quadPanel.SetActive(true);
        voxObjPanel.SetActive(false);
        convertPanel.SetActive(false);
        importPanel.SetActive(false);
        loadPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        blueprintPanel.SetActive(false);
    }

    public void OpenBlueprintMenu()
    {
        blueprintPanel.SetActive(true);
        quadPanel.SetActive(false);
        voxObjPanel.SetActive(false);
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