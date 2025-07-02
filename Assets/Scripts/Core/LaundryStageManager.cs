using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Collections;

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
    public TransitionManager transitionManager;
    public TextMeshProUGUI attemptsRemainingText;

    [Header("Events")]
    public UnityEvent onStageComplete;
    public UnityEvent onGameOver;

    private int currentStepIndex = 0;
    private int failCount = 0;
    private LaundryChallengeObject currentChallengeObject;

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

        // แสดงและอัพเดท attempts remaining ตั้งแต่เริ่มเกม
        UpdateAttemptsRemainingUI();

        SetupCurrentStep();
    }

    private void SetupCurrentStep()
    {
        if (currentStepIndex < 0 || currentStepIndex >= stageData.steps.Length)
        {
            Debug.LogError("Invalid step index!");
            return;
        }

        // ซ่อน challenge object เก่า
        if (currentChallengeObject != null)
        {
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
        Debug.Log($"[LaundryStageManager] Step {currentStepIndex} completed successfully!");
        currentChallengeObject.PlayAnimation("Success");
        // OnStepComplete จะถูกเรียกโดยอัตโนมัติหลังจาก animation เล่นจบ
    }

    private void OnStepComplete()
    {
        // Unsubscribe from the event to prevent memory leaks
        if (currentChallengeObject != null)
        {
            currentChallengeObject.onAnimationComplete.RemoveListener(OnStepComplete);
        }

        currentStepIndex++;
        
        // ตรวจสอบว่าทำ challenge ครบทุกข้อหรือยัง
        if (currentStepIndex >= stageData.steps.Length)
        {
            // Stage complete
            Debug.Log("[LaundryStageManager] All challenges completed! Stage complete!");
            currentChallengeObject = null; // เคลียร์ reference เพื่อป้องกัน error
            successPanel.SetActive(true);
            onStageComplete.Invoke();
        }
        else
        {
            SetupCurrentStep();
        }
    }

    private void OnStepFail()
    {
        Debug.Log($"[LaundryStageManager] Challenge failed! Current fail count: {failCount + 1}");
        
        currentChallengeObject.PlayAnimation("Fail");

        failCount++;
        
        // อัพเดทจำนวน attempts ที่เหลือ
        UpdateAttemptsRemainingUI();

        Debug.Log($"[LaundryStageManager] Remaining attempts: {stageData.maxFailAttempts - failCount}");

        // ถ้าเกิน max attempts ถึงจะแสดง game over
        if (failCount >= stageData.maxFailAttempts)
        {
            Debug.Log("[LaundryStageManager] Max attempts reached! Showing game over panel");
            gameOverPanel.SetActive(true);
            onGameOver.Invoke();
        }
        else
        {
            Debug.Log($"[LaundryStageManager] Challenge can be retried. Current challenge type: {currentChallengeObject.GetType().Name}");
        }
    }

    private void UpdateAttemptsRemainingUI()
    {
        if (attemptsRemainingText != null)
        {
            int remainingAttempts = stageData.maxFailAttempts - failCount;
            attemptsRemainingText.text = $"Attempts Remaining: {remainingAttempts}";
            attemptsRemainingText.gameObject.SetActive(true);
            Debug.Log($"[LaundryStageManager] Updated UI - Attempts remaining: {remainingAttempts}");
        }
        else
        {
            Debug.LogWarning("[LaundryStageManager] Attempts remaining text component is missing!");
        }
    }
} 