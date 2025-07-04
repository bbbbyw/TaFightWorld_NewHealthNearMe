using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public class LaundryChallengeObject
{
    [Header("Main Object")]
    public GameObject mainObject;        // object หลักที่ใช้ทำ animation
    public Animator animator;            // animator ของ object หลัก

    [Header("Animation Settings")]
    [Tooltip("เวลาเพิ่มเติมที่จะรอหลังจาก animation เล่นจบ (วินาที)")]
    public float additionalHoldTime = 0f;
    [SerializeField] // เพิ่ม SerializeField เพื่อให้แก้ไขค่าใน Inspector ได้
    private float _initialHoldTime; // เก็บค่าเริ่มต้นไว้
    [Tooltip("ชื่อของ animation state ที่จะเล่นเมื่อสำเร็จ (เช่น HangClothe, GrabBasket)")]
    public string successAnimationStateName;  // ไม่กำหนดค่าเริ่มต้น ให้กำหนดใน Unity Inspector
    [Tooltip("ความเร็วของ animation (1 = ปกติ, 0.5 = ช้าลงครึ่งหนึ่ง, 2 = เร็วขึ้น 2 เท่า)")]
    public float animationSpeed = 1f;

    [Header("Background Components")]
    public GameObject[] backgroundObjects; // objects ประกอบฉาก เช่น โต๊ะ, ตู้, อุปกรณ์ตกแต่ง

    // Event ที่จะถูกเรียกหลังจาก animation จบ
    public UnityEvent onAnimationComplete = new UnityEvent();

    private MonoBehaviour coroutineRunner;  // ใช้สำหรับรัน coroutine
    private AnimatorStateInfo currentState;
    private float animationLength;
    private Coroutine currentAnimationCoroutine;

    public void Initialize(MonoBehaviour runner)
    {
        coroutineRunner = runner;
        _initialHoldTime = additionalHoldTime;

        if (animator != null)
        {
            var controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                animationLength = 0;
                foreach (var clip in controller.animationClips)
                {
                    if (clip.name == successAnimationStateName)
                    {
                        animationLength = clip.length / animationSpeed;
                        break;
                    }
                }

                if (animationLength <= 0)
                {
                    foreach (var clip in controller.animationClips)
                    {
                        if (clip.name.Contains(successAnimationStateName))
                        {
                            animationLength = clip.length / animationSpeed;
                            break;
                        }
                    }
                }

                if (animationLength <= 0)
                {
                    animationLength = 1f;
                }
            }
        }
    }
    public void Show()
    {
        if (mainObject != null)
        {
            mainObject.SetActive(true);

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0);
                animator.speed = 1f;
            }
        }

        foreach (var obj in backgroundObjects)
            if (obj != null)
                obj.SetActive(true);
    }

    public void Hide()
    {
        if (mainObject != null)
            mainObject.SetActive(false);

        foreach (var obj in backgroundObjects)
            if (obj != null)
                obj.SetActive(false);
    }

    public void PlayAnimation(string triggerName)
    {
        if (coroutineRunner != null && currentAnimationCoroutine != null)
        {
            coroutineRunner.StopCoroutine(currentAnimationCoroutine);
        }

        if (animator != null && triggerName == "Success")
        {
            if (string.IsNullOrEmpty(successAnimationStateName))
            {
                Debug.LogError("[LaundryChallengeObject] Success animation state name is not set!");
                return;
            }

            additionalHoldTime = _initialHoldTime;

            animator.Play(successAnimationStateName, 0, 0f);
            animator.speed = animationSpeed;

            currentAnimationCoroutine = coroutineRunner.StartCoroutine(WaitForAnimationComplete());
        }
        else if (triggerName == "Fail")
        {
            currentAnimationCoroutine = coroutineRunner.StartCoroutine(WaitForFailFallback());
        }
    }

    private IEnumerator WaitForAnimationComplete()
    {
        yield return new WaitForSeconds(0.1f);

        float startTime = Time.time;
        while (true)
        {
            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName(successAnimationStateName) && state.normalizedTime >= 1f)
            {
                break;
            }

            if (Time.time - startTime > animationLength * 2)
            {
                break;
            }

            yield return null;
        }

        if (additionalHoldTime > 0)
        {
            yield return new WaitForSeconds(additionalHoldTime);
        }

        animator.speed = 1f;
        onAnimationComplete?.Invoke();
    }

    private IEnumerator WaitForFailFallback()
    {
        yield return new WaitForSeconds(0.5f);
        onAnimationComplete?.Invoke();
    }

    public void AnimationCompleteEvent()
    {
        onAnimationComplete?.Invoke();
    }
}
