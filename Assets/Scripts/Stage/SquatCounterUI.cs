using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SquatCounterUI : MonoBehaviour
{
    [Header("Game Manager")]
    public HeartScoreManager heartScoreManager;

    [Header("Pose System")]
    private PoseGameManager poseGameManager;
    public PoseRequirement squatPoseRequirement;
    public PoseRequirement runningPoseRequirement;

    [Header("Squat Counting")]
    public TextMeshProUGUI counterText;

    [Header("Running Phase")]
    public GameObject hero;
    public GameObject vehicle;

    [Header("Durations")]
    public float restDuration = 10f; 
    public float runDuration = 60f;  

    [Header("Audio")]
    public AudioSource bgmAudioSource;
    public AudioSource effectAudioSource;
    public AudioClip horseSound;
    public AudioClip gotItemSound;
    
    private float restTimeLeft = 0f;    
    private float runTimeLeft = 0f;

    private int currentSquats = 0;
    private int maxSquats = 15;

    private bool isSquat = false;
    private bool isResting = false;
    private bool isRunning = false;

    // For running
    private float totalDistance; 
    private Vector3 flatTarget;
    private int totalIntervals = 12; // 60 sec / 5 sec = 12 steps

    private Vector3 heroStartPos;
    private Vector3 runTargetPos;
    private Animator heroAnimator;

    void OnEnable()
    {
        isSquat = false;
        isResting = false;
        isRunning = false;

        if (hero != null)
        {
            heroAnimator = hero.GetComponent<Animator>();

            if (heroAnimator == null)
            {
                Debug.LogError(">> Animator not found on hero object!");
            }
            else
            {
                Debug.Log(">> Animator component successfully found on hero.");
            }
        }
        else
        {
            Debug.LogError(">> Hero GameObject is not assigned.");
        }

        poseGameManager = PoseGameManager.Instance;
        if (poseGameManager == null)
        {
            Debug.LogError("❌ PoseGameManager.Instance not found!");
        }
    }

    void Update()
    {
        // Rotate counterText to face the camera.
        if (counterText != null && Camera.main != null)
            counterText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        if (isResting)
        {
            restTimeLeft -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(restTimeLeft);
            counterText.text = $"พักผ่อนสักหน่อย: {seconds} วิ";

            if (restTimeLeft <= 0f)
            {
                isResting = false;
                StartRunning();
            }
            return;
        }

        if (!isSquat) return;
    }

    public void StartSquat()
    {
        currentSquats = 0;
        isSquat = true;

        if (bgmAudioSource != null)
        {
            bgmAudioSource.loop = true;  
            bgmAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("BGM AudioSource is NULL!");
        }

        if (counterText != null)
        {
            counterText.gameObject.SetActive(true);
            counterText.text = "เก็บของที่ตกก่อน!";
        }

        if (poseGameManager != null && squatPoseRequirement != null)
        {
            squatPoseRequirement.CountRequired = maxSquats;
            poseGameManager.PlaySinglePose(
                squatPoseRequirement,
                OnPoseCounted,
                OnPoseCompleted
            );
        }
        else
        {
            Debug.LogError("PoseGameManager or SquatPoseRequirement == null");
        }
        
    }

    private void OnPoseCounted()
    {
        currentSquats = poseGameManager.CurrentCount;

        if (counterText != null)
        counterText.text = $"Squat : {currentSquats} / {maxSquats}";

        Debug.Log($">> Squat counted: {currentSquats}");
    }

    private void OnPoseCompleted()
    {
        Debug.Log($">> Squat phase complete!");

        isSquat = false;

        if (currentSquats >= maxSquats)
        {
            Debug.Log("✅ Squat success!");

            if (effectAudioSource != null && gotItemSound != null)
            {
                effectAudioSource.PlayOneShot(gotItemSound);
            }
            else
            {
                Debug.LogWarning("EffectAudioSource or gotItemSound == null!");
            }

            StartResting();
        }
        else
        {
            Debug.Log("❌ Squat failed, not enough squats!");

            if (heartScoreManager != null)
            {
                heartScoreManager.OnChallengeFail(null, false);
                if (!heartScoreManager.IsStageFailed)
                {
                    Debug.Log("🔁 Restarting Squat...");
                    StartSquat();
                }
                else
                {
                     if (bgmAudioSource != null && bgmAudioSource.isPlaying)
                    {
                        bgmAudioSource.Stop();
                    }

                    Debug.Log("❌ Game Over due to squat failure!");
                }
            }
        }
    }

    private void StartResting()
    {
        isResting = true;
        restTimeLeft = restDuration;

        if (counterText != null)
        {
            counterText.text = $"พักผ่อนสักหน่อย : {Mathf.CeilToInt(restTimeLeft)} sec";
        }
    }

    private void StartRunning()
    {
        isRunning = true;
        runTimeLeft = runDuration;

        if (counterText != null)
        {
            counterText.text = "เริ่มวิ่ง!";
        }

        if (hero != null && vehicle != null)
        {
            heroStartPos = hero.transform.position;
            runTargetPos = vehicle.transform.position;
            flatTarget = new Vector3(runTargetPos.x, hero.transform.position.y, hero.transform.position.z);
            totalDistance = Vector3.Distance(heroStartPos, flatTarget);
        }

        if (heroAnimator != null)
        {
            heroAnimator.SetTrigger("Run");
        }

        if (poseGameManager != null && runningPoseRequirement != null)
        {
            poseGameManager.onHoldIntervalPassed = OnRunHoldIntervalPassed;
            poseGameManager.PlayPoseExternal(
                new List<PoseRequirement> { runningPoseRequirement },
                (success) =>
                {
                    if (success)
                    {
                        OnRunPoseCompleted();
                    }
                    else
                    {
                        OnRunPoseFailed();
                    }
                }
            );
        }
        else
        {
            Debug.LogError("PoseGameManager or RunningPoseRequirement is NULL");
        }

        Invoke(nameof(BeginRunCountdown), 1f);
    }

    private void OnRunHoldIntervalPassed(int seconds)
    {
        Debug.Log($">> วิ่งผ่าน {seconds} วิ!");

        if (counterText != null)
            counterText.text = $"วิ่งได้แล้ว {seconds} วิ!";

        // Move step by step every 5 seconds.
        if (hero != null)
        {
            float stepDistance = totalDistance / totalIntervals;
            Vector3 direction = (flatTarget - heroStartPos).normalized;
            Vector3 newPos = hero.transform.position + direction * stepDistance;
            hero.transform.position = newPos;
        }
    }

    private void BeginRunCountdown()
    {
        Debug.Log(">> Running countdown started...");
    }

    private void OnRunPoseCompleted()
    {
        Debug.Log(">> Running pose completed!");

        isRunning = false;

        if (effectAudioSource != null && horseSound != null)
        {
            effectAudioSource.PlayOneShot(horseSound);
        }
        else
        {
            Debug.LogWarning("EffectAudioSource or horseSound == null!");
        }

        if (counterText != null)
        {
            counterText.text = "ถึงรถม้าสักที!";
        }

        Invoke(nameof(CompleteStage), 2f);
    }

    private void OnRunPoseFailed()
    {
        Debug.Log(">> Running pose failed!");

        if (heartScoreManager != null)
        {
            heartScoreManager.OnChallengeFail(null, false);

            if (!heartScoreManager.IsStageFailed)
            {
                Debug.Log("🔁 Restarting Running...");
                StartRunning();
            }
            else
            {
                Debug.Log("❌ Game Over due to running failure!");

                if (bgmAudioSource != null && bgmAudioSource.isPlaying)
                {
                    bgmAudioSource.Stop();
                }
            }
        }
    }

    private void CompleteStage()
    {
        Debug.Log(">> Running pose completed! Telling HeartScoreManager to show game completion");

        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        if (heartScoreManager != null)
        {
            heartScoreManager.ShowGameCompletion();
        }
        else
        {
            Debug.LogError("HeartScoreManager is NULL in SquatCounterUI!");
        }
    }
}
