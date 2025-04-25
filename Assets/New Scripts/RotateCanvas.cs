using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//K‰‰nt‰‰ prefabeja katsomalla n‰ykvi‰ info tekstej‰ pelaajaa kohti

public class LookAtPlayer : MonoBehaviour
{
    public GameObject player;

    void Update()
    {
        transform.LookAt(player.transform);
    }
}