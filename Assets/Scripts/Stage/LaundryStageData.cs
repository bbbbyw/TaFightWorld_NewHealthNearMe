using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LaundryStage", menuName = "Stages/Laundry")]
public class LaundryStageData : ScriptableObject
{
    [System.Serializable]
    public class ChallengeStep
    {
        public LaundryActionType actionType;
        public string challengeText;

        [Header("Pose Requirements")]
        public List<PoseRequirement> poseRequirements = new List<PoseRequirement>();

        [Header("SFX Audio")]
        public AudioClip SFXSound;

        [Header("Animation")]
        public float additionalHoldTime = 0f;  // เวลาเพิ่มเติมที่จะรอหลังจาก animation เล่นจบ
        
        [Header("Debug")]
        public KeyCode successKey = KeyCode.A;
        public KeyCode failKey = KeyCode.UpArrow;
    }

    public ChallengeStep[] steps;
    public int maxFailAttempts = 3;
} 