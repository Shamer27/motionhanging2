using UnityEngine;
using System.Collections;

// Quits the player when the user hits escape
[SerializeField]

public class gameQuit : MonoBehaviour
{
    [SerializeField] private KeyCode quitKey;
    void Update()
    {
        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }
    }
}