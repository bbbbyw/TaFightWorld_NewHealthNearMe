using UnityEngine;
using Core;
using System.Collections;

public class DogChaseStageController : MonoBehaviour, IDogChaseStage
{
    [Header("Stage Settings")]
    public float characterMoveSpeed = 5f;
    public Transform exitPoint;        // จุดที่ตัวละครจะวิ่งไปเมื่อจบ stage

    [Header("Dog Settings")]
    public GameObject dog;            // หมาที่วางไว้ใน scene
    public float dogStartDistance = 5f;
    public Transform treeClimbPoint;
    public float dogChaseSpeed = 5f;     // ความเร็วของหมาตอนไล่
    public float dogClimbSpeed = 3f;     // ความเร็วของหมาตอนปีนต้นไม้

    [Header("Climb Settings")]
    public float climbHeight = 5f;    // ความสูงที่จะปีนขึ้นไป
    public float climbSpeed = 2f;     // ความเร็วในการปีน

    [Header("Stage Type")]
    public ChallengeType stageType = ChallengeType.DogChaseRun;  // กำหนดประเภทของ stage

    [Header("Animation Settings")]
    public GameObject runCharacter;    // GameObject ที่มี Animator สำหรับ run
    public GameObject climbCharacter;  // GameObject ที่มี Animator สำหรับ climb
    private Animator currentAnimator;  // Animator ที่กำลังใช้งาน
    private Animator dogAnimator;      // Animator ของหมา

    private Vector3 dogStartPosition;
    private bool isActive;
    private bool isTransitioning;
    private DogChaseChallenge currentChallenge;
    private DogChaseGameManager gameManager;

    public bool IsActive => isActive;
    public bool IsTransitioning => isTransitioning;

    private void Start()
    {
        gameManager = FindFirstObjectByType<DogChaseGameManager>();
        if (gameManager == null)
        {
            Debug.LogError("DogChaseGameManager not found!");
        }

        // Setup stage-specific character
        switch (stageType)
        {
            case ChallengeType.DogChaseRun:
                if (runCharacter != null)
                {
                    runCharacter.SetActive(true);
                    if (climbCharacter != null) climbCharacter.SetActive(false);
                    currentAnimator = runCharacter.GetComponent<Animator>();
                }
                else
                {
                    Debug.LogError($"Run character not assigned for {gameObject.name}!");
                }
                break;
            case ChallengeType.DogChaseClimb:
                if (climbCharacter != null)
                {
                    climbCharacter.SetActive(true);
                    if (runCharacter != null) runCharacter.SetActive(false);
                    currentAnimator = climbCharacter.GetComponent<Animator>();
                }
                else
                {
                    Debug.LogError($"Climb character not assigned for {gameObject.name}!");
                }
                break;
        }

        // Setup dog
        if (dog != null)
        {
            dogStartPosition = dog.transform.position;
            dogAnimator = dog.GetComponent<Animator>();
            if (dogAnimator == null)
            {
                Debug.LogWarning($"Dog Animator not found on {dog.name}!");
            }
        }
        else
        {
            Debug.LogError("Dog not assigned in scene!");
        }

        currentChallenge = GetComponentInChildren<DogChaseChallenge>();
    }

    public void StartDogChase()
    {
        isActive = true;
        isTransitioning = false;
    }

    public void CompleteDogChase(bool success)
    {
        if (!isActive || isTransitioning) return;

        Debug.Log($"[DogChase] CompleteDogChase called with success: {success}");

        if (success)
        {
            if (currentChallenge != null)
            {
                Debug.Log($"[DogChase] Current stage type: {stageType}");
                switch (stageType)
                {
                    case ChallengeType.DogChaseRun:
                        if (currentAnimator != null)
                        {
                            currentAnimator.SetTrigger("Run");
                        }
                        StartDogAnimation(true);
                        StartCoroutine(AutoMoveCharactersOut());
                        break;

                    case ChallengeType.DogChaseClimb:
                        Debug.Log("[DogChase] Starting climb sequence");
                        if (currentAnimator != null)
                        {
                            currentAnimator.SetTrigger("Climb");
                            Debug.Log("[DogChase] Climb animation triggered");
                        }
                        if (treeClimbPoint != null)
                        {
                            Debug.Log($"[DogChase] Tree climb point found at: {treeClimbPoint.position}");
                            var playerObject = GameObject.FindGameObjectWithTag("Player");
                            if (playerObject != null)
                            {
                                var player = playerObject.GetComponent<PlayerController>();
                                if (player != null)
                                {
                                    Debug.Log("[DogChase] Player controller found");
                                    player.transform.position = new Vector3(treeClimbPoint.position.x, player.transform.position.y, player.transform.position.z);
                                    
                                    var rb = player.GetComponent<Rigidbody2D>();
                                    if (rb != null)
                                    {
                                        Debug.Log("[DogChase] Disabling gravity");
                                        rb.gravityScale = 0;
                                        rb.linearVelocity = Vector2.zero;
                                    }

                                    Debug.Log("[DogChase] Starting climb coroutine");
                                    StartCoroutine(ClimbTree(player));
                                }
                                else
                                {
                                    Debug.LogError("[DogChase] PlayerController not found on player object!");
                                }
                            }
                            else
                            {
                                Debug.LogError("[DogChase] Could not find GameObject with Player tag!");
                            }
                        }
                        else
                        {
                            Debug.LogError($"[DogChase] Tree climb point is null!");
                        }
                        break;
                }
            }
            else
            {
                Debug.LogError("[DogChase] currentChallenge is null!");
            }
        }
        else
        {
            ResetDogPosition();
        }
    }

