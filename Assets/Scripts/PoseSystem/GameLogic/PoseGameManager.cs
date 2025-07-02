using System; 
using System.Collections;  
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class PoseGameManager : MonoBehaviour
{
    public static PoseGameManager Instance { get; private set; }

    [Header("Detection & Logic")]
    public PoseLandmarkerLive detector;
    public PoseLogic logic;
    public GameObject CameraPreview;
    private List<PoseRequirement> poseList; 

    [Header("UI References")]
    public GameObject poseIntroPanel;
    public GameObject resultPanel;
    public Image poseIconUI;
    public Image poseIconImage;
    public Image introStatusIcon;
    public Image resultStatusIcon;
    public Image blackFilter;
    public TextMeshProUGUI poseThaiName;
    public TextMeshProUGUI countText;
    public TextMeshProUGUI uiText;
    public TextMeshProUGUI resultText;
    public UnityEngine.Video.VideoPlayer videoPlayer;
    public RawImage howToVideoRawImage;

    [Header("Button")]
    public Button howToButton;
    public Button pauseButton;
    public Button continueButton;
    public Button retryButton;

    [Header("Video")]
    public RenderTexture videoRenderTexture;
    public Button playVideoButton;
    public Button pauseVideoButton;
    public Button closeVideoButton;

    [Header("Result UI")]
    public float introDelay = 2f;
    public float resultSuccessDelay = 1f;

    [Header("Status Icons")]
    public Sprite hourglassIcon;
    public Sprite successIcon;
    public Sprite failIcon;

    [Header("Audio")]
    public AudioSource poseAudioSource;
    public AudioClip introSound;
    public AudioClip successSound;
    public AudioClip failSound;
    private bool isHoldSFXPlaying = false;

    // State
    private PoseRequirement currentPose;
    public float countCooldown = 0.3f;
    private int currentPoseIndex = 0;
    private float timeRemaining;
    private float holdTimer = 0f;
    private int counter = 0;
    private float lastCountTime = 0f;
    public bool autoStartPose = false;
    private bool isPoseActive = false;
    private string lastUIText = "";
    private bool isPaused = false;

    // Coroutine
    private Coroutine poseTimerCoroutine;

    // Callbacks
    private ChallengeTriggerZone currentChallengeTriggerZone;
    private Action<bool> externalCallback; // For sending success/fail at all endings
    public Action onSinglePoseCounted;    // Call every time the count increments by 1 (Realtime Count mode)
    private bool enableSingleCountCallback = false;
    private bool forceRealtimeCountingOnly = false;
    private bool externalModeActive = false;
    private bool finalSuccess = true;
    public int CurrentCount => counter;
    public event System.Action<int> OnPoseStageAdvanced;

    void Start()
    {
        Debug.Log($"PoseGameManager instances: {FindObjectsOfType<PoseGameManager>().Length}");

        CameraPreview?.SetActive(false);
        poseIntroPanel?.SetActive(false);
        resultPanel?.SetActive(false);
        uiText?.gameObject.SetActive(false);
        poseIconUI?.gameObject.SetActive(false);
        poseIconImage?.gameObject.SetActive(false);
        pauseButton?.gameObject.SetActive(false);

        blackFilter?.gameObject.SetActive(false);

        howToButton?.gameObject.SetActive(false);
        howToVideoRawImage?.gameObject.SetActive(false);
        videoPlayer.Stop();
        videoPlayer.clip = null;
        videoPlayer.gameObject.SetActive(false);
        Debug.Log("✅ VideoPlayer force stopped and hidden");
        videoPlayer.playOnAwake = false;
        closeVideoButton.gameObject.SetActive(false);

        isPaused = true;
        isPoseActive = false;
        detector.SetPaused(true);
        
        if (autoStartPose && poseList != null && poseList.Count > 0)
        {
            externalModeActive = true;
            currentPoseIndex = 0;
            StartNextPose();
        }
    }

    void Awake()
    {
        Instance = this;

        resultPanel.SetActive(false);
        continueButton?.gameObject.SetActive(false);
        pauseVideoButton?.gameObject.SetActive(false);
        playVideoButton?.gameObject.SetActive(false);
    }

    public void ResetPoseState()
    {
        timeRemaining = 0f;
        holdTimer = 0f;
        counter = 0;
        isPoseActive = false;

        if (!externalModeActive)
        {
            enableSingleCountCallback = false;
            onSinglePoseCounted = null;
            externalCallback = null;
        }

        forceRealtimeCountingOnly = false;
        finalSuccess = true;
        lastUIText = "";
    }

    void StartNextPose()
    {
        Debug.Log($"Start Next Pose!");
        ResetPoseState();

        if (poseList == null || poseList.Count == 0)
        {
            Debug.LogError("❌ poseList is NULL or EMPTY in StartNextPose!");
            return;
        }

        if (currentPoseIndex >= poseList.Count)
        {
            Debug.Log("✅ All poses done. Calling callback if in external mode.");
            isPoseActive = false;
            detector.SetPaused(true);

            if (externalModeActive)
            {
                externalCallback?.Invoke(finalSuccess);
                externalCallback = null;
                externalModeActive = false;
            }
            return;
        }

        currentPose = poseList[currentPoseIndex];

        if (currentPose == null)
        {
            Debug.LogError($"❌ Pose at index {currentPoseIndex} is NULL!");
            return;
        }

        Debug.Log($"📌 Starting pose: {currentPose.PoseName} | Icon: {(currentPose.PoseIcon != null ? "OK" : "MISSING")}");

        if (poseIconImage != null && currentPose.PoseIcon != null)
        {
            poseIconImage.sprite = currentPose.PoseIcon;
            poseIconImage.preserveAspect = true;
        }
        else
        {
            Debug.LogWarning("⚠️ poseIconImage or currentPose.PoseIcon isNULL");
        }

        if (poseThaiName != null)
        {
            poseThaiName.text = currentPose.PoseNameThai ?? "Unnamed Pose";
        }

        Debug.Log($"StartNextPose: Invoking OnPoseStageAdvanced with index {currentPoseIndex}");
        OnPoseStageAdvanced?.Invoke(currentPoseIndex);

        if (poseList == null || poseList.Count == 0 || currentPoseIndex >= poseList.Count)
        {
            Debug.LogError("❌ Cannot start pose: poseList is null/empty or index out of range.");
            return;
        }
        
        currentPose = poseList[currentPoseIndex];
        timeRemaining = currentPose.TimeLimit;
        holdTimer = 0f;
        counter = 0;

        if (poseAudioSource != null && introSound != null)
        {
            poseAudioSource.PlayOneShot(introSound);
        }
        else
        {
            Debug.LogWarning("[AUDIO] poseAudioSource OR introSound is NULL");
        }
        
        Debug.Log($"[StartNextPose] currentPoseIndex={currentPoseIndex}, poseListCount={poseList?.Count}");
        Debug.Log($"[StartNextPose] currentPose={currentPose}, poseIconImage={poseIconImage}, poseIcon={currentPose?.PoseIcon}");

        try
        {
            if (poseIconImage != null && currentPose != null && currentPose.PoseIcon != null)
            {
                poseIconImage.sprite = currentPose.PoseIcon;
                poseIconImage.preserveAspect = true;
                poseIconUI.sprite = currentPose.PoseIcon;
                poseIconUI.preserveAspect = true;
            }
            else
            {
                Debug.LogError("❌ One of the required components is null during sprite assignment");
                Debug.Log($"poseIconImage: {(poseIconImage != null ? "✅" : "❌")}, " +
                        $"currentPose: {(currentPose != null ? "✅" : "❌")}, " +
                        $"PoseIcon: {(currentPose?.PoseIcon != null ? "✅" : "❌")}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"🔥 Exception while setting pose icon: {ex}");
        }

        if (introStatusIcon != null && hourglassIcon != null)
        {
            introStatusIcon.gameObject.SetActive(true); 
            introStatusIcon.sprite = hourglassIcon;
        }

        if (poseThaiName != null && currentPose.PoseNameThai != null)
            poseThaiName.text = currentPose.PoseNameThai;
        else
            Debug.LogWarning("⚠️ poseThaiName or currentPose.PoseNameThai is null");

        if (countText != null)
        {
            countText.text = currentPose.Type == PoseType.Counting
                ? $"จำนวนครั้ง: {currentPose.CountRequired}"
                : $"ค้างท่า: {currentPose.DurationRequired:0.0} วินาที";
        }
        else
        {
            Debug.LogWarning("⚠️ countText is null");
        }

        poseIconUI?.gameObject.SetActive(true);
        poseIntroPanel?.SetActive(true);
        Debug.Log($"[DEBUG] poseIntroPanel activeSelf = {poseIntroPanel?.activeSelf}");
    
        CameraPreview?.SetActive(false);
        uiText?.gameObject.SetActive(false);
        retryButton?.gameObject.SetActive(false);
        poseIconImage?.gameObject.SetActive(false);
        howToButton?.gameObject.SetActive(false);
        closeVideoButton.gameObject.SetActive(false);
        pauseButton?.gameObject.SetActive(false);
        blackFilter?.gameObject.SetActive(false);

        Invoke(nameof(BeginPoseDetection), introDelay);
    }

    void BeginPoseDetection()
    {
        Debug.Log("[DEBUG] BeginPoseDetection called");
        poseIntroPanel?.SetActive(false);
        poseIconUI?.gameObject.SetActive(false);
        CameraPreview?.SetActive(true);
        uiText?.gameObject.SetActive(true);
        if (poseIconImage != null && currentPose != null && currentPose.PoseIcon != null)
        {
            poseIconImage.sprite = currentPose.PoseIcon;    
            poseIconImage.preserveAspect = true;
        }
        poseIconImage?.gameObject.SetActive(true);
        howToButton?.gameObject.SetActive(true);
        pauseButton?.gameObject.SetActive(true);
        continueButton?.gameObject.SetActive(false);

        isPaused = false;
        isPoseActive = true;
        detector.SetPaused(false);
        detector.OnLandmarksUpdated += OnLandmarksDetected;
        lastUIText = "";

        if (poseTimerCoroutine != null) StopCoroutine(poseTimerCoroutine);
        poseTimerCoroutine = StartCoroutine(PoseTimeLimit());
    }

    IEnumerator PoseTimeLimit()
    {
        Debug.Log("[DEBUG] PoseTimeLimit started");
        while (timeRemaining > 0f)
        {
            //Debug.Log($"⏳ timeRemaining: {timeRemaining}, isPaused={isPaused}");
            if (!isPaused)
            {
                timeRemaining -= Time.deltaTime;
            }
            yield return null;

            if (currentPose.Type == PoseType.Holding && holdTimer >= currentPose.DurationRequired)
            {
                PoseCompleted(true);
                yield break;
            }

            if (currentPose.Type == PoseType.Counting && counter >= currentPose.CountRequired)
            {
                PoseCompleted(true);
                yield break;
            }
        }
        Debug.Log("[DEBUG] PoseTimeLimit ended");
        PoseCompleted(false);
    }
    void PoseCompleted(bool success)
    {
        Debug.Log($"Pose Completed!");

        isPoseActive = false;
        if (poseTimerCoroutine != null) StopCoroutine(poseTimerCoroutine);
        detector.SetPaused(true);

        if (isHoldSFXPlaying && poseAudioSource.clip == currentPose.HoldSFX)
        {
            poseAudioSource.Stop();
            poseAudioSource.clip = null;
            poseAudioSource.loop = false;
            isHoldSFXPlaying = false;
        }

        resultPanel?.SetActive(true);
        CameraPreview?.SetActive(false);
        poseIconUI?.gameObject.SetActive(true);
        poseIntroPanel?.SetActive(false);
        uiText?.gameObject.SetActive(false);
        poseIconImage?.gameObject.SetActive(false);
        howToButton?.gameObject.SetActive(false);
        closeVideoButton?.gameObject.SetActive(false);
        pauseButton?.gameObject.SetActive(false);
        blackFilter?.gameObject.SetActive(true);
        howToVideoRawImage?.gameObject.SetActive(false);

        if (poseIconUI != null)
        {
            poseIconUI.sprite = currentPose.PoseIcon;
            poseIconUI.preserveAspect = true;
        }

        if (resultStatusIcon != null)
            resultStatusIcon.sprite = success ? successIcon : failIcon;

        if (resultText != null)
        {
            resultText.text = success ? "สำเร็จ!" : "หมดเวลา!";
            if (ColorUtility.TryParseHtmlString(success ? "#3EC479" : "#F54447", out Color parsedColor))
                resultText.color = parsedColor;
        }

        Debug.Log(success
            ? $"\u2705 Pose {currentPose.PoseName} success"
            : $"\u274C Pose {currentPose.PoseName} failed");

        if (!success)
            finalSuccess = false;

        if (success)
        {
            if (successSound != null)
                poseAudioSource.PlayOneShot(successSound);
            retryButton?.gameObject.SetActive(false);
            currentPoseIndex++;

            Invoke(nameof(ProceedToNext), resultSuccessDelay);
        }
        else
        {
            retryButton?.gameObject.SetActive(true);

            if (failSound != null)
                poseAudioSource.PlayOneShot(failSound);

            if (externalModeActive && externalCallback != null)
            {
                Debug.Log("🔁 Notifying external system of pose failure");
                externalCallback.Invoke(false);

                return;
            }
        }
    }

    public void RetryCurrentPose()
    {
        Debug.Log("Retry!");
        RestartCurrentPose();

        if (currentChallengeTriggerZone != null)
        {
            Debug.Log("[PoseGameManager] Restarting challenge in currentChallengeTriggerZone");
            currentChallengeTriggerZone.RestartChallenge();
        }
        else
        {
            Debug.LogWarning("[PoseGameManager] currentChallengeTriggerZone is null, cannot restart challenge");
        }
    }

    void RestartCurrentPose()
    {
        Debug.Log($"Restart Current Pose!");
        timeRemaining = currentPose.TimeLimit;
        holdTimer = 0f;
        counter = 0;
        isPoseActive = false;

        resultPanel?.SetActive(false);
        retryButton?.gameObject.SetActive(false);
        blackFilter?.gameObject.SetActive(false);

        poseIntroPanel?.SetActive(true);
        if (introSound != null)
            poseAudioSource.PlayOneShot(introSound);

        poseIconUI?.gameObject.SetActive(true);
        uiText?.gameObject.SetActive(false);
        poseIconImage?.gameObject.SetActive(false);

        if (poseIconUI != null)
        {
            poseIconUI.sprite = currentPose.PoseIcon;
            poseIconUI.preserveAspect = true;
        }
        if (introStatusIcon != null && hourglassIcon != null)
            introStatusIcon.sprite = hourglassIcon;
        if (poseThaiName != null)
            poseThaiName.text = currentPose.PoseNameThai;

        if (countText != null)
        {
            countText.text = currentPose.Type == PoseType.Counting
                ? $"จำนวนครั้ง: {currentPose.CountRequired}"
                : $"ค้างท่า: {currentPose.DurationRequired:0.0} วินาที";
        }

        Invoke(nameof(BeginPoseDetection), introDelay);
    }

    public void StopCurrentPose(bool success)
    {
        if (!isPoseActive)
        {
            Debug.LogWarning("⚠️ Tried to stop pose but it's already inactive.");
            return;
        }

        Debug.Log($"🛑 Manually stopping pose: {currentPose.PoseName}, success: {success}");
        PoseCompleted(success);
    }

    void ProceedToNext()
    {
        resultPanel?.SetActive(false);
        retryButton?.gameObject.SetActive(false);
        poseIconUI?.gameObject.SetActive(false);
        poseIconImage?.gameObject.SetActive(false);
        blackFilter?.gameObject.SetActive(false);

        if (currentPoseIndex >= poseList.Count)
        {
            isPoseActive = false;
            detector.SetPaused(true);
            CameraPreview?.SetActive(false);

            if (externalModeActive)
            {
                externalCallback?.Invoke(finalSuccess);
                externalCallback = null;
                externalModeActive = false;
            }

            return;
        }

        StartNextPose();
    }

    void OnLandmarksDetected(List<Vector3> landmarks)
    {
        if (!isPoseActive || currentPose == null || logic == null || landmarks == null) {
            Debug.Log($"Skip detection. isPoseActive={isPoseActive}, currentPose={currentPose}, logic={logic}, landmarks={landmarks}");
            return;
        }

        bool detected = logic.IsPoseDetected(currentPose.PoseName, landmarks);
        int displayTime = Mathf.CeilToInt(timeRemaining);

        if (currentPose.Type == PoseType.Holding)
        {
            if (detected)
            {
                holdTimer += Time.deltaTime;

                if (!isHoldSFXPlaying && currentPose.HoldSFX != null)
                {
                    poseAudioSource.clip = currentPose.HoldSFX;
                    poseAudioSource.loop = true;
                    poseAudioSource.Play();
                    isHoldSFXPlaying = true;
                }
            }
            else
            {
                if (isHoldSFXPlaying && poseAudioSource.clip == currentPose.HoldSFX)
                {
                    poseAudioSource.Stop();
                    poseAudioSource.clip = null;
                    poseAudioSource.loop = false;
                    isHoldSFXPlaying = false;
                }
            }

            int holdSec = Mathf.FloorToInt(holdTimer);
            int requiredSec = Mathf.CeilToInt(currentPose.DurationRequired);
            string newText = $"{currentPose.PoseNameThai}\nค้าง: {holdSec}s / {requiredSec}s\nเวลาที่เหลือ: {displayTime}s";

            if (newText != lastUIText)
            {
                uiText.text = newText;
                lastUIText = newText;
            }
        }

        if (currentPose.Type == PoseType.Counting)
        {
            if (detected && Time.time - lastCountTime >= countCooldown)
            {
                counter++;
                lastCountTime = Time.time;

                if (currentPose.CountSFX != null)
                    poseAudioSource.PlayOneShot(currentPose.CountSFX);

                Debug.Log($"✅ Counted! total = {counter}");
                if (enableSingleCountCallback && onSinglePoseCounted != null)
                {
                    onSinglePoseCounted.Invoke();
                }
            }

            string newText = $"{currentPose.PoseNameThai}\nจำนวนครั้ง: {counter} / {currentPose.CountRequired}\nเวลาที่เหลือ: {displayTime}s";
            if (newText != lastUIText)
            {
                uiText.text = newText;
                lastUIText = newText;
            }
        }
    }

    // Call to play the desired set of moves and callback when successful/failed.
    public void PlayPoseExternal(List<PoseRequirement> poses, Action<bool> callback)
    {
        if (poses == null || poses.Count == 0)
        {
            Debug.LogError("❌ PlayPoseExternal called with NULL or EMPTY poses list");
            return;
        }

        poseList = poses.FindAll(p => p != null); 
        if (poseList.Count == 0)
        {
            Debug.LogError("❌ All PoseRequirements are NULL!");
            return;
        }

        externalCallback = callback;
        externalModeActive = true;
        currentPoseIndex = 0;
        finalSuccess = true;

        Debug.Log($"✅ PlayPoseExternal: {poseList.Count} poses loaded. Starting...");

        StartNextPose();
    }

    // Play a single pose in realtime count mode (counts times and calls back every time)
    public void PlaySinglePose(PoseRequirement pose, Action onCounted, Action onFinished = null)
    {   
        poseList = new List<PoseRequirement> { pose };
        currentPoseIndex = 0;

        onSinglePoseCounted = onCounted;
        externalCallback = (success) => onFinished?.Invoke();
        enableSingleCountCallback = true;

        externalModeActive = true;
        finalSuccess = true;

        StartNextPose();
    }

    // ----------------- Pause & Continue Play System -----------------
    public void OnPauseClicked()
    {
        isPaused = true;
        isPoseActive = false;
        CameraPreview?.SetActive(false);
        pauseButton?.gameObject.SetActive(true);
        continueButton?.gameObject.SetActive(true);
        blackFilter?.gameObject.SetActive(true);

        isPaused = true;
        isPoseActive = false;
        detector.SetPaused(true);
        if (poseTimerCoroutine != null)
            StopCoroutine(poseTimerCoroutine);

    }
    public void OnContinuePlayClicked()
    {
        if (poseTimerCoroutine != null)
            StopCoroutine(poseTimerCoroutine);

        isPaused = false;
        isPoseActive = true;
        detector.SetPaused(false);
        CameraPreview?.SetActive(true);
        pauseButton?.gameObject.SetActive(true);
        continueButton?.gameObject.SetActive(false);
        blackFilter?.gameObject.SetActive(false);
        poseTimerCoroutine = StartCoroutine(PoseTimeLimit());

    }

    // Use in case of go to home
    public void ForceStop()
    {
        Debug.Log("⛔ Force stopping PoseGameManager...");

        isPoseActive = false;
        if (poseTimerCoroutine != null) StopCoroutine(poseTimerCoroutine);
        detector.SetPaused(true);

        StopAllCoroutines();
        CameraPreview?.SetActive(false);
        resultPanel?.SetActive(false);
        poseIntroPanel?.SetActive(false);
        uiText?.gameObject.SetActive(false);
        poseIconUI?.gameObject.SetActive(false);
        poseIconImage?.gameObject.SetActive(false);
        howToButton?.gameObject.SetActive(false);
        pauseButton?.gameObject.SetActive(false);
        continueButton?.gameObject.SetActive(false);
        blackFilter?.gameObject.SetActive(false);
        howToVideoRawImage?.gameObject.SetActive(false);
        videoPlayer?.Stop();
        videoPlayer?.gameObject.SetActive(false);

        onSinglePoseCounted = null;
        externalCallback = null;
        enableSingleCountCallback = false;
        finalSuccess = false;

        Debug.Log("✅ PoseGameManager cleaned up.");
    }

    // ----------------- Tutorial Video System -----------------
    IEnumerator PlayVideoOnAndroid(string path)
    {
        using (UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(path))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Video Load Error: " + request.error);
                yield break;
            }

            videoPlayer.url = path;
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnVideoPrepared;
        vp.Play();
        playVideoButton?.gameObject.SetActive(false);
        pauseVideoButton?.gameObject.SetActive(true);
        Debug.Log("▶️ Video is now playing.");
    }

    public void OnHowToClicked()
    {
        isPaused = true;
        isPoseActive = false;
        detector.SetPaused(true);

        if (poseTimerCoroutine != null)
            StopCoroutine(poseTimerCoroutine);

        videoPlayer.gameObject.SetActive(true);
        howToVideoRawImage.gameObject.SetActive(true);

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        howToVideoRawImage.texture = videoRenderTexture;

        CameraPreview?.SetActive(false);
        closeVideoButton?.gameObject.SetActive(true);
        blackFilter?.gameObject.SetActive(true);

        playVideoButton?.gameObject.SetActive(true);

        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, currentPose.LocalVideoFileName);

#if UNITY_ANDROID
        videoPath = "jar:file://" + Application.dataPath + "!/assets/" + currentPose.LocalVideoFileName;
#else
        videoPath = "file://" + videoPath;
#endif

        videoPlayer.url = videoPath;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();

        pauseVideoButton?.gameObject.SetActive(false);
        playVideoButton?.gameObject.SetActive(true);

        Debug.Log("📺 Preparing video: " + videoPath);
    }

    public void PauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("⏸️ Video paused.");
        }
        pauseVideoButton?.gameObject.SetActive(false);
        playVideoButton?.gameObject.SetActive(true);
        videoPlayer?.gameObject.SetActive(false);
    }

    public void PlayVideo()
    {
        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
            Debug.Log("▶️ Video resumed.");
        }
        playVideoButton?.gameObject.SetActive(false);
        pauseVideoButton?.gameObject.SetActive(true);
    }

    public void OnCloseVideo()
    {
        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        videoPlayer.gameObject.SetActive(false);
        howToVideoRawImage?.gameObject.SetActive(false);

        CameraPreview?.SetActive(true);
        closeVideoButton?.gameObject.SetActive(false);
        blackFilter?.gameObject.SetActive(false);
        playVideoButton?.gameObject.SetActive(false);
        pauseVideoButton?.gameObject.SetActive(false);

        isPaused = false;
        isPoseActive = true;
        detector.SetPaused(false);
        poseTimerCoroutine = StartCoroutine(PoseTimeLimit());
    }

    IEnumerator WaitForPrepareAndPlay()
    {
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();
        Debug.Log("▶️ Video is now playing.");
    }

    public void SetCurrentChallengeTriggerZone(ChallengeTriggerZone triggerZone)
    {
        currentChallengeTriggerZone = triggerZone;
    }

}
