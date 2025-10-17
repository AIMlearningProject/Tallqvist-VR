using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

// Rotates canvas objects above prefabs towards the player.

public class LookAtPlayer : MonoBehaviour
{
    private Transform player;

    void Start()
    {
        GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        player = cameraObj.transform;
    }

    void Update()
    {
        transform.LookAt(player.transform);

        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }
}