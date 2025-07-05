using UnityEngine;
using System.Collections.Generic;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    public Dictionary<string, int> starsPerStage = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (starsPerStage == null)
        {
            starsPerStage = new Dictionary<string, int>();
        }
    }
}