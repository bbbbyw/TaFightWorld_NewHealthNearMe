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
}