using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class ChallengeTriggerZone : MonoBehaviour
{
    [Header("Challenge Configuration")]
    public ChallengeData challengeData;
    public bool isAutoStart = true;
    public bool isAutoWalkZone = false; // New field to determine if this is an auto-walk zone
    public float autoMoveSpeed = 5f; // Speed for auto-forward movement

    [Header("World Space UI")]
    public Canvas worldSpaceCanvas;        // Reference to World Space canvas
    public float uiOffset = 1.5f;          // How far above the player
    public Vector3 uiRotation = new Vector3(0, 0, 0); // UI rotation in world space

    [Header("UI References")]
    public TextMeshProUGUI challengeText;  // Using TextMeshPro for better quality
    public TextMeshProUGUI progressText;

    [Header("Events")]
    public UnityEvent onChallengeStart;
    public UnityEvent onChallengeSuccess;
    public UnityEvent onChallengeFail;

    private float challengeTimer = 0f;
    private float walkProgress = 0f;
    private bool isActive = false;
    private bool isFailed = false;
    private Rigidbody2D playerRb;
    private Transform playerTransform;
    private Vector3 initialCanvasPosition;
    private StageManager stageManager;

    private void Start()
    {
        // Ensure UI starts hidden
        if (worldSpaceCanvas != null)
        {
            initialCanvasPosition = worldSpaceCanvas.transform.position;
            worldSpaceCanvas.gameObject.SetActive(false);
        }

        // Find StageManager
        stageManager = GameObject.FindWithTag("GameController")?.GetComponent<StageManager>();
    }

    private void Update()
    {
        if (!isActive || playerTransform == null) return;

        // Update UI position to follow player
        if (worldSpaceCanvas != null)
        {
            // Position canvas above player
            Vector3 targetPos = playerTransform.position + Vector3.up * uiOffset;
            worldSpaceCanvas.transform.position = targetPos;
            worldSpaceCanvas.transform.eulerAngles = uiRotation;

            // Optional: Make UI face camera
            if (Camera.main != null)
            {
                worldSpaceCanvas.transform.forward = Camera.main.transform.forward;
            }
        }

        // Auto-forward movement only in auto walk zones
        if (isActive && !isFailed && playerRb != null && isAutoWalkZone)
        {
            playerRb.velocity = new Vector2(autoMoveSpeed, playerRb.velocity.y);
        }

        // Challenge logic
        challengeTimer -= Time.deltaTime;
        
        switch (challengeData.challengeType)
        {
            case ChallengeType.Jump:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    walkProgress++;
                    UpdateProgressDisplay();
                    if (walkProgress >= challengeData.requiredActions)
                    {
                        CompleteChallenge(true);
                    }
                }
                break;

            case ChallengeType.Walk:
                // Only check for W key input if not in auto walk zone
                if (!isAutoWalkZone && Input.GetKey(KeyCode.W))
                {
                    walkProgress += Time.deltaTime;
                    UpdateProgressDisplay();
                    if (walkProgress >= challengeData.requiredActions)
                    {
                        CompleteChallenge(true);
                    }
                }
                // For auto walk zones, progress automatically
                else if (isAutoWalkZone)
                {
                    walkProgress += Time.deltaTime;
                    UpdateProgressDisplay();
                    if (walkProgress >= challengeData.requiredActions)
                    {
                        CompleteChallenge(true);
                    }
                }
                break;
        }

        if (challengeTimer <= 0 && !isFailed)
        {
            CompleteChallenge(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive && other.CompareTag("Player") && !isFailed)
        {
            playerRb = other.GetComponent<Rigidbody2D>();
            playerTransform = other.transform;
            if (isAutoStart)
            {
                StartChallenge();
            }
        }
    }

    public void StartChallenge()
    {
        isActive = true;
        isFailed = false;
        challengeTimer = challengeData.timeLimit;
        walkProgress = 0f;

        // Show UI
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(true);
        }

        if (challengeText != null)
        {
            challengeText.text = challengeData.challengePrompt;
        }

        UpdateProgressDisplay();
        onChallengeStart?.Invoke();
    }

    private void UpdateProgressDisplay()
    {
        if (progressText != null)
        {
            switch (challengeData.challengeType)
            {
                case ChallengeType.Jump:
                    progressText.text = $"Jumps: {walkProgress}/{challengeData.requiredActions}";
                    break;
                case ChallengeType.Walk:
                    float percentage = (walkProgress / challengeData.requiredActions) * 100f;
                    progressText.text = $"Walking: {percentage:F0}%";
                    break;
            }
        }
    }

    private void CompleteChallenge(bool success)
    {
        isActive = false;
        
        if (success)
        {
            if (challengeText != null)
                challengeText.text = challengeData.successMessage;

            if (challengeData.challengeType == ChallengeType.Jump && playerRb != null)
            {
                playerRb.AddForce(Vector2.up * challengeData.jumpForce, ForceMode2D.Impulse);
            }

            onChallengeSuccess?.Invoke();
        }
        else
        {
            isFailed = true;
            if (challengeText != null)
                challengeText.text = challengeData.failureMessage;

            onChallengeFail?.Invoke();

            // Reset player position
            if (playerRb != null)
            {
                // Stop auto-movement
                playerRb.velocity = Vector2.zero;
                
                // Let StageManager handle the retry logic
                if (stageManager != null)
                {
                    stageManager.RetryStage();
                }
            }
        }

        StartCoroutine(HideUIAfterDelay(1.5f));
    }

    private IEnumerator HideUIAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(false);
            worldSpaceCanvas.transform.position = initialCanvasPosition;
        }

        // Only reset isActive if we succeeded
        if (!isFailed)
        {
            isActive = false;
        }
    }
} 