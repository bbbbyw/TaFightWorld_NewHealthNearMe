using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButtonController : MonoBehaviour
{
    public PoseGameManager poseGameManager;

    public void GoHome()
    {
        if (poseGameManager != null)
        {
            poseGameManager.ForceStop();
        }

        SceneManager.LoadScene("Homepage");
    }
}
