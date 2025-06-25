using UnityEngine;

[CreateAssetMenu(fileName = "LaundryStage", menuName = "Stages/Laundry")]
public class LaundryStageData : ScriptableObject
{
    [System.Serializable]
    public class ChallengeStep
    {
        public LaundryActionType actionType;
        public string challengeText;
        [Header("Debug")]
        public KeyCode successKey = KeyCode.A;
        public KeyCode failKey = KeyCode.UpArrow;
    }

    public ChallengeStep[] steps;
    public int maxFailAttempts = 3;
} 