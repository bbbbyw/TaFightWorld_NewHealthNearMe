using UnityEngine;

namespace Core
{
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool canMove = true;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on PlayerController!");
            enabled = false;
            return;
        }
    }

    public void PerformJump(float forwardForce, float upwardForce)
    {
        if (!canMove || rb == null) return;

        rb.AddForce(new Vector2(forwardForce, upwardForce), ForceMode2D.Impulse);
    }

    public void EnableMovement(bool enable)
    {
        canMove = enable;
        if (!enable)
        {
            StopMovement();
        }
    }

    public void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    public void ResetVelocity()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
}