using UnityEngine;
using UnityEngine.Events;

public class PoseInputSimulator : MonoBehaviour
{
    [Header("Key Mappings")]
    [SerializeField] private KeyCode walkKey = KeyCode.W;
    [SerializeField] private KeyCode jumpKey = KeyCode.J;
    [SerializeField] private KeyCode stretchKey = KeyCode.S;
    [SerializeField] private KeyCode nodKey = KeyCode.N;

    // Events
    public UnityEvent<StagePoseType> onPoseDetected;
    public UnityEvent<StagePoseType> onPoseEnded;

    private StagePoseType currentPose = StagePoseType.None;

    private void Update()
    {
        CheckPoseInput();
    }

    private void CheckPoseInput()
    {
        // Check for new poses
        if (Input.GetKeyDown(walkKey)) HandlePoseStart(StagePoseType.Walk);
        else if (Input.GetKeyDown(jumpKey)) HandlePoseStart(StagePoseType.Jump);
        else if (Input.GetKeyDown(stretchKey)) HandlePoseStart(StagePoseType.Stretch);
        else if (Input.GetKeyDown(nodKey)) HandlePoseStart(StagePoseType.Nod);

        // Check for pose end
        if (Input.GetKeyUp(walkKey) && currentPose == StagePoseType.Walk) HandlePoseEnd();
        else if (Input.GetKeyUp(jumpKey) && currentPose == StagePoseType.Jump) HandlePoseEnd();
        else if (Input.GetKeyUp(stretchKey) && currentPose == StagePoseType.Stretch) HandlePoseEnd();
        else if (Input.GetKeyUp(nodKey) && currentPose == StagePoseType.Nod) HandlePoseEnd();
    }

    private void HandlePoseStart(StagePoseType pose)
    {
        currentPose = pose;
        onPoseDetected?.Invoke(pose);
    }

    private void HandlePoseEnd()
    {
        onPoseEnded?.Invoke(currentPose);
        currentPose = StagePoseType.None;
    }

    // This will be useful when we switch to MediaPipe
    public void StartDetection()
    {
        enabled = true;
    }

    public void StopDetection()
    {
        enabled = false;
        if (currentPose != StagePoseType.None)
        {
            HandlePoseEnd();
        }
    }
} 