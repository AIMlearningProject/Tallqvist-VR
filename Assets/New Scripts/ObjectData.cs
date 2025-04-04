using UnityEngine;

[System.Serializable]  // Make this class serializable
public class ObjectData
{
    public string objectName;  // The name of the object
    public Vector3 position;  // Position of the object
    public Quaternion rotation;  // Rotation of the object
    public Vector3 scale;  // Scale of the object
}
