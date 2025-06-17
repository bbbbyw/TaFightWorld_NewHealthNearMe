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
    public UnityEvent<PoseType> onPoseDetected;
    public UnityEvent<PoseType> onPoseEnded;

    private PoseType currentPose = PoseType.None;

    private void Update()
    {
        CheckPoseInput();
    }

    private void CheckPoseInput()
    {
        // Check for new poses
        if (Input.GetKeyDown(walkKey)) HandlePoseStart(PoseType.Walk);
        else if (Input.GetKeyDown(jumpKey)) HandlePoseStart(PoseType.Jump);
        else if (Input.GetKeyDown(stretchKey)) HandlePoseStart(PoseType.Stretch);
        else if (Input.GetKeyDown(nodKey)) HandlePoseStart(PoseType.Nod);

        // Check for pose end
        if (Input.GetKeyUp(walkKey) && currentPose == PoseType.Walk) HandlePoseEnd();
        else if (Input.GetKeyUp(jumpKey) && currentPose == PoseType.Jump) HandlePoseEnd();
        else if (Input.GetKeyUp(stretchKey) && currentPose == PoseType.Stretch) HandlePoseEnd();
        else if (Input.GetKeyUp(nodKey) && currentPose == PoseType.Nod) HandlePoseEnd();
    }

    private void HandlePoseStart(PoseType pose)
    {
        currentPose = pose;
        onPoseDetected?.Invoke(pose);
    }

    private void HandlePoseEnd()
    {
        onPoseEnded?.Invoke(currentPose);
        currentPose = PoseType.None;
    }

    // This will be useful when we switch to MediaPipe
    public void StartDetection()
    {
        enabled = true;
    }

    public void StopDetection()
    {
        enabled = false;
        if (currentPose != PoseType.None)
        {
            HandlePoseEnd();
        }
    }
} 