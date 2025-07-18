using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void onPlayButton ()
    {
        SceneManager.LoadScene("Level1Shaded");
    }

    public void onQuitButton ()
    {
        Application.Quit();
    }
}
