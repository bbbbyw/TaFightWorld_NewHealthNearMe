using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAutoWalk : MonoBehaviour
{
    private Rigidbody2D rb;
    private float walkSpeed;
    private bool wasMoving = false;
    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    private const float STUCK_THRESHOLD = 0.1f;
    private const float STUCK_CHECK_DISTANCE = 0.001f; // ระยะทางขั้นต่ำที่ควรเคลื่อนที่ได้
    private const float UNSTUCK_FORCE = 0.1f; // แรงที่ใช้ในการแก้ติด

    private void Start()
    {
        InitializeRigidbody();
    }

    private void InitializeRigidbody()
    {
        if (rb != null) return;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found!");
            enabled = false;
            return;
        }

        // Ensure Rigidbody2D settings are correct
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        lastPosition = rb.position;
        
        Debug.Log($"[AutoWalk] Initial settings - Mass: {rb.mass}, Gravity: {rb.gravityScale}, Constraints: {rb.constraints}, SleepMode: {rb.sleepMode}");
    }

    public void Initialize(float speed)
    {
        InitializeRigidbody();
        
        if (rb == null)
        {
            Debug.LogError("[AutoWalk] Failed to initialize - Rigidbody2D not found!");
            return;
        }

        walkSpeed = speed;
        wasMoving = false;
        stuckTimer = 0f;
        lastPosition = rb.position;
        
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.WakeUp();
        }
        
        Debug.Log($"[AutoWalk] Started with speed: {speed}");
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Always wake up the Rigidbody2D if it's dynamic
        if (rb.bodyType == RigidbodyType2D.Dynamic)
        {
            rb.WakeUp();
        }

        // ตรวจสอบการติด
        float distanceMoved = Vector2.Distance(rb.position, lastPosition);
        bool shouldBeMoving = Mathf.Abs(walkSpeed) > 0.1f;

        if (shouldBeMoving && distanceMoved < STUCK_CHECK_DISTANCE)
        {
            stuckTimer += Time.fixedDeltaTime;
            if (stuckTimer >= STUCK_THRESHOLD)
            {
                Debug.Log($"[AutoWalk] Stuck detected! Pos: {rb.position}, Distance moved: {distanceMoved}");
                
                // แก้การติดด้วยหลายวิธี
                // 1. ใช้ AddForce
                rb.AddForce(new Vector2(UNSTUCK_FORCE, UNSTUCK_FORCE), ForceMode2D.Impulse);
                
                // 2. ขยับตำแหน่งเล็กน้อย
                rb.position += new Vector2(UNSTUCK_FORCE * 0.5f, UNSTUCK_FORCE * 0.5f);
                
                // 3. รีเซ็ตความเร็ว
                rb.linearVelocity = new Vector2(walkSpeed, rb.linearVelocity.y);
                
                // 4. Wake up อีกครั้ง
                rb.WakeUp();
                
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        // Set velocity
        Vector2 targetVelocity = new Vector2(walkSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;

        // Update tracking
        lastPosition = rb.position;
        wasMoving = shouldBeMoving;
    }

    private string GetColliderInfo()
    {
        var collider = GetComponent<Collider2D>();
        if (collider == null) return "No Collider2D found";
        
        return $"Collider type: {collider.GetType().Name}, Enabled: {collider.enabled}, IsTrigger: {collider.isTrigger}, Bounds: {collider.bounds}";
    }

    private void OnDisable()
    {
        Debug.Log($"[AutoWalk] Component disabled at position: {transform.position}");
    }

    private void OnDestroy()
    {
        Debug.Log($"[AutoWalk] Component destroyed at position: {transform.position}");
    }
} 