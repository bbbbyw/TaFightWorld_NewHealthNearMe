using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PrefabSpawnPoint : MonoBehaviour
{
    private StageManager stageManager;

    private void Start()
    {
        // Get StageManager reference
        stageManager = FindAnyObjectByType<StageManager>();
        if (stageManager == null)
        {
            Debug.LogError("StageManager not found!");
            return;
        }

        // Ensure we have a trigger collider
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 5f); // Tall enough to catch the player
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify StageManager to spawn next stage
            stageManager.OnPrefabSpawnPointReached();
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a visual indicator for the spawn point
        Gizmos.color = Color.blue;
        Vector3 pos = transform.position;
        Gizmos.DrawWireCube(pos, new Vector3(1f, 5f, 0.1f));
        
        // Draw an arrow pointing right
        Vector3 arrowStart = pos + Vector3.right * 0.5f;
        Vector3 arrowEnd = arrowStart + Vector3.right;
        Gizmos.DrawLine(arrowStart, arrowEnd);
        Gizmos.DrawLine(arrowEnd, arrowEnd + new Vector3(-0.2f, 0.2f));
        Gizmos.DrawLine(arrowEnd, arrowEnd + new Vector3(-0.2f, -0.2f));
    }
} 