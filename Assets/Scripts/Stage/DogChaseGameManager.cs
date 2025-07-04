using UnityEngine;
using System.Collections;
using UnityEngine.UI; // เพิ่มเพื่อใช้งาน UI
using UnityEngine.SceneManagement;

public class DogChaseGameManager : MonoBehaviour
{
    [System.Serializable]
    public class StageSetup
    {
        public GameObject stageRoot;           // Root GameObject ของแต่ละ stage
        public DogChaseStageController stageController;
        public DogChaseChallenge challenge;
    }

    [Header("Stage Setup")]
    public StageSetup[] stages;               // Array ของทุก stages
    public float transitionDelay = 2f;        // เวลารอระหว่าง stages

    [Header("Challenge Settings")]
    public int maxTotalAttempts = 3;  // จำนวนครั้งที่อนุญาตให้ fail รวมทั้งหมด
    public int totalFailCount { get; private set; } = 0;    // จำนวนครั้งที่ fail รวมทั้งหมด

    [Header("UI References")]
    public GameObject gameOverPanel;    // Panel แสดง Game Over
    public GameObject successPanel;     // เพิ่ม success panel
    public Button restartButton;        // ปุ่ม Restart
    public Button continueButton;       // ปุ่มสำหรับ success panel
    public Button retryButton;          // ปุ่มสำหรับ Retry Pose

    [Header("Game State")]
    public int currentStageIndex = 0;         // stage ปัจจุบัน
    private bool isTransitioning = false;

    [Header("Pose System")]
    public GameObject resultPanel;
    public GameObject PoseIconResult;
    public GameObject blackFilter;

    [Header("Life System")]
    public GameObject heartIconPrefab;
    public Transform heartContainer;

    [Header("Star System")]
    public GameObject starIconPrefab;
    public Transform starContainer;

    [Header("Audio")]
    public AudioSource bgmAudioSource;
    public AudioSource sfxAudioSource;
    public AudioClip winSound;
    public AudioClip gameOverSound;
    public AudioClip DogBarkSFX;

    private int starCount = 0;

