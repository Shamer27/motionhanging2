using UnityEngine;
using System.Collections;
using System.Collections.Generic;
// Quits the player when the user hits escape

public class gameQuit : MonoBehaviour
{
    [SerializeField] private KeyCode quitKey;
    Vector3 spawnPoint;
    void Start() { 
        spawnPoint =transform.position;
    }
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }

        
    void OnTriggerEnter (Collider col)
    {
        if(col.transform.tag == "death")
        {
             transform.position = spawnPoint;
        }
    }


}