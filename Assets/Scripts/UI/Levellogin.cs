using System.Collections;    
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Levellogin : MonoBehaviour
{
    public int totalLevels = 0; // Total number of levels

    public int unlockedLevels = 1; // Number of unlocked levels

    private int LevelButton[] levelButtons; // Number of level buttons

    public Button[] lvlButtonlogin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int levelAt = PlayerPrefs.GetInt("LevelAt", 2); // Default to level 1 if not set

        for (int i =0; i< lvlButtonlogin.Length; i++)
        {
            if(i+2 > levelAt)
                lvlButtonlogin[i].interactable = false; // Disable button if level is not unlocked
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
