using UnityEngine;
using System.Collections;
using System.Collections.Generic;   
using UnityEngine.UI;

public class Levellogin : MonoBehaviour
{
    public Button[] lvlButtonlogin;
    //Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int levelAt = PlayerPrefs.GetInt("LevelAt", 2); // Default to level 1 if not set

        for (int i = 0; i < lvlButtonlogin.Length; i++)
        {
            if (i + 2 > levelAt)
            {
                lvlButtonlogin[i].interactable = false; // Disable button if level is not unlocked
                Color newColor = Color.gray;
                lvlButtonlogin[i].GetComponent<Image>().color = newColor; // Change button color to gray
            }

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

}