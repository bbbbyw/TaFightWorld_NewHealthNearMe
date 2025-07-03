using UnityEngine;
using UnityEngine.Events;

public class LaundryChallengeTrigger : MonoBehaviour
{
    [Header("Challenge Settings")]
    public LaundryActionType actionType;
    public float challengeDuration = 3f; // ระยะเวลาที่ให้ทำ challenge
    public bool isActive = false;

    [Header("Visual Feedback")]
    public GameObject highlightEffect; // effect แสดงว่า challenge นี้กำลังทำงาน

    [Header("Events")]
    public UnityEvent onChallengeStart;
    public UnityEvent onChallengeSuccess;
    public UnityEvent onChallengeFail;

    private void Start()
    {
        // ปิด highlight ตอนเริ่มต้น
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
    }

    // เรียกจาก LaundryStageManager เมื่อถึงขั้นตอนนี้
    public void ActivateChallenge()
    {
        isActive = true;
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(true);
        }
        onChallengeStart.Invoke();
    }

    // เรียกเมื่อ challenge สำเร็จ
    public void OnSuccess()
    {
        if (!isActive) return;
        
        isActive = false;
        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
        onChallengeSuccess.Invoke();
    }

    // เรียกเมื่อ challenge ล้มเหลว
    public void OnFail()
    {
        if (!isActive) return;

        if (highlightEffect != null)
        {
            highlightEffect.SetActive(false);
        }
        onChallengeFail.Invoke();
    }
} 