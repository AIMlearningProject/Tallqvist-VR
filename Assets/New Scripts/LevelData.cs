using UnityEngine;
using System.Collections.Generic;

[System.Serializable]  // Make this class serializable so we can save/deserialize it to/from JSON
public class LevelData
{
    // A list of all objects in the level
    public List<ObjectData> objects = new List<ObjectData>();
}
