using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private string savePath;

    // This is the method that runs when the script starts.
    private void Start()
    {
        // Set the path where we will save the levels.
        savePath = Application.persistentDataPath + "/levels/";

        // If the "levels" directory doesn't exist, create it.
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    // Save the level to a file
    public void SaveLevel(string levelName)
    {
        // Create a new LevelData object to hold all the level information
        LevelData levelData = new LevelData();

        // Find all objects in the scene that are marked as "ModifiableObject"
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag("ModifiableObject");

        // For each object, create an ObjectData object and fill in the relevant information
        foreach (var obj in allObjects)
        {
            ObjectData objData = new ObjectData
            {
                objectName = obj.name,  // Store the object's name
                position = obj.transform.position,  // Store the position
                rotation = obj.transform.rotation,  // Store the rotation
                scale = obj.transform.localScale  // Store the scale
            };
            levelData.objects.Add(objData);  // Add this ObjectData to the LevelData
        }

        // Serialize the LevelData to a JSON string
        string jsonData = JsonUtility.ToJson(levelData, true);

        // Write the serialized data to a file
        File.WriteAllText(savePath + levelName + ".json", jsonData);
        Debug.Log("Level saved as " + levelName);
    }

    // Load a saved level from a file
    public void LoadLevel(string levelName)
    {
        // Define the file path based on the level name
        string filePath = savePath + levelName + ".json";

        // Check if the file exists
        if (File.Exists(filePath))
        {
            // Read the file's contents
            string jsonData = File.ReadAllText(filePath);

            // Deserialize the JSON string back into a LevelData object
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonData);

            // Clear all existing objects with the tag "ModifiableObject"
            GameObject[] existingObjects = GameObject.FindGameObjectsWithTag("ModifiableObject");
            foreach (var obj in existingObjects)
            {
                Destroy(obj);  // Destroy all objects in the scene with the "ModifiableObject" tag
            }

            // Instantiate new objects based on the loaded level data
            foreach (var objData in levelData.objects)
            {
                GameObject newObj = new GameObject(objData.objectName);  // Create a new GameObject

                // Set the position, rotation, and scale based on the saved data
                newObj.transform.position = objData.position;
                newObj.transform.rotation = objData.rotation;
                newObj.transform.localScale = objData.scale;

                // Optional: Set a tag for modifiable objects
                newObj.tag = "ModifiableObject";
            }

            Debug.Log("Level loaded: " + levelName);
        }
        else
        {
            Debug.LogError("Level file not found: " + levelName);
        }
    }

    // Delete a saved level file
    public void DeleteLevel(string levelName)
    {
        // Define the file path based on the level name
        string filePath = savePath + levelName + ".json";

        // Check if the file exists
        if (File.Exists(filePath))
        {
            File.Delete(filePath);  // Delete the file
            Debug.Log("Level deleted: " + levelName);
        }
        else
        {
            Debug.LogError("Level not found for deletion: " + levelName);
        }
    }
}
