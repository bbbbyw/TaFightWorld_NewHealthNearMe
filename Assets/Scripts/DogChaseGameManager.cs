using UnityEngine;
using System.Collections;
using UnityEngine.UI; // เพิ่มเพื่อใช้งาน UI

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

    [Header("Game State")]
    public int currentStageIndex = 0;         // stage ปัจจุบัน
    private bool isTransitioning = false;

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
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(RestartGame); // ใช้ RestartGame เหมือนกัน
        }

        // Reset fail count
        totalFailCount = 0;

        // Start the first stage
        if (stages.Length > 0 && stages[0].stageController != null)
        {
            stages[0].stageController.StartDogChase();
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
                ShowGameComplete();
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

        if (totalFailCount >= maxTotalAttempts)
        {
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
    }

    private void ShowGameComplete()
    {
        Debug.Log("Congratulations! Game Complete!");
        if (successPanel != null)
        {
            successPanel.SetActive(true);
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogError("Success Panel is not assigned!");
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

        // ปิดทุก stage
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].stageRoot != null)
            {
                stages[i].stageRoot.SetActive(false);
            }
        }

        // เปิด stage แรก
        if (stages.Length > 0)
        {
            stages[0].stageRoot.SetActive(true);
            if (stages[0].stageController != null)
            {
                stages[0].stageController.StartDogChase();
            }
        }
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
} 