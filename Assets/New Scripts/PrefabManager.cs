using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : MonoBehaviour
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

    public int GetCurrentIndex() => currentIndex;
}
