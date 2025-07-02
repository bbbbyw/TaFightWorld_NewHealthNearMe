using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class TransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public Image transitionPanel; // Panel ที่ใช้ทำ fade effect
    public float fadeSpeed = 2f;
    public float holdTime = 0.2f; // เวลาที่จะ hold ตรงกลาง transition

    private void Start()
    {
        // ตั้งค่าเริ่มต้นให้ panel โปร่งใส
        if (transitionPanel != null)
        {
            Color startColor = transitionPanel.color;
            startColor.a = 0f;
            transitionPanel.color = startColor;
        }
    }

    // เรียกใช้เพื่อทำ transition พร้อมกับ callback
    public void DoTransition(UnityAction onTransitionMidPoint = null)
    {
        StartCoroutine(TransitionCoroutine(onTransitionMidPoint));
    }

    private IEnumerator TransitionCoroutine(UnityAction onTransitionMidPoint)
    {
        // Fade Out
        float elapsedTime = 0f;
        Color startColor = transitionPanel.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            transitionPanel.color = Color.Lerp(startColor, targetColor, elapsedTime);
            yield return null;
        }

        // Hold at mid point
        yield return new WaitForSeconds(holdTime);

        // Execute callback at mid point
        onTransitionMidPoint?.Invoke();

        // Fade In
        elapsedTime = 0f;
        startColor = transitionPanel.color;
        targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            transitionPanel.color = Color.Lerp(startColor, targetColor, elapsedTime);
            yield return null;
        }
    }
} 