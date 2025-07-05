using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using TMPro;
using Core;

[DefaultExecutionOrder(-1)] // ให้ทำงานก่อน script อื่นๆ
public class ShuffleModeManager : MonoBehaviour
{
    public static ShuffleModeManager Instance { get; private set; }
    public static bool IsShuffleMode => Instance != null;

    [Header("Configuration")]
    [SerializeField] private ShuffleModeConfig config;
    [SerializeField] private GameObject shuffleUIPrefab;
    [SerializeField] private GameObject shufflePanelPrefab;

    [Header("UI Settings")]
    [SerializeField] private Vector2 uiOffset = new Vector2(0.02f, -0.17f);
    [SerializeField] private float shufflePanelDuration = 2f;
    

    private List<string> remainingStages;
    private string currentStage;
    private bool isWaitingForStageComplete = false;
    private GameObject shuffleUI;
    private GameObject shufflePanel;
    private bool isStageFailed = false;
    private int totalScore = 0; // เก็บคะแนนรวมจากทุก stage ที่ผ่าน
    private bool hasAddedScore = false; // Flag เพื่อป้องกันการเพิ่ม score ซ้ำ
    private bool isLoadingNextStage = false; // Flag เพื่อป้องกันการโหลด stage ซ้ำ

    // References found at runtime
    private GameObject shuffleFail;
    private TextMeshProUGUI totalScoreText;

    // Constants for max attempts per stage type
    private const int DOG_STAGE_MAX_ATTEMPTS = 3;
    private const int LAUNDRY_STAGE_MAX_ATTEMPTS = 3;
    private const int GARDEN_STAGE_MAX_ATTEMPTS = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeManager();
            InitializeUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManager()
    {
        if (config == null)
        {
            Debug.LogError("[ShuffleModeManager] Config is not assigned!");
            return;
        }

        remainingStages = new List<string>(config.stageScenes);
        ShuffleStages();

        if (remainingStages.Count > 0)
        {
            LoadNextStage();
        }
    }

    private void InitializeUI()
    {
        // สร้าง ShuffleUI
        if (shuffleUIPrefab != null && shuffleUI == null)
        {
            Debug.Log("Instantiating ShuffleUI prefab");
            shuffleUI = Instantiate(shuffleUIPrefab);
            DontDestroyOnLoad(shuffleUI);
            UpdateShuffleUIPosition();

            // Find shuffleFail and score text references
            shuffleFail = shuffleUI.transform.Find("ShuffleFail")?.gameObject;
            if (shuffleFail != null)
            {
                totalScoreText = shuffleFail.GetComponentInChildren<TextMeshProUGUI>();
                shuffleFail.SetActive(false); // Make sure it's disabled at start
                Debug.Log("ShuffleFail found in ShuffleUI prefab!");
            }
            else
            {
                Debug.LogError("[ShuffleModeManager] ShuffleFail not found in ShuffleUI prefab!");
            }
        }

        // สร้าง ShufflePanel และ DontDestroyOnLoad
        if (shufflePanelPrefab != null && shufflePanel == null)
        {
            shufflePanel = Instantiate(shufflePanelPrefab);
            DontDestroyOnLoad(shufflePanel);
            
            // ตั้งค่า RectTransform
            var rectTransform = shufflePanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            // ตั้งค่า Canvas
            var canvas = shufflePanel.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = 999;
            }

            // ปิดไว้ตอนเริ่มต้น
            shufflePanel.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[ShuffleModeManager] Scene loaded: {scene.name}");
        
        // Reset flags when loading new scene
        hasAddedScore = false;
        isLoadingNextStage = false;
        
        // ปิด ShufflePanel เมื่อโหลดฉากใหม่
        if (shufflePanel != null)
        {
            shufflePanel.SetActive(false);
        }
        
        StartCoroutine(InitializeStage());
    }

    private IEnumerator InitializeStage()
    {
        yield return new WaitForSeconds(0.1f);
        ConnectCurrentStage();
    }

    private void ConnectCurrentStage()
    {
        Debug.Log("[ShuffleModeManager] Connecting to stage...");
        
        var dogGameManager = FindFirstObjectByType<DogChaseGameManager>();
        if (dogGameManager != null)
        {
            Debug.Log("[ShuffleModeManager] Found DogChaseGameManager");
            StartCoroutine(WaitForDogStageCompletion(dogGameManager));
            return;
        }

        var stageManager = FindFirstObjectByType<StageManager>();
        if (stageManager != null)
        {
            Debug.Log("[ShuffleModeManager] Found StageManager");
            StartCoroutine(WaitForGardenStageCompletion(stageManager));
            return;
        }

        var laundryStage = FindFirstObjectByType<LaundryStageManager>();
        if (laundryStage != null)
        {
            Debug.Log("[ShuffleModeManager] Found LaundryStageManager");
            StartCoroutine(WaitForLaundryStageCompletion(laundryStage));
            return;
        }

        Debug.LogWarning("[ShuffleModeManager] No stage manager found!");
    }

