using UnityEngine;

[CreateAssetMenu(fileName = "New Challenge", menuName = "TaFightWorld/Challenge Data")]
public class ChallengeData : ScriptableObject
{
    [Header("Challenge Settings")]
    public ChallengeType challengeType;
    public string challengePrompt = "Jump 2 times!";
    public float timeLimit = 5f;

    [Header("Challenge Requirements")]
    public int requiredActions = 2;  // e.g., number of jumps or walk duration
    public float jumpForce = 10f;    // For jump challenges
    
    [Header("UI Messages")]
    public string successMessage = "Success!";
    public string failureMessage = "Try Again!";
} 