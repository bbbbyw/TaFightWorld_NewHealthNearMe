using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(ChallengeTriggerZone))]
public class AutoWalkZone : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    [Header("Zone Markers")]
    public WalkZoneMarker beginMarker;
    public WalkZoneMarker endMarker;
    
    [Header("Visual Settings")]
    public Color zoneColor = new Color(1f, 1f, 0f, 0.2f);  // Semi-transparent yellow
    public bool showZoneInGame = false;  // Whether to show the zone during gameplay
    
    private BoxCollider2D boxCollider;
    private bool isPlayerInZone = false;
    private Transform playerTransform;
    private bool isInJumpChallenge = false;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        UpdateZoneSize();
    }

    private void OnValidate()
    {
        UpdateZoneSize();
    }

    private void UpdateZoneSize()
    {
        if (beginMarker == null || endMarker == null) return;

        // Calculate center point between markers
        Vector3 center = (beginMarker.transform.position + endMarker.transform.position) * 0.5f;
        transform.position = center;

        // Calculate size based on distance between markers
        float width = Vector3.Distance(beginMarker.transform.position, endMarker.transform.position);
        
        // Update collider size
        if (boxCollider != null)
        {
            boxCollider.size = new Vector2(width, 2f);  // Height is fixed at 2 units
            boxCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            playerTransform = other.transform;
            CheckForJumpChallenge();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            playerTransform = null;
            isInJumpChallenge = false;
        }
    }

    private void CheckForJumpChallenge()
    {
        // Use OverlapCircle instead of OverlapCircleNonAlloc
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerTransform.position, 1f);
        foreach (Collider2D collider in colliders)
        {
            ChallengeTriggerZone challengeZone = collider.GetComponent<ChallengeTriggerZone>();
            if (challengeZone != null && challengeZone.challengeData.challengeType == ChallengeType.Jump)
            {
                isInJumpChallenge = true;
                break;
            }
        }
    }

    private void Update()
    {
        if (isPlayerInZone && playerTransform != null && !isInJumpChallenge)
        {
            // Calculate movement direction based on markers
            Vector3 direction = (endMarker.transform.position - beginMarker.transform.position).normalized;
            Vector3 movement = direction * moveSpeed * Time.deltaTime;
            
            // Move the player
            playerTransform.position += movement;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showZoneInGame || beginMarker == null || endMarker == null) return;

        Gizmos.color = zoneColor;
        
        // Draw zone area
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(boxCollider.size.x, boxCollider.size.y, 0.1f));
        }

        // Draw path line
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(beginMarker.transform.position, endMarker.transform.position);
        
        // Draw direction arrows
        Vector3 direction = (endMarker.transform.position - beginMarker.transform.position).normalized;
        float arrowSize = 0.5f;
        
        // Draw multiple arrows along the path
        float pathLength = Vector3.Distance(beginMarker.transform.position, endMarker.transform.position);
        int numArrows = Mathf.Max(1, Mathf.FloorToInt(pathLength / 2f));
        
        for (int i = 0; i < numArrows; i++)
        {
            float t = (i + 1) / (float)(numArrows + 1);
            Vector3 arrowPos = Vector3.Lerp(beginMarker.transform.position, endMarker.transform.position, t);
            
            Vector3 right = direction * arrowSize;
            Vector3 up = Vector3.up * arrowSize;
            
            // Draw arrow
            Gizmos.DrawLine(arrowPos, arrowPos - right + up);
            Gizmos.DrawLine(arrowPos, arrowPos - right - up);
        }
    }

    private void Start()
    {
        // Get the ChallengeTriggerZone component and set it as an auto-walk zone
        var challengeTrigger = GetComponent<ChallengeTriggerZone>();
        if (challengeTrigger != null)
        {
            challengeTrigger.isAutoWalkZone = true;
        }
    }
} 