using UnityEngine;
using UnityEngine.XR;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public Transform spawnPoint;
    private int selectedPrefabIndex = 0;

    void Update()
    {
        // Oculus button to spawn an object (Trigger button)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            SpawnObject();
        }

        // Oculus button to delete an object (Grip button)
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch))
        {
            DeleteObject();
        }

        // Cycle prefabs using the thumbstick (Right Joystick Left/Right)
        float joystickX = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;

        if (joystickX > 0.5f) // Move Right to cycle forward
        {
            CyclePrefab(1);
        }
        else if (joystickX < -0.5f) // Move Left to cycle backward
        {
            CyclePrefab(-1);
        }
    }

    void SpawnObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            GameObject spawned = Instantiate(prefabs[selectedPrefabIndex], hit.point, Quaternion.identity);
            spawned.tag = "SpawnedObject";
        }
    }

    void DeleteObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit))
        {
            if (hit.collider.gameObject.CompareTag("SpawnedObject"))
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }

    void CyclePrefab(int direction)
    {
        selectedPrefabIndex += direction;
        if (selectedPrefabIndex >= prefabs.Length) selectedPrefabIndex = 0;
        if (selectedPrefabIndex < 0) selectedPrefabIndex = prefabs.Length - 1;
    }
}