    public void CompleteStage(bool success)
    {
        if (gameManager != null)
        {
            Debug.Log($"[DogChase] Stage completed with success: {success}");
            gameManager.OnStageComplete(success);
        }
        else
        {
            Debug.LogError("[DogChase] GameManager not found!");
        }
    }

    public void ResetDogPosition()
    {
        if (dog != null)
        {
            dog.transform.position = dogStartPosition;
        }
    }

    public void StartDogAnimation(bool isRunning)
    {
        if (dogAnimator != null)
        {
            dogAnimator.SetTrigger(isRunning ? "RunAndBark" : "Idle");
        }
    }

    public void TransitionToNextScene()
    {
        CompleteStage(true);
    }

    private IEnumerator AutoMoveCharactersOut()
    {
        if (currentChallenge == null || currentChallenge.gameObject == null) yield break;

        var player = currentChallenge.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            var autoWalk = player.gameObject.AddComponent<PlayerAutoWalk>();
            autoWalk.Initialize(characterMoveSpeed);

            if (exitPoint != null)
            {
                while (Vector3.Distance(player.transform.position, exitPoint.position) > 0.1f)
                {
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(2f);
            }
        }

        if (dog != null)
        {
            var dogAutoWalk = dog.AddComponent<PlayerAutoWalk>();
            dogAutoWalk.Initialize(dogChaseSpeed); // ใช้ความเร็วสำหรับการไล่
        }

        // เมื่อตัวละครเคลื่อนที่เสร็จ ให้จบ stage
        CompleteStage(true);
    }

    private IEnumerator ClimbTree(PlayerController player)
    {
        isTransitioning = true;
        float startY = player.transform.position.y;
        float targetY = startY + climbHeight;
        
        Debug.Log($"[DogChase] Starting climb from {startY} to {targetY}");

        if (dog != null)
        {
            StartDogAnimation(true);
            var dogAutoWalk = dog.AddComponent<PlayerAutoWalk>();
            dogAutoWalk.Initialize(dogClimbSpeed);
            Vector3 dogTargetPosition = new Vector3(treeClimbPoint.position.x, dog.transform.position.y, dog.transform.position.z);
            StartCoroutine(MoveDogToTarget(dog.transform, dogTargetPosition, dogClimbSpeed));
        }
        
        // เริ่มเล่น animation ปีน
        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("Climb");
        }
        
        while (player.transform.position.y < targetY)
        {
            float newY = player.transform.position.y + climbSpeed * Time.deltaTime;
            player.transform.position = new Vector3(
                player.transform.position.x,
                newY,
                player.transform.position.z
            );
            Debug.Log($"[DogChase] Climbing... Current Y: {player.transform.position.y}");
            yield return null;
        }

        // หยุดการเคลื่อนที่ที่จุดสิ้นสุดพอดี
        player.transform.position = new Vector3(
            player.transform.position.x,
            targetY,
            player.transform.position.z
        );

        // รอสักครู่ก่อนจบ stage
        yield return new WaitForSeconds(1f);

        Debug.Log("[DogChase] Climb complete!");
        isTransitioning = false;
        
        CompleteStage(true);
    }

    private IEnumerator MoveDogToTarget(Transform dogTransform, Vector3 targetPosition, float speed)
    {
        while (Vector3.Distance(dogTransform.position, targetPosition) > 0.1f)
        {
            dogTransform.position = Vector3.MoveTowards(
                dogTransform.position,
                targetPosition,
                speed * Time.deltaTime
            );
            yield return null;
        }

        // เมื่อถึงจุดหมาย หยุดแอนิเมชันวิ่ง
        StartDogAnimation(false);

        // ลบ component AutoWalk
        var autoWalk = dog.GetComponent<PlayerAutoWalk>();
        if (autoWalk != null)
        {
            Destroy(autoWalk);
        }
    }
} 