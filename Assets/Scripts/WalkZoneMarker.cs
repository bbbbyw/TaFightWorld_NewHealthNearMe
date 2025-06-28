using UnityEngine;

public class WalkZoneMarker : MonoBehaviour
{
    public enum MarkerType
    {
        Begin,
        End
    }

    public MarkerType markerType;
    public Color gizmoColor = Color.green;  // Green for begin, will be red for end

    private void OnDrawGizmos()
    {
        // Draw a vertical line with a sphere on top
        Gizmos.color = markerType == MarkerType.Begin ? Color.green : Color.red;
        
        // Draw vertical line
        Vector3 lineStart = transform.position;
        Vector3 lineEnd = lineStart + Vector3.up * 2f;
        Gizmos.DrawLine(lineStart, lineEnd);
        
        // Draw sphere on top
        Gizmos.DrawWireSphere(lineEnd, 0.3f);
        
        // Draw direction arrow for begin marker
        if (markerType == MarkerType.Begin)
        {
            Vector3 arrowStart = transform.position + Vector3.up;
            Vector3 arrowEnd = arrowStart + Vector3.right;
            Gizmos.DrawLine(arrowStart, arrowEnd);
            Gizmos.DrawLine(arrowEnd, arrowEnd + (Vector3.left + Vector3.up) * 0.2f);
            Gizmos.DrawLine(arrowEnd, arrowEnd + (Vector3.left + Vector3.down) * 0.2f);
        }
    }
} 