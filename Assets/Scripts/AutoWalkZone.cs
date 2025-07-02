using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class AutoWalkZone : MonoBehaviour
{
    [Header("Auto Walk Settings")]
    public float autoMoveSpeed = 7f;
    public float transitionSmoothness = 0.1f; // Lower value = faster transition
    public float triggerZoneWidth = 2f; // Width of the trigger zone
    public float triggerZoneHeight = 4f; // Height of the trigger zone

    private Rigidbody2D playerRb;
    private bool isPlayerInZone = false;
    private float currentSpeed = 0f;
    private bool isTransitioning = false;
    private BoxCollider2D triggerCollider;
    private bool isExiting = false;

    private void Start()
    {
        // Get and setup the trigger collider
        triggerCollider = GetComponent<BoxCollider2D>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector2(triggerZoneWidth, triggerZoneHeight);
            triggerCollider.offset = new Vector2(0, triggerZoneHeight / 4f);
        }

        // Disable the component by default
        enabled = false;
    }

    private void OnEnable()
    {
        isPlayerInZone = false;
        isTransitioning = false;
        isExiting = false;
        currentSpeed = 0f;
        playerRb = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody2D>();
            isExiting = false;
            isPlayerInZone = true;
            isTransitioning = true;
            currentSpeed = playerRb.velocity.x;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Only process if we're not already in the zone and not exiting
            if (!isPlayerInZone && !isExiting)
            {
                isPlayerInZone = true;
                isTransitioning = true;
                if (playerRb == null)
                {
                    playerRb = other.GetComponent<Rigidbody2D>();
                    currentSpeed = playerRb.velocity.x;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(HandleExit());
        }
    }

    private IEnumerator HandleExit()
    {
        yield return new WaitForFixedUpdate();

        if (playerRb != null)
        {
            Collider2D playerCollider = playerRb.GetComponent<Collider2D>();
            if (playerCollider != null && !triggerCollider.IsTouching(playerCollider))
            {
                isExiting = true;
                isPlayerInZone = false;
                StartCoroutine(SmoothExit());
            }
        }
    }

    private IEnumerator SmoothExit()
    {
        float targetSpeed = 0f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.fixedDeltaTime / transitionSmoothness;
            if (playerRb != null)
            {
                float newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, t);
                playerRb.velocity = new Vector2(newSpeed, playerRb.velocity.y);
            }
            yield return new WaitForFixedUpdate();
        }

        // Ensure we reach exactly 0
        if (playerRb != null)
        {
            playerRb.velocity = new Vector2(0f, playerRb.velocity.y);
        }

        isTransitioning = false;
        enabled = false;
    }

    private void FixedUpdate()
    {
        if (!isPlayerInZone || playerRb == null) return;

        if (isTransitioning)
        {
            // Smoothly transition to auto move speed
            float targetSpeed = autoMoveSpeed;
            float t = Time.fixedDeltaTime / transitionSmoothness;
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, t);

            if (Mathf.Abs(currentSpeed - targetSpeed) < 0.01f)
            {
                currentSpeed = targetSpeed;
                isTransitioning = false;
            }
        }
        else
        {
            currentSpeed = autoMoveSpeed;
        }

        // Apply movement
        playerRb.velocity = new Vector2(currentSpeed, playerRb.velocity.y);
    }

    public void SetZoneSize(float width, float height)
    {
        triggerZoneWidth = width;
        triggerZoneHeight = height;
        if (triggerCollider != null)
        {
            triggerCollider.size = new Vector2(width, height);
            triggerCollider.offset = new Vector2(0, height / 4f);
        }
    }
} 