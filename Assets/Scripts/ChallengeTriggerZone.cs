using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;
using Core;

[RequireComponent(typeof(Collider2D))]
public class ChallengeTriggerZone : MonoBehaviour
{
    [Header("Challenge Configuration")]
    public ChallengeData challengeData;
    public float characterMoveSpeed = 5f;
    public float forwardJumpForce = 4f;
    public float upwardJumpForce = 4f;
    public bool isStandalone = false;
    public Transform nextChallengePoint;

    [Header("World Space UI")]
    public Canvas worldSpaceCanvas;
    public float uiOffset = 2f;
    public Vector3 uiRotation = Vector3.zero;

    [Header("UI References")]
    public TextMeshProUGUI challengeText;
    public TextMeshProUGUI progressText;

    [Header("Events")]
    public UnityEvent onChallengeStart;
    public UnityEvent onChallengeSuccess;
    public UnityEvent onChallengeFail;

    // Public properties
    public bool IsActive => isActive;

    // Private fields
    private bool isActive = false;
    private bool isFailed = false;
    private StageManager stageManager;
    private PlayerController playerController;
    private Vector3 challengeStartPosition;

    private void Start()
    {
        stageManager = FindAnyObjectByType<StageManager>();
        if (stageManager == null)
        {
            Debug.LogError("StageManager not found!");
            return;
        }

        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(false);
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
            worldSpaceCanvas.worldCamera = Camera.main;
        }

