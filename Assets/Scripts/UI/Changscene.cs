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

    public void GotoHomepage()
    {
        SceneManager.LoadScene("Homepage");
    }
}