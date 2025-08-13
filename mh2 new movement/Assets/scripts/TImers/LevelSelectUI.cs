using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    // Start is called before the first frame update

    public TMP_Text level1TimeText;
    public TMP_Text level2TimeText;
    public TMP_Text level3TimeText;
    public TMP_Text level4TimeText;
    public TMP_Text level5TimeText;
    public TMP_Text level6TimeText;
    public TMP_Text level7TimeText;
    public TMP_Text level8TimeText;
    public TMP_Text level9TimeText;
    public TMP_Text level10TimeText;
    public TMP_Text level11TimeText;
    public TMP_Text level12TimeText;
    public TMP_Text level13TimeText;
    public TMP_Text level14TimeText;
    public TMP_Text level15TimeText;
    public TMP_Text level16TimeText;
    public TMP_Text level17TimeText;
    public TMP_Text level18TimeText;

    void Start()
    {
        ShowBestTime("Level1Shaded", level1TimeText);
        ShowBestTime("Level2Shaded", level2TimeText);
        ShowBestTime("Level3Shaded", level3TimeText);
        ShowBestTime("Level4Shaded", level4TimeText);
        ShowBestTime("Level5Shaded", level5TimeText);
        ShowBestTime("Level6Shaded", level6TimeText);
        ShowBestTime("Level7Shaded", level7TimeText);
        ShowBestTime("Level8Shaded", level8TimeText);
        ShowBestTime("Level9Shaded", level9TimeText);
        ShowBestTime("Level10Shaded", level10TimeText);
        ShowBestTime("Level11Shaded", level11TimeText);
        ShowBestTime("Level12Shaded", level12TimeText);
        ShowBestTime("Level13Shaded", level13TimeText);
        ShowBestTime("Level14Shaded", level14TimeText);
        ShowBestTime("Level15Shaded", level15TimeText);
        ShowBestTime("Level16Shaded", level16TimeText);
        ShowBestTime("Level17Shaded", level17TimeText);
        ShowBestTime("Level18Shaded", level18TimeText);

    }


    void ShowBestTime(string key, TMP_Text text)
    {
        if (PlayerPrefs.HasKey(key))
        {
            float time = PlayerPrefs.GetFloat(key);
            TimeSpan t = TimeSpan.FromSeconds(time);
            text.text = t.ToString(@"mm\:ss\.ff"); // Format: mm:ss.ff
        }
        else
        {
            text.text = "Best: --:--.--";
        }
    }
}
