using UnityEngine;
using Core;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Challenge", menuName = "Game/Challenge Data")]
public class ChallengeData : ScriptableObject
{
    [SerializeField]
    public string challengePrompt;
    public ChallengeType challengeType;  

    [Header("Pose Requirements")]
    public List<PoseRequirement> poseRequirements = new List<PoseRequirement>();

    [Header("UI Prefabs")]
    public GameObject walkSuccessUI;
    public GameObject walkFailureUI;
    public GameObject jumpSuccessUI;
    public GameObject jumpFailureUI;
    public GameObject twistSuccessUI;  // New: UI for twist success
    public GameObject twistFailureUI;  // New: UI for twist failure
    
    [Header("UI Messages")]
    public string successMessage = "Success!";
    public string failureMessage = "Try Again!";

    // These fields will be used by MediaPipe scripts later
    [HideInInspector] public float requiredHoldTime; // For Walk: time to hold pose
    [HideInInspector] public float requiredJumpHeight; // For Jump: minimum height
    [HideInInspector] public float requiredJumpCount; // For Jump: number of jumps
    [HideInInspector] public float requiredTwistAngle; // For Twist: minimum angle
} 