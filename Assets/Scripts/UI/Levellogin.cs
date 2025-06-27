using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Levellogin : MonoBehaviour
{
    /*
   public int totalLevels = 0; // Total number of levels

   public int unlockedLevels = 1; // Number of unlocked levels

   private int LevelButton[] levelButtons; // Number of level buttons

    public Button[] lvlButtonlogin;
    //Start is called once before the first execution of Update after the MonoBehaviour is created
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
        
    }*/

    public Button[] lvlButtonlogin; // Array of level buttons

    public Image[] Lock;

    public Image[] Done;

    private int highestLevel;

    private void Start()
    {
        highestLevel = PlayerPrefs.GetInt("highestLavel", 1);

        for (int i = 0; i < lvlButtonlogin.Length; i++)
        {
            int levelNum = i + 1;
            if (levelNum > highestLevel)
            {
                lvlButtonlogin[i].interactable = false; // Disable button if level is not unlocked
                lvlButtonlogin[i].GetComponent<Image>().sprite = Lock[i].sprite;
                lvlButtonlogin[i].GetComponentInChildren<Text>().text = ""; // Change button text to "Locked"
            }
            else
            {
                lvlButtonlogin[i].interactable = true; // Enable button if level is unlocked    
                lvlButtonlogin[i].GetComponentInChildren<Text>().text = "" + levelNum; // Set button text to level number
                lvlButtonlogin[i].GetComponent<Image>().sprite = Done[i].sprite; // Change button sprite to "Done"
            }
        }
    }

    public void LoadLevel(int levelNum)
    {
        SceneManager.LoadScene("Level_" + levelNum); // Load the scene corresponding to the level index
    }

    public void Reset()
    {
        PlayerPrefs.DeleteAll(); // Reset all PlayerPrefs data
        PlayerPrefs.Save();
    }

}
