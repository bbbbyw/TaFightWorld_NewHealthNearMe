using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Changscene : MonoBehaviour
{
    public void GotoLogin()
    {
        SceneManager.LoadScene("Login");
    }

    public void QuitTheGame()
    {
        Debug.Log("Button working game exited");
        Application.Quit();
    }

    public void GotoStart()
    {
        SceneManager.LoadScene("Start Page");
    }

    public void GotoHomepage()
    {
        SceneManager.LoadScene("Homepage");
    }

    public void GotoDailyLogin()
    {
        SceneManager.LoadScene("Daily Login");
    }

    public void GotoSelectStage()
    {
        SceneManager.LoadScene("Select Stage");
    }

    public void GotoShuffle()
    {
        SceneManager.LoadScene("ShuffleMode");
    }

    public void GotoDressing()
    {
        SceneManager.LoadScene("Dressing Scene");
    }
    public void GotoStage1()
    {
        SceneManager.LoadScene("Stage1 Dog");
    }
    public void GotoStage2()
    {
        SceneManager.LoadScene("Stage2 Garden");
    }
    public void GotoStage3()
    {
        SceneManager.LoadScene("Stage3 Laundry");
    }
    public void GotoStage4()
    {
        SceneManager.LoadScene("Stage4 AccidentSt");
    }
    public void GotoStage5()
    {
        SceneManager.LoadScene("Stage5 KnightTrialScene");
    }

}