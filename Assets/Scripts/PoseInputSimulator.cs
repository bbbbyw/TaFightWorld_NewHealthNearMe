using UnityEngine;
using UnityEngine.Events;
using Core;

public class PoseInputSimulator : MonoBehaviour
{
    [Header("Key Mappings")]
    [SerializeField] private KeyCode walkKey = KeyCode.W;
    [SerializeField] private KeyCode jumpKey = KeyCode.J;
    [SerializeField] private KeyCode stretchKey = KeyCode.S;
    [SerializeField] private KeyCode nodKey = KeyCode.N;
    [SerializeField] private KeyCode twistKey = KeyCode.A;

    // Events
    public UnityEvent<ChallengeType> onPoseDetected;
    public UnityEvent<ChallengeType> onPoseEnded;

    private ChallengeType currentPose = ChallengeType.None;

    private void Update()
    {
        CheckPoseInput();
    }

    private void CheckPoseInput()
    {
        // Check for new poses
        if (Input.GetKeyDown(walkKey)) HandlePoseStart(ChallengeType.Walk);
        else if (Input.GetKeyDown(jumpKey)) HandlePoseStart(ChallengeType.Jump);
        else if (Input.GetKeyDown(twistKey)) HandlePoseStart(ChallengeType.TwistBody);
        else if (Input.GetKeyDown(stretchKey)) HandlePoseStart(ChallengeType.None);
        else if (Input.GetKeyDown(nodKey)) HandlePoseStart(ChallengeType.None);

        // Check for pose end
        if (Input.GetKeyUp(walkKey) && currentPose == ChallengeType.Walk) HandlePoseEnd();
        else if (Input.GetKeyUp(jumpKey) && currentPose == ChallengeType.Jump) HandlePoseEnd();
        else if (Input.GetKeyUp(twistKey) && currentPose == ChallengeType.TwistBody) HandlePoseEnd();
        else if (Input.GetKeyUp(stretchKey) && currentPose == ChallengeType.None) HandlePoseEnd();
        else if (Input.GetKeyUp(nodKey) && currentPose == ChallengeType.None) HandlePoseEnd();
    }

    private void HandlePoseStart(ChallengeType pose)
    {
        currentPose = pose;
        onPoseDetected?.Invoke(pose);
    }

    private void HandlePoseEnd()
    {
        onPoseEnded?.Invoke(currentPose);
        currentPose = ChallengeType.None;
    }

    // This will be useful when we switch to MediaPipe
    public void StartDetection()
    {
        enabled = true;
    }

    public void StopDetection()
    {
        enabled = false;
        if (currentPose != ChallengeType.None)
        {
            HandlePoseEnd();
        }
    }
} 