using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.SceneManagement;  // Add this for scene management

public class StageManager : MonoBehaviour
{
    [Header("Stage Configuration")]
    [SerializeField] private List<GameObject> storyModeSequence = new List<GameObject>();
    [SerializeField] private List<GameObject> randomPool = new List<GameObject>();
    [SerializeField] private float blockWidth = 23f;
    [SerializeField] private bool isShuffleMode;
    [SerializeField] private Transform prefabSpawnPoint;

    [Header("Stage Management")]
    [SerializeField] private int maxActiveStages = 2;
    [SerializeField] private float destroyDelay = 1f;

    [Header("Stage Settings")]
    public float transitionDelay = 1f;
    public int maxFailAttempts = 3;            // Maximum fails allowed before showing fail panel
    public float stageStartDelay = 1f;
    public float resultPanelDuration = 2f;
    public int currentStageIndex = 0;     // Current stage number
    public bool isLastStage = false;      // Is this the final stage?

    [Header("World Space UI Canvas")]
    public Canvas worldSpaceCanvas;
    public float uiOffset = 2f;
    public Vector3 uiRotation = new Vector3(0, 0, 0);

    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI descriptionText;
    public GameObject successPanel;
    public GameObject failPanel;
    public Button retryButton;
    public Button nextButton;
    public TextMeshProUGUI failCountText; // Add this to show remaining attempts

    [Header("Components")]
    public PoseInputSimulator poseDetector;
    public SpriteRenderer backgroundImage;

    [Header("Challenge Tracking")]
    public List<ChallengeTriggerZone> stageChallengeTriggers;  // All challenges in this stage
    
    private Dictionary<ChallengeTriggerZone, bool> challengeCompletionStatus = new Dictionary<ChallengeTriggerZone, bool>();
    private Dictionary<ChallengeTriggerZone, int> challengeFailAttempts = new Dictionary<ChallengeTriggerZone, int>();
    private List<GameObject> activeStages = new List<GameObject>();
    private System.Random random;
    private GameObject lastSpawnedPrefab;
    private Transform playerTransform;
    private bool isStageActive = false;
    private int totalFailAttempts = 0;
    private Vector3 checkpointPosition;

    private void Start()
    {
        random = new System.Random();
        SpawnInitialStage();

        // Initialize challenge tracking
        foreach (var challenge in stageChallengeTriggers)
        {
            challengeCompletionStatus[challenge] = false;
            challengeFailAttempts[challenge] = 0;
            
            // Subscribe to challenge events
            challenge.onChallengeSuccess.AddListener(() => OnChallengeSuccess(challenge));
            challenge.onChallengeFail.AddListener(() => OnChallengeFail(challenge));
        }

        // Setup event listeners
        poseDetector.onPoseDetected.AddListener(OnPoseDetected);
        nextButton.onClick.AddListener(NextStage);

        // Hide all UI initially
        SetUIVisibility(false);
        
        // Find player if not set
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Set initial checkpoint
        if (playerTransform != null)
            checkpointPosition = playerTransform.position;

        // Start stage with delay
        Invoke("StartStage", stageStartDelay);

        // Update fail count display
        UpdateFailCountDisplay();
    }

    private void Update()
    {
        if (!isStageActive || playerTransform == null) return;

        // Check if player has fallen below a certain point
        if (playerTransform.position.y < -10f)
        {
            OnChallengeFail(null); // Trigger fail when player falls
        }
    }

    private void UpdateFailCountDisplay()
    {
        if (failCountText != null)
        {
            int remainingAttempts = maxFailAttempts - totalFailAttempts;
            failCountText.text = $"Attempts Remaining: {remainingAttempts}";
        }
    }

    private void SpawnInitialStage()
    {
        if (storyModeSequence == null || storyModeSequence.Count == 0)
        {
            Debug.LogError("No stage prefabs assigned to story mode sequence!");
            return;
        }

        GameObject initialStage = Instantiate(
            isShuffleMode ? GetRandomStagePrefab() : storyModeSequence[0],
            prefabSpawnPoint.position,
            Quaternion.identity
        );

        activeStages.Add(initialStage);
        lastSpawnedPrefab = initialStage;
    }

