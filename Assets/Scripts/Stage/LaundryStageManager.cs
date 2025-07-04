using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LaundryStageManager : MonoBehaviour
{
    [Header("Stage Data")]
    public LaundryStageData stageData;

    [Header("Challenge Objects")]
    public LaundryChallengeObject pickupClothes;  // ชุด object สำหรับหยิบผ้า
    public LaundryChallengeObject wringClothes;   // ชุด object สำหรับบิดผ้า
    public LaundryChallengeObject hangClothes;    // ชุด object สำหรับตากผ้า

    [Header("UI")]
    public TextMeshProUGUI challengeText;
    public GameObject gameOverPanel;
    public GameObject successPanel;
    public Button restartButton;        // ปุ่ม Restart
    public Button NextStageButton;       // ปุ่มสำหรับ Win Panel
    public string nextSceneName;
    public Button retryButton;          // ปุ่มสำหรับ Retry Pose
    public TransitionManager transitionManager;

    [Header("Events")]
    public UnityEvent onStageComplete;
    public UnityEvent onGameOver;

    private int currentStepIndex = 0;
    private int failCount = 0;
    private LaundryChallengeObject currentChallengeObject;

    [Header("Pose System")]
    public PoseGameManager poseGameManager;
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

    private bool isStepFinished = false;
    private bool waitingForRetry = false;

    private void Start()
    {
        if (stageData == null)
        {
            Debug.LogError("Stage data is missing!");
            return;
        }

        // ซ่อนทุก challenge object ตอนเริ่มต้น
        pickupClothes.Hide();
        wringClothes.Hide();
        hangClothes.Hide();

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (successPanel) successPanel.SetActive(false);

        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        if (NextStageButton) NextStageButton.onClick.AddListener(OnNextStageButtonClicked);

        UpdateHeartUI();

        SetupCurrentStep();
    }

    private void SetupCurrentStep()
    {
        isStepFinished = false;
        waitingForRetry = false;

        if (currentStepIndex < 0 || currentStepIndex >= stageData.steps.Length)
        {
            Debug.LogError("Invalid step index!");
            return;
        }

        // ซ่อน challenge object เก่า
        if (currentChallengeObject != null)
        {
            currentChallengeObject.onAnimationComplete.RemoveListener(OnStepComplete);
            currentChallengeObject.Hide();
        }

        // ตั้งค่า challenge object ใหม่ตาม action type
        var currentStep = stageData.steps[currentStepIndex];
        switch (currentStep.actionType)
        {
            case LaundryActionType.PickupClothes:
                currentChallengeObject = pickupClothes;
                break;
            case LaundryActionType.WringClothes:
                currentChallengeObject = wringClothes;
                break;
            case LaundryActionType.HangClothes:
                currentChallengeObject = hangClothes;
                break;
            default:
                Debug.LogError($"Unsupported action type: {currentStep.actionType}");
                return;
        }

        // Initialize และตั้งค่า animation duration
        currentChallengeObject.Initialize(this);
        currentChallengeObject.additionalHoldTime = currentStep.additionalHoldTime;

        // Subscribe to animation complete event
        currentChallengeObject.onAnimationComplete.AddListener(OnStepComplete);

        // แสดง challenge object และ text
        currentChallengeObject.Show();
        if (challengeText != null)
        {
            challengeText.text = currentStep.challengeText;
        }

        Debug.Log($"[LaundryStage] Starting pose check for step: {currentStep.challengeText}");

        if (currentStep.poseRequirements == null || currentStep.poseRequirements.Count == 0)
        {
            Debug.LogError("[LaundryStage] No PoseRequirements in this step!");
            return;
        }

        poseGameManager.PlayPoseExternal(
            currentStep.poseRequirements,
            success =>
            {
                if (success)
                {
                    OnStepSuccess();
                }
                else
                {
                    OnStepFail();
                }
            });
    }

    private void Update()
    {
        // ตรวจสอบเงื่อนไขพื้นฐาน
        if (stageData == null || currentChallengeObject == null) return;

        // ตรวจสอบว่า currentStepIndex ไม่เกินขอบเขตของ array
        if (currentStepIndex < 0 || currentStepIndex >= stageData.steps.Length)
        {
            // ถ้าเกินขอบเขต แสดงว่าทำ challenge ครบหมดแล้ว
            return;
        }

        var currentStep = stageData.steps[currentStepIndex];

        // Debug controls
        if (Input.GetKeyDown(currentStep.successKey))
        {
            OnStepSuccess();
        }
        else if (Input.GetKeyDown(currentStep.failKey))
        {
            OnStepFail();
        }
    }

    private void OnStepSuccess()
    {
        if (isStepFinished && !waitingForRetry) return;
        isStepFinished = true;

        Debug.Log($"[LaundryStageManager] Step {currentStepIndex} completed successfully!");
        currentChallengeObject.PlayAnimation("Success");
        // OnStepComplete จะถูกเรียกโดยอัตโนมัติหลังจาก animation เล่นจบ
    }

    private void OnStepComplete()
    {
        Debug.Log($"[LaundryStageManager] OnStepComplete called at step {currentStepIndex}");

        // Unsubscribe from the event to prevent memory leaks
        if (currentChallengeObject != null)
        {
            currentChallengeObject.onAnimationComplete.RemoveListener(OnStepComplete);
        }

        waitingForRetry = false;

        currentStepIndex++;

        // ตรวจสอบว่าทำ challenge ครบทุกข้อหรือยัง
        if (currentStepIndex >= stageData.steps.Length)
        {
            // Stage complete
            Debug.Log("[LaundryStageManager] All challenges completed! Stage complete!");
            currentChallengeObject = null; // เคลียร์ reference เพื่อป้องกัน error

            ShowStarsBasedOnRemainingHearts();

            if (bgmAudioSource) bgmAudioSource.Stop();
            if (successPanel) successPanel.SetActive(true);

            if (sfxAudioSource && winSound)
                sfxAudioSource.PlayOneShot(winSound);

            onStageComplete.Invoke();
        }
        else
        {
            SetupCurrentStep();
        }
    }

    private void OnStepFail()
    {
        if (isStepFinished) return;
        isStepFinished = true;

        Debug.Log($"[LaundryStageManager] Challenge failed! Current fail count: {failCount + 1}");

        // ลบ listener OnStepComplete ออกก่อน เพื่อไม่ให้ trigger step ถัดไปตอน animation fail จบ
        currentChallengeObject.onAnimationComplete.RemoveListener(OnStepComplete);
        currentChallengeObject.PlayAnimation("Fail");

        failCount++;
        UpdateHeartUI();

        Debug.Log($"[LaundryStageManager] Remaining attempts: {stageData.maxFailAttempts - failCount}");

        if (failCount >= stageData.maxFailAttempts)
        {
            // เกินจำนวนครั้งที่กำหนด แสดงเกมโอเวอร์
            Debug.Log("[LaundryStageManager] Max attempts reached! Showing game over panel");
            if (resultPanel) resultPanel.SetActive(false);
            if (PoseIconResult) PoseIconResult.SetActive(false);
            if (blackFilter) blackFilter.SetActive(false);

            if (bgmAudioSource) bgmAudioSource.Stop();
            if (gameOverPanel) gameOverPanel.SetActive(true);

            if (sfxAudioSource && gameOverSound)
                sfxAudioSource.PlayOneShot(gameOverSound);

            onGameOver.Invoke();
        }
        else
        {
            Debug.Log("[LaundryStageManager] Challenge can be retried. Restarting current step.");

            waitingForRetry = true;
        }
    }

    private void OnPlayerRetry()
    {
        if (!waitingForRetry)
        {
            Debug.LogWarning("[LaundryStageManager] Retry called but not waiting for retry.");
            return;
        }

        Debug.Log("[LaundryStageManager] Player requested retry, restarting current step.");

        waitingForRetry = false;
        isStepFinished = false;

        SetupCurrentStep();
    }

    private void UpdateHeartUI()
    {
        foreach (Transform child in heartContainer)
            Destroy(child.gameObject);

        int remaining = stageData.maxFailAttempts - failCount;

        for (int i = 0; i < remaining; i++)
            Instantiate(heartIconPrefab, heartContainer);
    }

    private void ShowStarsBasedOnRemainingHearts()
    {
        foreach (Transform child in starContainer)
            Destroy(child.gameObject);

        int stars = stageData.maxFailAttempts - failCount;

        for (int i = 0; i < stars; i++)
            Instantiate(starIconPrefab, starContainer);
    }

    public void RestartGame()
    {
        Debug.Log("[LaundryStageManager] Restarting game!");
        currentStepIndex = 0;
        failCount = 0;
        if (bgmAudioSource)
        {
            bgmAudioSource.Stop();
            bgmAudioSource.Play();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void PoseStepSuccess()
    {
        Debug.Log("[LaundryStageManager] PoseStepSuccess called!");
        OnStepSuccess();
    }

    public void PoseStepFail()
    {
        Debug.Log("[LaundryStageManager] PoseStepFail called!");
        OnStepFail();
    }
    
    private void OnNextStageButtonClicked()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("[LaundryStageManager] Loading next scene: " + nextSceneName);

            if (bgmAudioSource)
                bgmAudioSource.Stop();

            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[LaundryStageManager] Next scene name is empty or null.");
        }
    }
} 