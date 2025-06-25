using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class LaundryStageManager : MonoBehaviour
{
    [Header("Stage Data")]
    public LaundryStageData stageData;
    
    [Header("Challenge Objects")]
    public GameObject basketObject;      // object สำหรับหยิบผ้า
    public GameObject wringObject;       // object สำหรับบิดผ้า
    public GameObject hangObject;        // object สำหรับตากผ้า

    [Header("UI")]
    public TextMeshProUGUI challengeText;
    public GameObject gameOverPanel;
    public GameObject successPanel;
    public TransitionManager transitionManager;

    [Header("Events")]
    public UnityEvent onStageComplete;
    public UnityEvent onGameOver;

    private int currentStepIndex = 0;
    private int failCount = 0;
    private GameObject currentObject;
    private Animator currentAnimator;

    private void Start()
    {
        if (stageData == null)
        {
            Debug.LogError("LaundryStageData not assigned!");
            return;
        }

        // ปิดทุก object ตอนเริ่มต้น
        basketObject.SetActive(false);
        wringObject.SetActive(false);
        hangObject.SetActive(false);

        SetupCurrentStep();
    }

    private void SetupCurrentStep()
    {
        var currentStep = stageData.steps[currentStepIndex];
        
        // ใช้ transition ในการเปลี่ยน object
        transitionManager.DoTransition(() => {
            // ทำงานตรงกลาง transition (ตอนจอดำ)
            challengeText.text = currentStep.challengeText;

            // ปิด object เก่า
            if (currentObject != null)
            {
                currentObject.SetActive(false);
            }

            // เปิด object ใหม่ตาม action type
            switch (currentStep.actionType)
            {
                case LaundryActionType.PickupClothes:
                    currentObject = basketObject;
                    break;
                case LaundryActionType.WringClothes:
                    currentObject = wringObject;
                    break;
                case LaundryActionType.HangClothes:
                    currentObject = hangObject;
                    break;
            }

            if (currentObject != null)
            {
                currentObject.SetActive(true);
                currentAnimator = currentObject.GetComponent<Animator>();
            }
        });
    }

    private void Update()
    {
        if (stageData == null || currentObject == null) return;
        
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
        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("Success");
        }

        // รอให้ animation เล่นจบก่อนไปขั้นต้อนถัดไป
        Invoke("MoveToNextStep", 1f);
    }

    private void MoveToNextStep()
    {
        currentStepIndex++;
        if (currentStepIndex >= stageData.steps.Length)
        {
            // Stage complete
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
        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("Fail");
        }

        failCount++;
        if (failCount >= stageData.maxFailAttempts)
        {
            // Game over
            gameOverPanel.SetActive(true);
            onGameOver.Invoke();
        }
    }
} 