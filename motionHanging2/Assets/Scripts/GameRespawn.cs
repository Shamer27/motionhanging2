using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameRespawn : MonoBehaviour {
    Vector3 spawnPoint;
    void OnTriggerEnter (Collider col)
    {
        if(col.transform.tag == "death")
        {
             transform.position = spawnPoint;
        }
    }

    void Start() { 
        spawnPoint =transform.position;
    }
}