    private void HideSuccessPanel()
    {
        var successPanel = GameObject.Find("SuccessPanel");
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }
    }

    private void OnStageFail()
    {
        Debug.Log($"[ShuffleModeManager] Stage failed - Total score: {totalScore}");
        isStageFailed = true;
        
        // แสดง total score ใน fail panel
        
        
        // แสดง score ใน shuffleFail
        if (shuffleFail != null)
        {
            Debug.Log("Setting shuffleFail to active");
            shuffleFail.SetActive(true);
            if (totalScoreText != null)
            {
                totalScoreText.text = $"Total Score: {totalScore}";
                Debug.Log($"totalScoreText: {totalScoreText.text}");
                totalScoreText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogError("[ShuffleModeManager] Score text not found in shuffleFail!");
            }
        }
        else
        {
            Debug.LogError("[ShuffleModeManager] ShuffleFail is null!");
        }
        
        // ปิด ShufflePanel ถ้ามี
        if (shufflePanel != null)
        {
            shufflePanel.SetActive(false);
        }
        
        // ทำลายตัวเองเพื่อจบ shuffle mode
        Destroy(gameObject);
    }

    private IEnumerator WaitForDogStageCompletion(DogChaseGameManager gameManager)
    {
        Debug.Log("[ShuffleModeManager] Starting to wait for dog stage completion");
        bool isCompleted = false;
        isWaitingForStageComplete = true;

        int startStageIndex = gameManager.currentStageIndex;
        bool isTransitioning = false;

        while (!isCompleted && isWaitingForStageComplete && !isStageFailed)
        {
            // ตรวจสอบ fail panel
            var currentFailedPanel = GameObject.Find("FailedPanel");
            if (currentFailedPanel != null && currentFailedPanel.activeSelf)
            {
                OnStageFail();
                yield break;
            }

            if (gameManager.currentStageIndex > startStageIndex && !isTransitioning)
            {
                isTransitioning = true;
                yield return new WaitForSeconds(gameManager.transitionDelay);
                startStageIndex = gameManager.currentStageIndex;
                isTransitioning = false;
            }

            if (gameManager.currentStageIndex >= gameManager.stages.Length - 1)
            {
                var currentStage = gameManager.stages[gameManager.currentStageIndex];
                if (currentStage.stageController != null && !currentStage.stageController.IsTransitioning)
                {
                    Debug.Log("[ShuffleModeManager] Dog stage completed");
                    isCompleted = true;
                    
                    // คำนวณ score จาก remaining attempts โดยใช้ totalFailCount จาก DogChaseGameManager
                    if (!hasAddedScore)
                    {
                        int remainingAttempts = DOG_STAGE_MAX_ATTEMPTS - gameManager.totalFailCount;
                        totalScore += remainingAttempts;
                        Debug.Log($"[ShuffleModeManager] Added {remainingAttempts} points from Dog stage (max:{DOG_STAGE_MAX_ATTEMPTS} - fails:{gameManager.totalFailCount}). Total score: {totalScore}");
                        hasAddedScore = true;
                    }
                    
                    HideSuccessPanel();
                    yield return StartCoroutine(ShowShufflePanel());
                }
            }

            yield return null;
        }

        if (isCompleted && !isStageFailed && !isLoadingNextStage)
        {
            LoadNextStage();
        }
    }

    private IEnumerator WaitForGardenStageCompletion(StageManager stageManager)
    {
        Debug.Log("[ShuffleModeManager] Starting to wait for garden stage completion");
        bool isCompleted = false;
        
        while (!isCompleted && !isStageFailed)
        {
            // ตรวจสอบ fail panel
            var currentFailedPanel = GameObject.Find("FailedPanel");
            if (currentFailedPanel != null && currentFailedPanel.activeSelf)
            {
                OnStageFail();
                yield break;
            }

            // เช็คว่า stage complete หรือยัง
            if (stageManager.IsGameCompleted)
            {
                Debug.Log("[ShuffleModeManager] Garden stage completed via IsGameCompleted");
                isCompleted = true;
            }
            else
            {
                // เช็คการ complete ผ่าน position ของ player
                var challengeObject = FindFirstObjectByType<ChallengeTriggerZone>();
                if (challengeObject != null && challengeObject.IsCompleted)
                {
                    var player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null)
                    {
                        float lastPrefabX = 26.8f + ((stageManager.storyModeSequence.Count - 1) * 23f);
                        
                        if (player.transform.position.x >= lastPrefabX)
                        {
                            Debug.Log("[ShuffleModeManager] Garden stage completed via player position");
                            isCompleted = true;
                        }
                    }
                }
            }

            // ถ้า stage complete ให้เพิ่ม score
            if (isCompleted && !hasAddedScore)
            {
                var gameState = stageManager.GetGameState();
                int remainingAttempts = GARDEN_STAGE_MAX_ATTEMPTS - gameState.CurrentFailCount;
                totalScore += remainingAttempts;
                Debug.Log($"[ShuffleModeManager] Added {remainingAttempts} points from Garden stage (max:{GARDEN_STAGE_MAX_ATTEMPTS} - fails:{gameState.CurrentFailCount}). Total score: {totalScore}");
                hasAddedScore = true;
                
                HideSuccessPanel();
                yield return StartCoroutine(ShowShufflePanel());
                if (!isLoadingNextStage)
                {
                    LoadNextStage();
                }
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator WaitForLaundryStageCompletion(LaundryStageManager stage)
    {
        Debug.Log("[ShuffleModeManager] Starting to wait for laundry stage completion");
        bool isCompleted = false;

        UnityAction onComplete = () => {
            Debug.Log("[ShuffleModeManager] Laundry stage completed");
            isCompleted = true;
        };

        stage.onStageComplete.AddListener(onComplete);

        while (!isCompleted && !isStageFailed)
        {
            // ตรวจสอบ fail panel
            var currentFailedPanel = GameObject.Find("FailedPanel");
            if (currentFailedPanel != null && currentFailedPanel.activeSelf)
            {
                OnStageFail();
                yield break;
            }

            yield return null;
        }

        stage.onStageComplete.RemoveListener(onComplete);

        if (isCompleted && !isStageFailed)
        {
            // คำนวณ score จาก remaining attempts โดยใช้ failCount จาก LaundryStageManager
            if (!hasAddedScore)
            {
                int remainingAttempts = LAUNDRY_STAGE_MAX_ATTEMPTS - stage.failCount;
                totalScore += remainingAttempts;
                Debug.Log($"[ShuffleModeManager] Added {remainingAttempts} points from Laundry stage (max:{LAUNDRY_STAGE_MAX_ATTEMPTS} - fails:{stage.failCount}). Total score: {totalScore}");
                hasAddedScore = true;
            }
            
            HideSuccessPanel();
            yield return StartCoroutine(ShowShufflePanel());
            if (!isLoadingNextStage)
            {
                LoadNextStage();
            }
        }
    }

    private IEnumerator ShowShufflePanel()
    {
        Debug.Log("[ShuffleModeManager] Showing ShufflePanel");
        
        if (shufflePanel != null)
        {
            // แสดง ShufflePanel
            shufflePanel.SetActive(true);
            
            // รอตามเวลาที่กำหนด
            yield return new WaitForSeconds(shufflePanelDuration);
        }
        else
        {
            Debug.LogError("[ShuffleModeManager] ShufflePanel is null!");
        }
    }

    private void ShuffleStages()
    {
        int n = remainingStages.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            string temp = remainingStages[k];
            remainingStages[k] = remainingStages[n];
            remainingStages[n] = temp;
        }
    }

    private void LoadNextStage()
    {
        Debug.Log("[ShuffleModeManager] Loading next stage");
        if (isLoadingNextStage)
        {
            Debug.Log("[ShuffleModeManager] Already loading next stage, skipping...");
            return;
        }

        isLoadingNextStage = true;

        if (remainingStages.Count == 0)
        {
            remainingStages = new List<string>(config.stageScenes);
            ShuffleStages();
        }

        currentStage = remainingStages[0];
        remainingStages.RemoveAt(0);
        Debug.Log($"[ShuffleModeManager] Loading stage: {currentStage}");
        SceneManager.LoadScene(currentStage);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            if (shuffleUI != null)
            {
                // Make sure shuffleFail is disabled before destroying
                Debug.Log("Destroying shuffleUI");
            }
            if (shufflePanel != null)
            {
                Destroy(shufflePanel);
            }
            Instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        if (shuffleUI != null && Camera.main != null)
        {
            UpdateShuffleUIPosition();
        }
    }

    private void UpdateShuffleUIPosition()
    {
        if (shuffleUI != null && Camera.main != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 targetPosition = new Vector3(
                cameraPosition.x + uiOffset.x,
                cameraPosition.y + uiOffset.y,
                shuffleUI.transform.position.z
            );
            shuffleUI.transform.position = targetPosition;
        }
    }
} 