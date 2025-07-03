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

    public void Initialize(MonoBehaviour runner)
    {
        coroutineRunner = runner;
        
        // เก็บค่า additionalHoldTime เริ่มต้นไว้
        _initialHoldTime = additionalHoldTime;
        
        // หาความยาวของ animation
        if (animator != null)
        {
            var controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                Debug.Log($"[LaundryChallengeObject] Searching for animation clips in controller: {controller.name}");
                foreach (var clip in controller.animationClips)
                {
                    Debug.Log($"[LaundryChallengeObject] Found clip: {clip.name}, Length: {clip.length}");
                    // เปลี่ยนการค้นหาจากการใช้ Contains เป็นการเช็คชื่อที่ตรงกันพอดี
                    if (clip.name == successAnimationStateName)
                    {
                        // คำนวณความยาวจริงโดยคำนึงถึง speed
                        animationLength = clip.length / animationSpeed;
                        Debug.Log($"[LaundryChallengeObject] Found success animation clip: {clip.name}, base length: {clip.length}, with speed {animationSpeed}, actual length: {animationLength}");
                        break;
                    }
                }

                if (animationLength <= 0)
                {
                    Debug.LogWarning($"[LaundryChallengeObject] Could not find exact animation clip named '{successAnimationStateName}', searching with contains...");
                    // ถ้าหาไม่เจอ ลองหาแบบ contains
                    foreach (var clip in controller.animationClips)
                    {
                        if (clip.name.Contains(successAnimationStateName))
                        {
                            animationLength = clip.length / animationSpeed;
                            Debug.Log($"[LaundryChallengeObject] Found animation clip containing name: {clip.name}, base length: {clip.length}, with speed {animationSpeed}, actual length: {animationLength}");
                            break;
                        }
                    }
                }

                // ถ้ายังหาไม่เจอ ใช้ค่าเริ่มต้น
                if (animationLength <= 0)
                {
                    animationLength = 1f;
                    Debug.LogWarning($"[LaundryChallengeObject] Could not find animation clip. Using default length: {animationLength} seconds");
                }
            }
        }
    }

    public void Show()
    {
        // แสดง object หลัก
        if (mainObject != null)
        {
            mainObject.SetActive(true);
        }

        // แสดง background components
        foreach (var obj in backgroundObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    public void Hide()
    {
        // ซ่อน object หลัก
        if (mainObject != null)
        {
            mainObject.SetActive(false);
        }

        // ซ่อน background components
        foreach (var obj in backgroundObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    public void PlayAnimation(string triggerName)
    {
        // เล่น animation เฉพาะเมื่อเป็น Success เท่านั้น
        if (animator != null && triggerName == "Success")
        {
            if (string.IsNullOrEmpty(successAnimationStateName))
            {
                Debug.LogError("[LaundryChallengeObject] Success animation state name is not set!");
                return;
            }

            // คืนค่า additionalHoldTime กลับเป็นค่าเริ่มต้น
            additionalHoldTime = _initialHoldTime;

            Debug.Log($"[LaundryChallengeObject] Playing success animation state: {successAnimationStateName} with speed: {animationSpeed}, additionalHoldTime: {additionalHoldTime}");
            
            // กำหนด speed ก่อนเล่น animation
            animator.speed = animationSpeed;
            animator.Play(successAnimationStateName);
            
            // รอให้ animation เล่นจบแล้วค่อยเรียก callback
            if (coroutineRunner != null)
            {
                coroutineRunner.StartCoroutine(WaitForAnimationComplete());
            }
            else
            {
                Debug.LogWarning("[LaundryChallengeObject] CoroutineRunner not initialized. Animation completion callback won't be fired.");
            }
        }
    }

    private IEnumerator WaitForAnimationComplete()
    {
        // รอให้ animation เริ่มเล่น
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log($"[LaundryChallengeObject] Waiting for animation to complete. Animation length with speed {animationSpeed}: {animationLength} seconds");
        
        // ตรวจสอบว่า animation เล่นจบจริงๆ
        currentState = animator.GetCurrentAnimatorStateInfo(0);
        float startTime = Time.time;
        
        while (!currentState.IsName(successAnimationStateName) || currentState.normalizedTime < 1.0f)
        {
            // เพิ่มการตรวจสอบ timeout โดยคำนึงถึง speed
            if (Time.time - startTime > animationLength * 2)
            {
                Debug.LogWarning("[LaundryChallengeObject] Animation wait timeout! Proceeding with completion.");
                break;
            }
            
            yield return null;
            currentState = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[LaundryChallengeObject] Animation state: {currentState.shortNameHash}, progress: {currentState.normalizedTime:F2}, speed: {animator.speed}");
        }

        // รอเวลาเพิ่มเติมถ้ากำหนดไว้
        if (additionalHoldTime > 0)
        {
            Debug.Log($"[LaundryChallengeObject] Holding for additional {additionalHoldTime} seconds");
            yield return new WaitForSeconds(additionalHoldTime);
        }
        
        // คืนค่า speed กลับเป็นค่าปกติ
        animator.speed = 1f;
        
        Debug.Log("[LaundryChallengeObject] Animation complete, invoking completion event");
        onAnimationComplete.Invoke();
    }
} 