    private void Start()
    {
        // Setup stage references
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].stageRoot != null)
            {
                // Get or add stage controller
                stages[i].stageController = stages[i].stageRoot.GetComponentInChildren<DogChaseStageController>();
                if (stages[i].stageController == null)
                {
                    Debug.LogError($"Stage {i} is missing DogChaseStageController!");
                }

                // Get or add challenge
                stages[i].challenge = stages[i].stageRoot.GetComponentInChildren<DogChaseChallenge>();
                if (stages[i].challenge == null)
                {
                    Debug.LogError($"Stage {i} is missing DogChaseChallenge!");
                }

                // Activate only the first stage
                stages[i].stageRoot.SetActive(i == currentStageIndex);
            }
        }

        // Setup UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(() =>
            {
                Debug.Log("[DogChase] Retry button pressed!");
                if (currentStageIndex >= 0 && currentStageIndex < stages.Length)
                {
                    if (stages[currentStageIndex].challenge != null)
                    {
                        stages[currentStageIndex].challenge.RestartChallenge();
                    }
                    else
                    {
                        Debug.LogWarning("[DogChase] No challenge found for current stage.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DogChase] Invalid currentStageIndex: {currentStageIndex}");
                }
            });
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(RestartGame); // ใช้ RestartGame เหมือนกัน
        }

        if (sfxAudioSource != null && DogBarkSFX != null)
        {
                sfxAudioSource.PlayOneShot(DogBarkSFX);
        }

        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.loop = true;
            bgmAudioSource.Play();
        }

        // Reset fail count
        totalFailCount = 0;
        UpdateHeartUI();

        // Start the first stage
        if (stages.Length > 0 && stages[0].stageController != null)
        {
            stages[0].stageController.StartDogChase();
        }
    }

    private void UpdateHeartUI()
    {
        int remainingAttempts = maxTotalAttempts - totalFailCount;

        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < remainingAttempts; i++)
        {
            Instantiate(heartIconPrefab, heartContainer);
        }
    }

    private void ShowStarsBasedOnRemainingHearts()
    {
        foreach (Transform child in starContainer)
        {
            Destroy(child.gameObject);
        }

        int remainingHearts = maxTotalAttempts - totalFailCount;
        starCount = remainingHearts;

        for (int i = 0; i < starCount; i++)
        {
            Instantiate(starIconPrefab, starContainer);
        }
    }

    public void OnStageComplete(bool success)
    {
        if (isTransitioning) return;

        if (success)
        {
            // ถ้าเป็น stage สุดท้าย จบเกม
            if (currentStageIndex >= stages.Length - 1)
            {
                Debug.Log("Game Complete!");
                StartCoroutine(DelayShowGameComplete());
                return;
            }

            // เปลี่ยนไป stage ถัดไป
            StartCoroutine(TransitionToNextStage());
        }
        else
        {
            // แสดง Game Over UI
            ShowGameOver();
        }
    }

    public void OnChallengeFail()
    {
        totalFailCount++;
        Debug.Log($"[GameManager] Total fails: {totalFailCount}/{maxTotalAttempts}");

        UpdateHeartUI();

        if (totalFailCount >= maxTotalAttempts)
        {
            if (resultPanel != null && resultPanel.activeSelf)
            {
                resultPanel.SetActive(false);
                blackFilter.gameObject.SetActive(false);
                PoseIconResult.gameObject.SetActive(false);
                Debug.Log("[StageManager] resultPanel inactivation!");
            }
            ShowGameOver();
        }
    }

    public bool CanRetryChallenge()
    {
        return totalFailCount < maxTotalAttempts;
    }

    public void ShowGameOver()
    {
        Debug.Log("Game Over - Too many fails!");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (successPanel != null)
            {
                successPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Game Over Panel is not assigned!");
        }

        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        if (sfxAudioSource != null && gameOverSound != null)
        {
            sfxAudioSource.PlayOneShot(gameOverSound);
        }
    }

    private void ShowGameComplete()
    {
        Debug.Log("Congratulations! Game Complete!");
        if (successPanel != null)
        {
            successPanel.SetActive(true);
            ShowStarsBasedOnRemainingHearts();
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Success Panel is not assigned!");
        }

        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        if (sfxAudioSource != null && winSound != null)
        {
            sfxAudioSource.PlayOneShot(winSound);
        }
    }

    public void RestartGame()
    {
        // ซ่อน UI ทั้งหมด
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        if (successPanel != null)
        {
            successPanel.SetActive(false);
        }

        // Reset game state
        currentStageIndex = 0;
        isTransitioning = false;
        totalFailCount = 0;  // Reset total fail count
        starCount = 0;
        UpdateHeartUI();

        // ปิดทุก stage
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].stageRoot != null)
            {
                stages[i].stageRoot.SetActive(false);
            }
        }

        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator TransitionToNextStage()
    {
        isTransitioning = true;

        // รอให้ animation หรือ effect ต่างๆ จบ
        yield return new WaitForSeconds(transitionDelay);

        // ปิด stage ปัจจุบัน
        if (stages[currentStageIndex].stageRoot != null)
        {
            stages[currentStageIndex].stageRoot.SetActive(false);
        }

        // เปิด stage ถัดไป
        currentStageIndex++;
        if (currentStageIndex < stages.Length && stages[currentStageIndex].stageRoot != null)
        {
            stages[currentStageIndex].stageRoot.SetActive(true);
            // เริ่ม stage ใหม่
            if (stages[currentStageIndex].stageController != null)
            {
                stages[currentStageIndex].stageController.StartDogChase();
            }
        }

        isTransitioning = false;
    }
    
    private IEnumerator DelayShowGameComplete()
    {
        yield return new WaitForSeconds(1.5f);
        ShowGameComplete();
    }
} 