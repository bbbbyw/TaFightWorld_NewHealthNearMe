using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Core;

public class StageManager : MonoBehaviour
{
    [Header("Stage Configuration")]
    public List<GameObject> storyModeSequence;
    [SerializeField] private float blockWidth = 23f;
    [SerializeField] private Transform prefabSpawnPoint;

    [Header("Stage Settings")]
    [SerializeField] private float transitionDelay = 1f;
    [SerializeField] private bool trackStandaloneChallenges = true;

    [Header("UI References")]
    [SerializeField] private GameObject stageCompletePanel;
    [SerializeField] private TextMeshProUGUI stageCompleteText;
    [SerializeField] private GameObject stageFailPanel;
    [SerializeField] private TextMeshProUGUI attemptsRemainingText;
    [SerializeField] private GameObject jobNotiUI; // Reference to the JobNoti UI in scene

    // Game state tracking
    private bool isGameCompleted = false;
    public bool IsGameCompleted => isGameCompleted;

    // Stage management
    private int currentStageIndex = 0;
    private bool canSpawnNextStage = true;
    private GameObject currentStagePrefab;
    private ChallengeTriggerZone[] challengeZones;

    // Challenge tracking
    private List<ChallengeTriggerZone> standaloneZones = new List<ChallengeTriggerZone>();
    private List<ChallengeTriggerZone> completedStandaloneZones = new List<ChallengeTriggerZone>();
    private int completedChallenges = 0;
    private bool isStageComplete = false;

    // Fail state tracking
    private int currentFailCount = 0;
    private bool hasShownCompletion = false;
    private const int maxFailAttempts = 3;

    private void Start()
    {
        Debug.Log("[StageManager] Starting with maxFailAttempts: " + maxFailAttempts);
        
        if (storyModeSequence == null || storyModeSequence.Count == 0)
        {
            Debug.LogWarning("No story mode sequence assigned - will only track standalone challenges");
        }

        if (stageCompletePanel != null)
        {
            stageCompletePanel.SetActive(false);
        }

        if (stageFailPanel != null)
        {
            stageFailPanel.SetActive(false);
        }

        // Find all standalone challenge zones in the scene
        if (trackStandaloneChallenges)
        {
            var allZones = FindObjectsByType<ChallengeTriggerZone>(FindObjectsSortMode.None);
            foreach (var zone in allZones)
            {
                // If the zone is not part of any story sequence prefab, consider it standalone
                if (!IsPartOfStorySequence(zone.gameObject))
                {
                    standaloneZones.Add(zone);
                    Debug.Log($"[StageManager] Found standalone challenge zone at position: {zone.transform.position}");
                }
            }
        }

        UpdateAttemptsRemainingUI();
    }