    private void SpawnNextStage()
    {
        if (currentStageIndex >= storyModeSequence.Count && !isShuffleMode) return;

        // Calculate spawn position
        Vector3 spawnPosition;
        if (activeStages.Count > 0)
        {
            // Get the rightmost position of the last spawned stage
            Transform lastStage = activeStages[activeStages.Count - 1].transform;
            Renderer[] renderers = lastStage.GetComponentsInChildren<Renderer>();
            float rightmostPoint = float.MinValue;
            
            foreach (Renderer renderer in renderers)
            {
                float right = renderer.bounds.max.x;
                if (right > rightmostPoint)
                    rightmostPoint = right;
            }
            
            // Add some padding between stages
            spawnPosition = new Vector3(rightmostPoint + 2f, prefabSpawnPoint.position.y, prefabSpawnPoint.position.z);
        }
        else
        {
            // First stage spawns at the spawn point
            spawnPosition = prefabSpawnPoint.position;
        }

        // Spawn the stage
        GameObject stagePrefab;
        if (isShuffleMode)
        {
            stagePrefab = randomPool[Random.Range(0, randomPool.Count)];
        }
        else
        {
            stagePrefab = storyModeSequence[currentStageIndex];
        }

        GameObject newStage = Instantiate(stagePrefab, spawnPosition, Quaternion.identity);
        activeStages.Add(newStage);

        // Clean up old stages if we have too many
        while (activeStages.Count > maxActiveStages)
        {
            GameObject oldestStage = activeStages[0];
            activeStages.RemoveAt(0);
            Destroy(oldestStage, destroyDelay);
        }

        if (!isShuffleMode)
            currentStageIndex++;
    }

    public void ExtendWalkSection()
    {
        // Find the last spawned walk stage prefab
        GameObject walkPrefab = null;
        foreach (var prefab in isShuffleMode ? randomPool : storyModeSequence)
        {
            if (prefab.name.Contains("Walk"))
            {
                walkPrefab = prefab;
                break;
            }
        }

        if (walkPrefab != null)
        {
            // Spawn an additional walk section
            Vector3 spawnPosition = lastSpawnedPrefab.transform.position + Vector3.right * blockWidth;
            GameObject extendedWalk = Instantiate(walkPrefab, spawnPosition, Quaternion.identity);
            activeStages.Add(extendedWalk);
            lastSpawnedPrefab = extendedWalk;

            // Clean up old stages
            while (activeStages.Count > maxActiveStages)
            {
                GameObject oldestStage = activeStages[0];
                activeStages.RemoveAt(0);
                Destroy(oldestStage, destroyDelay);
            }
        }
    }

    private GameObject GetRandomStagePrefab()
    {
        if (randomPool == null || randomPool.Count == 0)
        {
            Debug.LogError("No stage prefabs in random pool!");
            return null;
        }
        return randomPool[random.Next(randomPool.Count)];
    }

    public void StartStage()
    {
        isStageActive = true;
        totalFailAttempts = 0;
        
        // Reset all challenge statuses
        foreach (var challenge in stageChallengeTriggers)
        {
            challengeCompletionStatus[challenge] = false;
            challengeFailAttempts[challenge] = 0;
        }

        SetUIVisibility(true);
        if (successPanel) successPanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
        
        onStageStart?.Invoke();
    }

    private void OnChallengeSuccess(ChallengeTriggerZone challenge)
    {
        // Mark this challenge as completed
        challengeCompletionStatus[challenge] = true;

        // Update checkpoint position
        if (playerTransform != null)
            checkpointPosition = playerTransform.position;

        // Check if all challenges are completed
        bool allChallengesCompleted = true;
        foreach (var status in challengeCompletionStatus.Values)
        {
            if (!status)
            {
                allChallengesCompleted = false;
                break;
            }
        }

        // If all challenges completed, show success panel
        if (allChallengesCompleted)
        {
            ShowStageSuccess();
        }
    }

