using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Core;


public class AccidentStageManager : MonoBehaviour
{
    [Header("Stage Configuration")]
    [SerializeField] private Button retryButton;

    [Header("UI References")]
    [SerializeField] private GameObject stageCompletePanel;
    [SerializeField] private TextMeshProUGUI stageCompleteText;
    [SerializeField] private GameObject stageFailPanel;
    //[SerializeField] private TextMeshProUGUI attemptsRemainingText;

    [Header("Pose System")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private GameObject PoseIconResult;
    [SerializeField] private GameObject blackFilter;

    [Header("Life System")]
    [SerializeField] private GameObject heartIconPrefab;
    [SerializeField] private Transform heartContainer;

    [Header("Star System")]
    [SerializeField] private GameObject starIconPrefab;
    [SerializeField] private Transform starContainer;


    // Game state tracking
    private bool isGameCompleted = false;
    public bool IsGameCompleted => isGameCompleted;

    // Fail state tracking
    private int currentFailCount = 0;
    private bool hasShownCompletion = false;
    private const int maxFailAttempts = 3;
    private bool isStageFailed = false;
    public bool IsStageFailed => isStageFailed;

    // Score system (Star)
    private int starCount = 0;
    public int StarCount => starCount;

    void Start()
    {
        Debug.Log("[StageManager] Starting with maxFailAttempts: " + maxFailAttempts);

        if (stageCompletePanel != null)
        {
            stageCompletePanel.SetActive(false);
        }

        if (stageFailPanel != null)
        {
            stageFailPanel.SetActive(false);
        }
        retryButton.onClick.AddListener(OnPlayerPressedRestart);

        UpdateAttemptsRemainingUI();
    }

    void Update()
    {

    }

    private void UpdateAttemptsRemainingUI()
    {
        int remainingAttempts = maxFailAttempts - currentFailCount;

        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < remainingAttempts; i++)
        {
            Instantiate(heartIconPrefab, heartContainer);
        }
    }

    public void OnChallengeFail(ChallengeTriggerZone zone, bool waitForPlayerRetry)
    {
        currentFailCount++;
        Debug.Log($"[StageManager] Current fail count: {currentFailCount}, Max attempts: {maxFailAttempts}");

        UpdateAttemptsRemainingUI();

        // Check if max attempts reached immediately
        if (currentFailCount >= maxFailAttempts)
        {
            if (resultPanel != null && resultPanel.activeSelf)
            {
                resultPanel.SetActive(false);
                blackFilter.gameObject.SetActive(false);
                PoseIconResult.gameObject.SetActive(false);
                Debug.Log("[StageManager] resultPanel inactivation!");
            }

            if (!isStageFailed)
            {
                isStageFailed = true;
                Debug.Log("[StageManager] Max fail attempts reached, showing fail panel");
                StartCoroutine(ShowStageFailPanel());
            }
        }
        else
        {
            if (!waitForPlayerRetry)
            {
                Debug.Log($"[StageManager] Restarting challenge - Attempts remaining: {maxFailAttempts - currentFailCount}");
                zone.RestartChallenge();
            }
            else
            {
                Debug.Log("[StageManager] Waiting for retry button instead of restarting immediately.");
            }
        }
    }

    private IEnumerator ShowStageFailPanel()
    {
        Debug.Log("[StageManager] Showing stage fail panel");

        if (stageFailPanel == null)
        {
            Debug.LogError("❌ Stage fail panel is NULL!");
            yield break;
        }

        stageFailPanel.SetActive(true);

        foreach (Transform child in stageFailPanel.transform)
        {
            if (!child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(true);
            }
        }

        Canvas.ForceUpdateCanvases();

        // Force panel to be in front of camera
        Transform cam = Camera.main.transform;
        stageFailPanel.transform.position = cam.position + cam.forward * 2f;
        stageFailPanel.transform.rotation = cam.rotation;
        stageFailPanel.transform.localScale = Vector3.one * 0.01f;

        Canvas.ForceUpdateCanvases();

        Debug.Log("✅ Forced stageFailPanel to be in front of camera");

        yield break;
    }

    public void OnPlayerPressedRestart()
    {
        Debug.Log("[StageManager] 🟢 Retry button clicked! -> Restart Game!");
        RestartGame();
    }

    public void RestartGame()
    {
        Debug.Log("[StageManager] Restarting game - Resetting all progress");
        // Reset all progress
        currentFailCount = 0;  // Reset fail count when restarting entire game
        hasShownCompletion = false;
        isGameCompleted = false; // Reset game completion state

        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ShowGameCompletion()
    {
        if (!hasShownCompletion && stageCompletePanel != null)
        {
            hasShownCompletion = true;
            isGameCompleted = true; // Set game completion state
            stageCompletePanel.SetActive(true);

            if (stageCompleteText != null)
            {
                stageCompleteText.text = "ในที่สุด!\nคุณจะได้เดินทางไปยังปราสาทแล้ว";
            }

            ShowStarsBasedOnRemainingHearts();

            // Reset fail count when game is complete
            currentFailCount = 0;
            UpdateAttemptsRemainingUI();

            // Log completion for debugging/analytics
            Debug.Log("[StageManager] Game completed! Player got the job!");
        }
    }
    // Method for save system to get game state
    public GameState GetGameState()
    {
        return new GameState
        {
            IsCompleted = isGameCompleted,
            CurrentFailCount = currentFailCount,
            HasShownCompletion = hasShownCompletion,
            StarCount = starCount
        };
    }


    // Method for save system to restore game state
    public void RestoreGameState(GameState state)
    {
        isGameCompleted = state.IsCompleted;
        currentFailCount = state.CurrentFailCount;
        hasShownCompletion = state.HasShownCompletion;
        
        if (hasShownCompletion)
        {
            ShowStarsBasedOnStarCount();
        }

        // Update UI
        UpdateAttemptsRemainingUI();
        if (stageCompletePanel != null)
        {
            stageCompletePanel.SetActive(hasShownCompletion);
        }
    }

    private void ShowStarsBasedOnStarCount()
    {
        foreach (Transform child in starContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < starCount; i++)
        {
            Instantiate(starIconPrefab, starContainer);
        }
    }
    
    private void ShowStarsBasedOnRemainingHearts()
    {
        // Clean the old stars
        foreach (Transform child in starContainer)
        {
            Destroy(child.gameObject);
        }

        // Calculate stars from the remaining hearts
        int remainingHearts = maxFailAttempts - currentFailCount;
        starCount = remainingHearts; // Store stars in a variable (used for saving or other systems)

        Debug.Log($"[StageManager] Showing {starCount} stars based on {remainingHearts} hearts remaining");

        for (int i = 0; i < starCount; i++)
        {
            Instantiate(starIconPrefab, starContainer);
        }
    }
}