    private bool IsPartOfStorySequence(GameObject obj)
    {
        if (storyModeSequence == null) return false;
        
        // Check if this object is a child of any story sequence prefab
        foreach (var prefab in storyModeSequence)
        {
            if (prefab == null) continue;
            
            // Get all ChallengeTriggerZones in the prefab
            var prefabZones = prefab.GetComponentsInChildren<ChallengeTriggerZone>(true);
            foreach (var zone in prefabZones)
            {
                if (zone.gameObject == obj)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OnPrefabSpawnPointReached()
    {
        Debug.Log($"[StageManager] OnPrefabSpawnPointReached - Current fail count: {currentFailCount}, Current stage index: {currentStageIndex}");
        
        if (!canSpawnNextStage) return;
        
        // Check if we have more stages to spawn
        if (currentStageIndex >= storyModeSequence.Count)
        {
            Debug.Log("[StageManager] All story stages have been spawned");
            return;
        }

        // Spawn next stage
        SpawnCurrentStagePrefab();
        currentStageIndex++;
        
        Debug.Log($"[StageManager] Spawned stage {currentStageIndex}/{storyModeSequence.Count}");
    }

    private void SpawnCurrentStagePrefab()
    {
        Debug.Log($"[StageManager] SpawnCurrentStagePrefab - Current fail count before spawn: {currentFailCount}");
        
        if (currentStageIndex >= storyModeSequence.Count)
        {
            Debug.LogWarning("Attempted to spawn stage beyond sequence length!");
            return;
        }

        // Prevent spawning while processing
        canSpawnNextStage = false;

        // Calculate spawn position - always use the same formula
        Vector3 spawnPosition = new Vector3(26.8f + (currentStageIndex * blockWidth), -1.610113f, 0f);

        currentStagePrefab = Instantiate(storyModeSequence[currentStageIndex], spawnPosition, Quaternion.identity);
        
        // Get all challenge zones in the stage
        challengeZones = currentStagePrefab.GetComponentsInChildren<ChallengeTriggerZone>();

        Debug.Log($"[StageManager] Spawned stage {currentStageIndex + 1} with {challengeZones.Length} challenges at position {spawnPosition}. Current fail count after spawn: {currentFailCount}");

        // Allow spawning next stage after this one is spawned
        canSpawnNextStage = true;
    }

    private void UpdateAttemptsRemainingUI()
    {
        if (attemptsRemainingText != null)
        {
            int remainingAttempts = maxFailAttempts - currentFailCount;
            Debug.Log($"[StageManager] Updating UI - Current fail count: {currentFailCount}, Remaining attempts: {remainingAttempts}");
            attemptsRemainingText.text = $"Attempts Remaining: {remainingAttempts}";
        }
    }

    public void OnChallengeSuccess(ChallengeTriggerZone zone)
    {
        // Handle standalone challenges
        if (standaloneZones.Contains(zone))
        {
            Debug.Log($"[StageManager] Standalone challenge completed at position: {zone.transform.position}");
            if (!completedStandaloneZones.Contains(zone))
            {
                completedStandaloneZones.Add(zone);
                Debug.Log($"[StageManager] Total completed standalone challenges: {completedStandaloneZones.Count}/{standaloneZones.Count}");
            }
            HandleChallengeSuccess(zone);
            return;
        }

        // Handle story sequence challenges
        HandleChallengeSuccess(zone);
    }

    private void HandleChallengeSuccess(ChallengeTriggerZone zone)
    {
        // Show challenge success UI from challenge data
        if (zone.challengeData != null)
        {
            GameObject successUI = null;
            if (zone.challengeData.challengeType == ChallengeType.Walk && zone.challengeData.walkSuccessUI != null)
            {
                Vector3 spawnPosition = zone.transform.position + Vector3.up * 2f;
                successUI = Instantiate(zone.challengeData.walkSuccessUI, spawnPosition, Quaternion.identity);
                Destroy(successUI, 2f);
            }
            else if (zone.challengeData.challengeType == ChallengeType.Jump && zone.challengeData.jumpSuccessUI != null)
            {
                Vector3 spawnPosition = zone.transform.position + Vector3.up * 2f;
                successUI = Instantiate(zone.challengeData.jumpSuccessUI, spawnPosition, Quaternion.identity);
                Destroy(successUI, 2f);
            }
            else if (zone.challengeData.challengeType == ChallengeType.TwistBody)
            {
                // Stop player movement for TwistBody challenge
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // Stop auto walk if exists
                    var autoWalk = player.GetComponent<PlayerAutoWalk>();
                    if (autoWalk != null)
                    {
                        Destroy(autoWalk);
                    }
                    
                    // Disable movement through PlayerController
                    var playerController = player.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.EnableMovement(false);
                    }
                }
                
                // For TwistBody, show JobNoti UI first
                if (jobNotiUI != null)
                {
                    jobNotiUI.SetActive(true);
                    var twistUI = jobNotiUI.GetComponent<TwistBodyUI>();
                    if (twistUI != null)
                    {
                        // Pass both the zone and this StageManager to TwistBodyUI
                        twistUI.Initialize(zone, this);
                    }
                }
                else
                {
                    // If no JobNoti UI, just show success UI and complete game
                    if (zone.challengeData.twistSuccessUI != null)
                    {
                        Vector3 spawnPosition = zone.transform.position + Vector3.up * 2f;
                        successUI = Instantiate(zone.challengeData.twistSuccessUI, spawnPosition, Quaternion.identity);
                        Destroy(successUI, 2f);
                    }
                    ShowGameCompletion();
                }
            }
        }
    }

    // New method to be called by TwistBodyUI when story is complete
    public void OnTwistStoryComplete(ChallengeTriggerZone zone)
    {
        if (zone.challengeData != null && zone.challengeData.twistSuccessUI != null)
        {
            Vector3 spawnPosition = zone.transform.position + Vector3.up * 2f;
            GameObject successUI = Instantiate(zone.challengeData.twistSuccessUI, spawnPosition, Quaternion.identity);
            Destroy(successUI, 2f);
        }
        // Show game completion instead of checking stage completion
        ShowGameCompletion();
    }


    public void OnChallengeFail(ChallengeTriggerZone zone)
    {
        // Handle both standalone and story sequence challenges
        currentFailCount++;
        Debug.Log($"[StageManager] Challenge failed at {zone.transform.position}. Type: {zone.challengeData.challengeType}, Current fail count: {currentFailCount}, Max attempts: {maxFailAttempts}");
        Debug.Log($"[StageManager] Challenge is part of prefab: {zone.transform.root.name}");
        
        UpdateAttemptsRemainingUI();

        // Show challenge fail UI from challenge data
        if (zone.challengeData != null)
        {
            GameObject failUI = null;
            if (zone.challengeData.challengeType == ChallengeType.Walk && zone.challengeData.walkFailureUI != null)
            {
                Vector3 spawnPosition = zone.transform.position + Vector3.up * 2f;
                failUI = Instantiate(zone.challengeData.walkFailureUI, spawnPosition, Quaternion.identity);
                Destroy(failUI, 2f);
            }
            else if (zone.challengeData.challengeType == ChallengeType.Jump && zone.challengeData.jumpFailureUI != null)
            {
                Vector3 spawnPosition = zone.transform.position + Vector3.up * 2f;
                failUI = Instantiate(zone.challengeData.jumpFailureUI, spawnPosition, Quaternion.identity);
                Destroy(failUI, 2f);
            }
            else if (zone.challengeData.challengeType == ChallengeType.TwistBody && zone.challengeData.twistFailureUI != null)
            {
                Vector3 spawnPosition = zone.transform.position + Vector3.up * 2f;
                failUI = Instantiate(zone.challengeData.twistFailureUI, spawnPosition, Quaternion.identity);
                Destroy(failUI, 2f);
            }
        }

        // Check if max attempts reached immediately
        if (currentFailCount >= maxFailAttempts)
        {
            Debug.Log("[StageManager] Max fail attempts reached, showing fail panel");
            StartCoroutine(ShowStageFailPanel());
        }
        else
        {
            // Restart the challenge if not max attempts
            Debug.Log($"[StageManager] Restarting challenge - Attempts remaining: {maxFailAttempts - currentFailCount}");
            zone.RestartChallenge();
        }
    }

    private IEnumerator ShowStageFailPanel()
    {
        Debug.Log("[StageManager] Showing stage fail panel");
        if (stageFailPanel != null)
        {
            stageFailPanel.SetActive(true);
            yield return new WaitForSeconds(2f);
            stageFailPanel.SetActive(false);
            RestartStage();
        }
        else
        {
            Debug.LogWarning("Stage fail panel is null!");
            RestartStage();
            yield break;
        }
    }

    private IEnumerator CompleteStage()
    {
        Debug.Log($"[StageManager] CompleteStage - Current fail count before completion: {currentFailCount}");
        isStageComplete = true;
        
        // Wait for transition
        yield return new WaitForSeconds(transitionDelay);

        // Reset stage progress but keep fail count
        Debug.Log($"[StageManager] Preparing for next stage - Keeping fail count at: {currentFailCount}");
        completedChallenges = 0;
        isStageComplete = false;
        Debug.Log("[StageManager] Stage transition complete");
    }

    // New method to show game completion after TwistBody challenge
    public void ShowGameCompletion()
    {
        if (!hasShownCompletion && stageCompletePanel != null)
        {
            hasShownCompletion = true;
            isGameCompleted = true; // Set game completion state
            stageCompletePanel.SetActive(true);
            if (stageCompleteText != null)
            {
                stageCompleteText.text = "Congratulations!\nYou got the job!";
            }
            // Reset fail count when game is complete
            currentFailCount = 0;
            UpdateAttemptsRemainingUI();

            // Log completion for debugging/analytics
            Debug.Log("[StageManager] Game completed! Player got the job!");
        }
    }

    public void RestartStage()
    {
        Debug.Log("[StageManager] Restarting stage - Resetting fail count from: " + currentFailCount);
        currentFailCount = 0;  // Reset fail count only when explicitly restarting stage
        completedChallenges = 0;
        isStageComplete = false;

        // Clean up current stage
        if (currentStagePrefab != null)
        {
            Debug.Log($"[StageManager] Destroying current stage prefab: {currentStagePrefab.name}");
            Destroy(currentStagePrefab);
        }

        // Update UI
        UpdateAttemptsRemainingUI();
        if (stageFailPanel != null)
        {
            stageFailPanel.SetActive(false);
        }

        // Respawn current stage
        SpawnCurrentStagePrefab();
    }

    public void RestartGame()
    {
        Debug.Log("[StageManager] Restarting game - Resetting all progress");
        // Reset all progress
        currentStageIndex = 0;
        completedChallenges = 0;
        currentFailCount = 0;  // Reset fail count when restarting entire game
        isStageComplete = false;
        hasShownCompletion = false;
        isGameCompleted = false; // Reset game completion state
        completedStandaloneZones.Clear();
        
        // Reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Method for save system to get game state
    public GameState GetGameState()
    {
        return new GameState
        {
            IsCompleted = isGameCompleted,
            CurrentFailCount = currentFailCount,
            HasShownCompletion = hasShownCompletion
        };
    }

    // Method for save system to restore game state
    public void RestoreGameState(GameState state)
    {
        isGameCompleted = state.IsCompleted;
        currentFailCount = state.CurrentFailCount;
        hasShownCompletion = state.HasShownCompletion;
        
        // Update UI
        UpdateAttemptsRemainingUI();
        if (stageCompletePanel != null)
        {
            stageCompletePanel.SetActive(hasShownCompletion);
        }
    }
}

// Class to hold save data (can be serialized)
[System.Serializable]
public class GameState
{
    public bool IsCompleted;
    public int CurrentFailCount;
    public bool HasShownCompletion;
} 