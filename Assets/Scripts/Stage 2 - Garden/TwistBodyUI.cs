using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TwistBodyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image jobPostingImage;
    [SerializeField] private TextMeshProUGUI storyText;
    private TMP_FontAsset thaiFont; // Reference to Thai font asset

    [Header("Story Configuration")]
    [SerializeField] private float autoChangeDelay = 3f;
    [SerializeField]
    private string[] storySequence = new string[] {
        "ป้ายประกาศงั้นหรอ",
        "รับสมัครอัศวินฝีมือดี",
        "ถ้าคิดว่าคุณเป็นคนแข็งแกร่งก็ลองมาวัดฝีมือสิ",
        "สนใจสมัครได้ที่ปราสาทอาณาจักรmovemove",
        "นี่่คือโอกาสที่ฉันจะกลายเป็นคนที่แข็งแกร่งอย่างที่ฝันไว้สักที",
        "แตะที่ใดก็ได้เพื่อไปต่อ..."
    };

    private int currentTextIndex = 0;
    private bool isAutoChanging = true;
    private Coroutine autoChangeCoroutine;
    private ChallengeTriggerZone challengeZone;
    private StageManager stageManager;

    private void Start()
    {
        Debug.Log("[TwistBodyUI] Start called");

#if UNITY_EDITOR
        thaiFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Kanit/Kanit-Regular SDF.asset");
        if (thaiFont != null)
        {
            Debug.Log("[TwistBodyUI] Successfully loaded Thai font from AssetDatabase.");
        }
        else
        {
            Debug.LogError("[TwistBodyUI] ❌ Failed to load Thai font. Please check the path.");
        }
#endif

        if (storyText != null)
        {
            Debug.Log("[TwistBodyUI] storyText component found");
            if (thaiFont != null)
            {
                Debug.Log("[TwistBodyUI] Applying Thai font to storyText.");
                storyText.font = thaiFont;
            }

            if (storySequence.Length > 0)
            {
                storyText.text = storySequence[0];
                StartAutoChange();
            }
            else
            {
                Debug.LogError("[TwistBodyUI] Story sequence is empty!");
            }
        }
        else
        {
            Debug.LogError("[TwistBodyUI] storyText is not assigned!");
        }
    }

    public void Initialize(ChallengeTriggerZone zone, StageManager manager)
    {
        challengeZone = zone;
        stageManager = manager;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            AdvanceText();
        }
    }

    private void StartAutoChange()
    {
        if (autoChangeCoroutine != null) StopCoroutine(autoChangeCoroutine);
        autoChangeCoroutine = StartCoroutine(AutoChangeText());
    }

    private IEnumerator AutoChangeText()
    {
        while (isAutoChanging && currentTextIndex < storySequence.Length - 1)
        {
            yield return new WaitForSeconds(autoChangeDelay);
            AdvanceText();
        }
    }

    private void AdvanceText()
    {
        currentTextIndex++;
        if (currentTextIndex >= storySequence.Length)
        {
            currentTextIndex = storySequence.Length - 1;
            isAutoChanging = false;
            if (autoChangeCoroutine != null)
            {
                StopCoroutine(autoChangeCoroutine);
                autoChangeCoroutine = null;
            }

            if (stageManager != null && challengeZone != null)
            {
                stageManager.OnTwistStoryComplete(challengeZone);
            }
            gameObject.SetActive(false);
        }
        else if (storyText != null)
        {
            storyText.text = storySequence[currentTextIndex];
        }
    }

    public void CloseUI()
    {
        if (autoChangeCoroutine != null) StopCoroutine(autoChangeCoroutine);
        gameObject.SetActive(false);
    }
}