using UnityEngine;

[CreateAssetMenu(fileName = "New Stage", menuName = "TaFightWorld/Stage")]
public class Stage : ScriptableObject
{
    [Header("Stage Information")]
    public string stageTitle = "Stage 1";
    public string instruction = "Walk forward!";
    public string description = "You see a peaceful garden ahead...";

    [Header("Stage Requirements")]
    public StagePoseType requiredPose = StagePoseType.Walk;
    public float completionDelay = 1.5f; // Time before moving to next stage
}

public enum StagePoseType
{
    None,
    Walk,
    Jump,
    Stretch,
    Nod
} 