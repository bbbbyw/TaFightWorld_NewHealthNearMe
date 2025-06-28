using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class KnightTrialManager : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI winText;
    public GameObject hero;
    public GameObject enemy;
    public float moveSpeed = 2f;

    [Header("Pose Requirements")]
    public PoseRequirement punchLeft;
    public PoseRequirement punchRight;
    public PoseRequirement kickLeft;
    public PoseRequirement kickRight;
    public PoseRequirement dodge;

    public PoseGameManager poseGameManager;

    private List<PoseRequirement> allStages;
    private int currentStageIndex = 0;

    private void Start()
    {
        Debug.Log($"KnightTrialManager Start(), poseGameManager assigned? {poseGameManager != null}");
        if (poseGameManager == null)
        {
            Debug.LogError("❌ poseGameManager is not assigned in KnightTrialManager!");
            return;
        }

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

        poseGameManager.OnPoseStageAdvanced += HandlePoseStageAdvanced;
        poseGameManager.onSinglePoseCounted = OnRealtimeCounted;

        poseGameManager.PlayPoseExternal(allStages, OnGameComplete);
    }

    private void HandlePoseStageAdvanced(int newIndex)
    {
        Debug.Log($"HandlePoseStageAdvanced called with index {newIndex}");
        currentStageIndex = newIndex;

        StartCoroutine(MoveHeroSmoothly(1f));
    }

    void OnRealtimeCounted()
    {   
        Debug.Log($"Counted pose: {allStages[currentStageIndex].PoseNameThai} ({currentStageIndex}), count: {poseGameManager.CurrentCount}");

        var currentPose = allStages[currentStageIndex];

        

        instructionText.text = $"{currentPose.PoseNameThai}: {poseGameManager.CurrentCount}/{currentPose.CountRequired}";

        switch (currentPose.PoseName.ToLower())
        {
            case "l-punch":
                PushEnemy(0.2f);
                break;
            case "r-punch":
                PushEnemy(0.2f);
                break;
            case "l-kick":
                PushEnemy(1f);
                break;
            case "r-kick":
                PushEnemy(1f);
                break;
            case "iceskate":
                PushEnemy(2f);
                break;
        }
    }

    void OnGameComplete(bool success)
    {
        instructionText.text = success ? "คุณคืออัศวินตัวจริง!" : "คุณล้มเหลว...";

        if (enemy != null) enemy.SetActive(false);

        if (winText != null)
        {
            winText.text = success ? "Winning!" : "Game Over!";
            if (ColorUtility.TryParseHtmlString(success ? "#FFFF2E" : "#F54447", out Color parsedColor))
                winText.color = parsedColor;
            StartCoroutine(ShowWinTextFade());
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
