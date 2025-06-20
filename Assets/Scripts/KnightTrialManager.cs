using UnityEngine;
using TMPro;
using System.Collections;

public class KnightTrialManager : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI winText;
    public GameObject hero;
    public GameObject enemy;
    public float moveSpeed = 2f;

    private int punchLeft = 0, punchRight = 0, kickLeft = 0, kickRight = 0, dodgeCount = 0;
    private enum Stage { PunchLeft, PunchRight, KickLeft, KickRight, Dodge, Done }
    private Stage currentStage = Stage.PunchLeft;

    void Start()
    {
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            CanvasGroup cg = winText.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 0f;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) HandleAction("left");
        if (Input.GetKeyDown(KeyCode.D)) HandleAction("right");
    }

    void HandleAction(string side)
    {
        switch (currentStage)
        {
            case Stage.PunchLeft:
                if (side == "left" && punchLeft < 10)
                {
                    punchLeft++;
                    instructionText.text = $"Punch Left: {punchLeft}/10";
                    if (punchLeft == 10)
                    {
                        PushEnemy(1.5f);
                        StartCoroutine(MoveHeroSmoothly(1f));
                        currentStage = Stage.PunchRight;
                        instructionText.text = $"Punch Right: {punchRight}/10";
                    }
                }
                break;

            case Stage.PunchRight:
                if (side == "right" && punchRight < 10)
                {
                    punchRight++;
                    instructionText.text = $"Punch Right: {punchRight}/10";
                    if (punchRight == 10)
                    {
                        PushEnemy(1.5f);
                        StartCoroutine(MoveHeroSmoothly(1f));
                        instructionText.text = "Punch Complete!";
                        currentStage = Stage.KickLeft;
                        Invoke(nameof(StartKickLeft), 1f);
                    }
                }
                break;

            case Stage.KickLeft:
                if (side == "left" && kickLeft < 10)
                {
                    kickLeft++;
                    instructionText.text = $"Kick Left: {kickLeft}/10";
                    if (kickLeft == 10)
                    {
                        currentStage = Stage.KickRight;
                        instructionText.text = $"Kick Right: {kickRight}/10";
                    }
                }
                break;

            case Stage.KickRight:
                if (side == "right" && kickRight < 10)
                {
                    kickRight++;
                    instructionText.text = $"Kick Right: {kickRight}/10";
                    if (kickRight == 10)
                    {
                        PushEnemy(1.5f);
                        StartCoroutine(MoveHeroSmoothly(1f));
                        instructionText.text = "Kick Complete!";
                        currentStage = Stage.Dodge;
                        Invoke(nameof(StartDodge), 1f);
                    }
                }
                break;

            case Stage.Dodge:
                dodgeCount++;
                instructionText.text = $"Dodge Left/Right: {dodgeCount}/20";
                if (dodgeCount >= 20)
                {
                    PushEnemy(2.5f);
                    StartCoroutine(MoveHeroSmoothly(1.5f));
                    instructionText.text = "You are the real Knight!";
                    currentStage = Stage.Done;

                    if (enemy != null)
                        enemy.SetActive(false);

                    if (winText != null)
                    {
                        winText.text = "Winning!";
                        StartCoroutine(ShowWinTextFade());
                    }
                }
                break;
        }
    }

    void StartKickLeft() => instructionText.text = $"Kick Left: {kickLeft}/10";
    void StartDodge() => instructionText.text = $"Dodge Left/Right: {dodgeCount}/20";

    void PushEnemy(float distance)
    {
        if (enemy != null)
            enemy.transform.position += new Vector3(distance, 0, 0);
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