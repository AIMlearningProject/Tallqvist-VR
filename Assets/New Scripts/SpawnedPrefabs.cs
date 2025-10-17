using System.Collections.Generic;
using UnityEngine;

// List of prefabs spawned during runtime, used for saving.
public class SpawnedPrefabs : MonoBehaviour
{
    public static SpawnedPrefabs Instance;
    public List<GameObject> spawnablePrefabs;
    public List<GameObject> spawnedObjects = new List<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnObject(int prefabIndex, Vector3 position, Quaternion rotation)
    {
        var prefab = spawnablePrefabs[prefabIndex];
        var obj = Instantiate(prefab, position, rotation);
        spawnedObjects.Add(obj);
        obj.name = prefab.name;
    }

    public GameObject GetPrefabByID(string id)
    {
        foreach (var prefab in spawnablePrefabs)
        {
            if (prefab == null) continue;

            if (prefab.name == id)
                return prefab;
        }

        Debug.LogWarning($"Prefab with ID '{id}' not found in SpawnedPrefabs.");
        return null;
    }

    public void CleanDestroyedReferences()
    {
        spawnedObjects.RemoveAll(item => item == null);
    }
}