    private void OnChallengeFail(ChallengeTriggerZone challenge)
    {
        if (challenge != null)
            challengeFailAttempts[challenge]++;
        
        totalFailAttempts++;
        UpdateFailCountDisplay();

        // If total fails reach max, show game over
        if (totalFailAttempts >= maxFailAttempts)
        {
            ShowGameOver();
        }
        else
        {
            // Reset player to checkpoint
            if (playerTransform != null)
            {
                playerTransform.position = checkpointPosition;
                var rb = playerTransform.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = Vector2.zero;
            }
        }
    }

    private void ShowGameOver()
    {
        isStageActive = false;
        SetUIVisibility(false);
        
        if (failPanel)
        {
            failPanel.SetActive(true);
            // Optional: Show game over text or special UI
        }
    }

    public void RetryStage()
    {
        // Only allow retry if we haven't exceeded max attempts
        if (totalFailAttempts < maxFailAttempts)
        {
            HideFailPanel();
            
            // Reset player to checkpoint
            if (playerTransform != null)
            {
                playerTransform.position = checkpointPosition;
                var rb = playerTransform.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.velocity = Vector2.zero;
            }

            StartStage();
        }
        else
        {
            ShowGameOver();
        }
    }

    public void NextStage()
    {
        // Hide the success panel
        if (successPanel) 
            successPanel.SetActive(false);

        if (isLastStage)
        {
            // If this is the last stage, you might want to:
            // 1. Load a victory scene
            // 2. Show final score
            // 3. Return to menu
            Debug.Log("Game Complete!");
            // Example: Load victory scene
            // SceneManager.LoadScene("VictoryScene");
        }
        else
        {
            // Load the next stage
            currentStageIndex++;
            string nextSceneName = "Stage" + (currentStageIndex + 1);
            
            // Check if the next scene exists
            if (SceneUtility.GetBuildIndexByScenePath(nextSceneName) != -1)
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("Next stage scene not found: " + nextSceneName);
                // Fallback to some default behavior
            }
        }
    }

    private void OnPoseDetected(StagePoseType pose)
    {
        if (activeStages.Count == 0) return;

        GameObject currentStage = activeStages[activeStages.Count - 1];
        if (pose == currentStage.GetComponent<Stage>().requiredPose)
        {
            CompleteStage();
        }
    }

    private void CompleteStage()
    {
        if (activeStages.Count == 0) return;

        GameObject currentStage = activeStages[activeStages.Count - 1];
        activeStages.RemoveAt(activeStages.Count - 1);
        Destroy(currentStage, destroyDelay);

        // Show success
        StartCoroutine(ShowSuccess());

        // Notify listeners
        onStageComplete?.Invoke();
    }

    private IEnumerator ShowSuccess()
    {
        successPanel.SetActive(true);

        if (activeStages.Count > 0)
        {
            // Wait for button press or auto-advance
            if (transitionDelay <= 0)
            {
                nextButton.gameObject.SetActive(true);
            }
            else
            {
                yield return new WaitForSeconds(transitionDelay);
                NextStage();
            }
        }
        else
        {
            Debug.Log("Game Complete!");
        }
    }

    private void SetUIVisibility(bool visible)
    {
        if (titleText != null) titleText.gameObject.SetActive(visible);
        if (instructionText != null) instructionText.gameObject.SetActive(visible);
        if (descriptionText != null) descriptionText.gameObject.SetActive(visible);
        if (successPanel != null) successPanel.SetActive(false);
        if (failPanel != null) failPanel.SetActive(false);
    }

    private void HideFailPanel()
    {
        if (failPanel != null)
            failPanel.SetActive(false);
    }

    private void HideSuccessPanel()
    {
        if (successPanel != null)
            successPanel.SetActive(false);
    }

    private void ShowStageSuccess()
    {
        isStageActive = false;
        SetUIVisibility(false);
        
        if (successPanel)
        {
            successPanel.SetActive(true);
            // Optional: Auto-hide if no next button
            if (nextButton == null)
                Invoke("HideSuccessPanel", resultPanelDuration);
        }
        
        onStageComplete?.Invoke();
    }

    // Events that other scripts can listen to
    public UnityEvent onStageStart;
    public UnityEvent onStageComplete;
} 