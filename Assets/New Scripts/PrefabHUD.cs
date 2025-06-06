using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.XR;

//Asettaa k�ytt�j�n valitseman prefabin nimen HUD:iin.

public class PrefabHUD : MonoBehaviour
{
    public List<GameObject> prefabs;
    private int currentIndex = 0;

    public GameObject PrefabLabel;

    private InputDevice rightController;
    private bool stickInUse = false;

    void Start()
    {
        UpdateLabel();
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    //Ohjaimen lukeminen, tattiohjauksen pit�� palautua "0" asentoon ennen seuraavaa tapahtumaa.
    void Update()
    {
        if (!rightController.isValid)
        {
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            return;
        }

        Vector2 primary2DAxis;
        if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis))
        {
            if (!stickInUse)
            {
                if (primary2DAxis.x > 0.8f)
                {
                    NextPrefab();
                    stickInUse = true;
                }
                else if (primary2DAxis.x < -0.8f)
                {
                    PreviousPrefab();
                    stickInUse = true;
                }
            }

            if (stickInUse && Mathf.Abs(primary2DAxis.x) < 0.1f)
            {
                stickInUse = false;
            }
        }
    }

    public void NextPrefab()
    {
        currentIndex = (currentIndex + 1) % prefabs.Count;
        UpdateLabel();
    }

    public void PreviousPrefab()
    {
        currentIndex = (currentIndex - 1 + prefabs.Count) % prefabs.Count;
        UpdateLabel();
    }

    public void UpdateLabel()
    {
        var label = PrefabLabel.GetComponent<TMP_Text>();
        if (label != null && prefabs.Count > 0)
        {
            label.text = prefabs[currentIndex].name;
        }
    }

    public GameObject GetCurrentPrefab()
    {
        return prefabs[currentIndex];
    }
}