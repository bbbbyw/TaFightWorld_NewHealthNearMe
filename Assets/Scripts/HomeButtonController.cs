using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButtonController : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene("Homepage");
    }
}
