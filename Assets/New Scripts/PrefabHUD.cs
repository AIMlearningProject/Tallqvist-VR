using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

//Asettaa käyttäjän valitseman prefabin nimen UI:hin

public class PrefabHUD : MonoBehaviour
{
    public List<GameObject> prefabs;
    private int currentIndex = 0;

    public GameObject GetCurrentPrefab()
    {
        return prefabs[currentIndex];
    }
    public void NextPrefab()
    {
        currentIndex = (currentIndex + 1) % prefabs.Count;
    }

    public void PreviousPrefab()
    {
        currentIndex = (currentIndex - 1 + prefabs.Count) % prefabs.Count;
    }

    public GameObject PrefabLabel;

    public PrefabManager manager;
    public void OnEnable()
    {

        if (currentIndex == 0)
        {
            PrefabLabel.GetComponent<TMP_Text>().text = "Crate";
        }
        else if (currentIndex == 1)
        {
            PrefabLabel.GetComponent<TMP_Text>().text = "Shelter";
        }
        else
        {
            PrefabLabel.GetComponent<TMP_Text>().text = "Toilet";
        }
    }

    public int GetCurrentIndex() => currentIndex;
}