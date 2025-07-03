using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class KnightTrialManager : MonoBehaviour
{
    [Header("Game Manager")]
    public HeartScoreManager heartScoreManager;

    [Header("Pose System")]
    public PoseGameManager poseGameManager;
    public PoseRequirement punchLeft;
    public PoseRequirement punchRight;
    public PoseRequirement kickLeft;
    public PoseRequirement kickRight;
    public PoseRequirement dodge;

    [Header("Stage")]
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI winText;
    public GameObject hero;
    public GameObject enemy;
    public float moveSpeed = 2f;

    [Header("Audio")]
    public AudioSource BGMaudioSource;
    public AudioSource SFXaudioSource;
    public AudioClip BGMsound;
    public AudioClip winSFX1;
    public AudioClip winSFX2;

    private List<PoseRequirement> allStages;
    private int currentStageIndex = 0;
    private List<Vector3> heroStagePositions;
    private List<Vector3> enemyStagePositions;

    private void Start()
    {
        Debug.Log($"KnightTrialManager Start(), poseGameManager assigned? {poseGameManager != null}");
        if (poseGameManager == null)
        {
            Debug.LogError("❌ poseGameManager is not assigned in KnightTrialManager!");
            return;
        }

        heroStagePositions = new List<Vector3>();
        enemyStagePositions = new List<Vector3>();

        heroStagePositions.Add(hero.transform.position);
        enemyStagePositions.Add(enemy.transform.position);

        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            CanvasGroup cg = winText.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0f;
        }

        allStages = new List<PoseRequirement>
        {
            punchLeft,
            punchRight,
            kickLeft,
            kickRight,
            dodge
        };

        if (BGMaudioSource != null && BGMsound != null)
        {
            BGMaudioSource.clip = BGMsound;
            BGMaudioSource.loop = true;
            BGMaudioSource.Play();
            Debug.Log("🎵 BGM started looping");
        }

        poseGameManager.PlayPoseExternal(allStages, OnGameComplete);

        poseGameManager.OnPoseStageAdvanced += HandlePoseStageAdvanced;
        poseGameManager.onSinglePoseCounted = OnRealtimeCounted;
    }

    private void HandlePoseStageAdvanced(int newIndex)
    {
        Debug.Log($"HandlePoseStageAdvanced called with index {newIndex}");
        currentStageIndex = newIndex;
        Debug.Log($"[DEBUG] poseIntroPanel.activeSelf = {poseGameManager.poseIntroPanel?.activeSelf}");

        // Save position before starting this stage
        heroStagePositions.Add(hero.transform.position);
        enemyStagePositions.Add(enemy.transform.position);

        StartCoroutine(MoveHeroSmoothly(2f));
        Debug.Log($"[DEBUG] poseIntroPanel AFTER = {poseGameManager.poseIntroPanel?.activeSelf}");
    }

    void OnRealtimeCounted()
    {   
        var currentPose = allStages[currentStageIndex];
        Debug.Log($"Counted pose: {currentPose.PoseNameThai} ({currentStageIndex}), count: {poseGameManager.CurrentCount}");

        if (poseGameManager.CurrentCount < currentPose.CountRequired)
        {
            instructionText.text = $"{currentPose.PoseNameThai}: {poseGameManager.CurrentCount}/{currentPose.CountRequired}";
        }
        else
        {
            instructionText.text = $"{currentPose.PoseNameThai}: สำเร็จ!";
        }

        switch (currentPose.PoseName.ToLower())
        {
            case "l-punch":
            case "r-punch":
            case "l-kick":
            case "r-kick":
            case "iceskate":
                PushEnemy(0.2f);
                break;
        }
    }

    void OnGameComplete(bool success)
    {
        instructionText.text = success ? "คุณคืออัศวินตัวจริง!" : "คุณล้มเหลว...";  

        if (BGMaudioSource != null && BGMaudioSource.isPlaying)
        {
            BGMaudioSource.Stop();
            Debug.Log("⏹️ BGM stopped");
        }

        if (!success)
        {
            // Return to the position before the current stage.
            if (currentStageIndex < heroStagePositions.Count)
                hero.transform.position = heroStagePositions[currentStageIndex];
            
            if (currentStageIndex < enemyStagePositions.Count)
                enemy.transform.position = enemyStagePositions[currentStageIndex];

            Debug.Log($"🔙 Reset to stage {currentStageIndex} positions.");
        }
        else
        {
            if (enemy != null)
                enemy.SetActive(false);
        }

        if (success)
        {
            if (winText != null)
            {
                winText.text = "WINNING!";
                if (ColorUtility.TryParseHtmlString("#FFFF2E", out Color parsedColor))
                    winText.color = parsedColor;
            }

            StartCoroutine(PlayWinSequence());
        }
        else
        {
            if (heartScoreManager != null)
            {
                heartScoreManager.OnChallengeFail(null, false);
            }
            else
            {
                Debug.LogWarning("❌ heartScoreManager is not assigned!");
            }
        }
    }

    IEnumerator PlayWinSequence()
    {
        if (SFXaudioSource != null && winSFX1 != null)
        {
            SFXaudioSource.PlayOneShot(winSFX1);
            yield return new WaitForSeconds(winSFX1.length);
        }

        if (SFXaudioSource != null && winSFX2 != null)
        {
            SFXaudioSource.PlayOneShot(winSFX2);
        }

        yield return StartCoroutine(ShowWinTextFade());

        if (heartScoreManager != null)
        {
            heartScoreManager.ShowGameCompletion();
        }
    }

    void PushEnemy(float distance)
    {
        if (enemy != null)
        {
            enemy.transform.position += new Vector3(distance, 0, 0);
            Debug.Log($"✅ Enemy moved to {enemy.transform.position}");
        }
        else
        {
            Debug.LogWarning("❌ Enemy GameObject not assigned.");
        }
    }

    IEnumerator MoveHeroSmoothly(float distance)
    {
        float moved = 0f;
        float step = 0.02f;

        while (moved < distance)
        {
            float moveThisFrame = step * moveSpeed;
            hero.transform.position += new Vector3(moveThisFrame, 0, 0);
            moved += moveThisFrame;
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator ShowWinTextFade()
    {
        CanvasGroup cg = winText.GetComponent<CanvasGroup>();
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime / 1.5f;
            cg.alpha = Mathf.Clamp01(alpha);
            yield return null;
        }
    }
}