        if (challengeData == null)
        {
            Debug.LogError("No ChallengeData assigned!");
            return;
        }
    }

    private void Update()
    {
        if (!isActive || playerController == null) return;

        UpdateUIPosition();

        switch (challengeData.challengeType)
        {
            case ChallengeType.Walk:
                HandleWalkChallenge();
                break;
            case ChallengeType.Jump:
                HandleJumpChallenge();
                break;
            case ChallengeType.TwistBody:
                HandleTwistChallenge();
                break;
        }
    }

    private void HandleWalkChallenge()
    {
        // Success on W key press
        if (Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log($"[Challenge] Walk challenge completed by pressing W at position: {transform.position}");
            CompleteChallenge(true);
        }
        // Fail on Up Arrow key press (for testing)
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log($"[Challenge] Walk challenge failed at position: {transform.position}");
            CompleteChallenge(false);
        }
    }

    private void HandleJumpChallenge()
    {
        // Success on Space key press
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[Challenge] Jump challenge started at position: {transform.position}");
            playerController.PerformJump(forwardJumpForce, upwardJumpForce);
            CompleteChallenge(true);
        }
        // Fail on Up Arrow key press (for testing)
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log($"[Challenge] Jump challenge failed at position: {transform.position}");
            CompleteChallenge(false);
        }
    }

    private void HandleTwistChallenge()
    {
        // Success on A key press
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log($"[Challenge] Twist challenge completed by pressing A at position: {transform.position}");
            CompleteChallenge(true);
        }
        // Fail on Up Arrow key press (for testing)
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Debug.Log($"[Challenge] Twist challenge failed at position: {transform.position}");
            CompleteChallenge(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"[Challenge] Player entered challenge zone at position: {transform.position}, isStandalone: {isStandalone}");

        // Get player controller first
        playerController = other.GetComponent<PlayerController>();
        if (playerController == null)
        {
            playerController = other.gameObject.AddComponent<PlayerController>();
        }

        // Store start position and stop player movement
        challengeStartPosition = other.transform.position;
        playerController.ResetVelocity();
        playerController.StopMovement();

        // Only remove auto walk if this is not a standalone challenge or if we have a next point
        if (challengeData != null && (!isStandalone || nextChallengePoint != null))
        {
            PlayerAutoWalk autoWalk = other.GetComponent<PlayerAutoWalk>();
            if (autoWalk != null)
            {
                Debug.Log($"[Challenge] Removing auto walk at position: {transform.position}");
                Destroy(autoWalk);
            }
        }

        // Start the challenge
        StartChallenge();
    }

    private void StartChallenge()
    {
        isActive = true;
        isFailed = false;

        Debug.Log($"[Challenge] Starting new challenge - Type: {challengeData.challengeType}, Position: {transform.position}");

        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(true);
            UpdateUIPosition();
            UpdateChallengeDisplay();
        }

        // Enable player input for this challenge
        if (playerController != null)
        {
            playerController.EnableMovement(true);
        }

        onChallengeStart.Invoke();
    }

    private void UpdateUIPosition()
    {
        if (worldSpaceCanvas != null && playerController != null)
        {
            Vector3 targetPos = playerController.transform.position + Vector3.up * uiOffset;
            worldSpaceCanvas.transform.position = targetPos;
            worldSpaceCanvas.transform.eulerAngles = uiRotation;

            if (Camera.main != null)
            {
                worldSpaceCanvas.transform.forward = Camera.main.transform.forward;
            }
        }
    }

    private void UpdateChallengeDisplay()
    {
        if (challengeText != null)
        {
            string challengeTypeText = challengeData.challengeType == ChallengeType.Walk ? "Walk" : challengeData.challengeType == ChallengeType.Jump ? "Jump" : "Twist Body";
            challengeText.text = $"{challengeTypeText} Challenge\n{challengeData.challengePrompt}";
        }
    }

    public void RestartChallenge()
    {
        if (playerController != null)
        {
            playerController.transform.position = challengeStartPosition;
            playerController.ResetVelocity();
            StartChallenge();
        }
    }

    public void CompleteChallenge(bool success)
    {
        if (!isActive && !challengeData.challengeType.Equals(ChallengeType.Walk))
        {
            Debug.Log("[Challenge] Tried to complete non-walk challenge but it's not active");
            return;
        }

        isActive = false;
        Debug.Log($"[Challenge] Challenge completed - Success: {success}, Position: {transform.position}");

        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.gameObject.SetActive(false);
        }

        if (success && challengeData != null)
        {
            // Add auto walk component and initialize it only if we have somewhere to go
            var player = playerController.gameObject;
            bool shouldAddAutoWalk = !isStandalone || nextChallengePoint != null;
            
            if (shouldAddAutoWalk)
            {
                // Remove existing auto walk if any
                var existingAutoWalk = player.GetComponent<PlayerAutoWalk>();
                if (existingAutoWalk != null)
                {
                    Debug.Log($"[Challenge] Removing existing auto walk before adding new one");
                    Destroy(existingAutoWalk);
                }

                Debug.Log($"[Challenge] Adding auto walk component with speed: {characterMoveSpeed}");
                PlayerAutoWalk autoWalk = player.AddComponent<PlayerAutoWalk>();
                autoWalk.Initialize(characterMoveSpeed);
            }
            else
            {
                Debug.Log($"[Challenge] Skipping auto walk addition for standalone challenge without next point");
            }

            onChallengeSuccess.Invoke();
            
            // Add null check before calling StageManager
            if (stageManager != null)
            {
                Debug.Log($"[Challenge] Notifying StageManager of success");
                stageManager.OnChallengeSuccess(this);
            }
        }
        else
        {
            onChallengeFail.Invoke();
            
            // Add null check before calling StageManager
            if (stageManager != null)
            {
                Debug.Log($"[Challenge] Notifying StageManager of failure");
                stageManager.OnChallengeFail(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isActive || isFailed)
            {
                if (worldSpaceCanvas != null)
                {
                    worldSpaceCanvas.gameObject.SetActive(false);
                }
            }
            
            playerController = null;
        }
    }

    public void OnDrawGizmos()
    {
        if (worldSpaceCanvas != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, worldSpaceCanvas.transform.position);
            Gizmos.DrawWireSphere(worldSpaceCanvas.transform.position, 0.5f);
        }
    }
}