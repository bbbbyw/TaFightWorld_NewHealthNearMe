using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TwistBodyUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image jobPostingImage;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TMP_FontAsset thaiFont; // Reference to Thai font asset
    
    [Header("Story Configuration")]
    [SerializeField] private float autoChangeDelay = 3f;
    [SerializeField] private string[] storySequence = new string[] {
        "You found a job posting!",
        "It seems like a perfect opportunity...",
        "The requirements match your skills...",
        "Would you like to apply?",
        "Tap anywhere to continue..."
    };

    private int currentTextIndex = 0;
    private bool isAutoChanging = true;
    private Coroutine autoChangeCoroutine;
    private ChallengeTriggerZone challengeZone;
    private StageManager stageManager;

    private void Awake()
    {
        Debug.Log("[TwistBodyUI] Awake called");
        // Check components
        if (storyText == null)
        {
            Debug.LogError("[TwistBodyUI] storyText is not assigned!");
        }
        if (jobPostingImage == null)
        {
            Debug.LogError("[TwistBodyUI] jobPostingImage is not assigned!");
        }
    }

    private void Start()
    {
        Debug.Log("[TwistBodyUI] Start called");
        // Set Thai font if available
        if (storyText != null)
        {
            Debug.Log("[TwistBodyUI] storyText component found");
            if (thaiFont != null)
            {
                Debug.Log("[TwistBodyUI] Setting Thai font");
                storyText.font = thaiFont;
            }
            else
            {
                Debug.LogWarning("[TwistBodyUI] Thai font asset is not assigned!");
            }
            
            if (storySequence.Length > 0)
            {
                Debug.Log($"[TwistBodyUI] Setting initial text: {storySequence[0]}");
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
            Debug.LogError("[TwistBodyUI] Cannot set text - storyText is null!");
        }
    }

    public void Initialize(ChallengeTriggerZone zone, StageManager manager)
    {
        Debug.Log("[TwistBodyUI] Initialize called");
        challengeZone = zone;
        stageManager = manager;
        
        if (zone == null)
        {
            Debug.LogError("[TwistBodyUI] ChallengeTriggerZone is null in Initialize!");
        }
        if (manager == null)
        {
            Debug.LogError("[TwistBodyUI] StageManager is null in Initialize!");
        }
    }

    private void Update()
    {
        // Check for touch/click input
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Debug.Log("[TwistBodyUI] Input detected - advancing text");
            AdvanceText();
        }
    }

    private void StartAutoChange()
    {
        if (autoChangeCoroutine != null)
        {
            StopCoroutine(autoChangeCoroutine);
        }
        autoChangeCoroutine = StartCoroutine(AutoChangeText());
        Debug.Log("[TwistBodyUI] Started auto-change coroutine");
    }

    private IEnumerator AutoChangeText()
    {
        while (isAutoChanging && currentTextIndex < storySequence.Length - 1)
        {
            yield return new WaitForSeconds(autoChangeDelay);
            Debug.Log("[TwistBodyUI] Auto-advancing text");
            AdvanceText();
        }
    }

    private void AdvanceText()
    {
        currentTextIndex++;
        Debug.Log($"[TwistBodyUI] Advancing to text index: {currentTextIndex}");
        
        if (currentTextIndex >= storySequence.Length)
        {
            currentTextIndex = storySequence.Length - 1;
            isAutoChanging = false;
            if (autoChangeCoroutine != null)
            {
                StopCoroutine(autoChangeCoroutine);
                autoChangeCoroutine = null;
            }

            Debug.Log("[TwistBodyUI] Reached end of story sequence");
            // When we reach the last text, notify StageManager
            if (stageManager != null && challengeZone != null)
            {
                Debug.Log("[TwistBodyUI] Notifying StageManager of story completion");
                stageManager.OnTwistStoryComplete(challengeZone);
            }
            else
            {
                Debug.LogError("[TwistBodyUI] Cannot notify completion - stageManager or challengeZone is null!");
            }
            
            // Hide this UI
            Debug.Log("[TwistBodyUI] Hiding UI");
            gameObject.SetActive(false);
        }
        else if (storyText != null)
        {
            Debug.Log($"[TwistBodyUI] Setting text to: {storySequence[currentTextIndex]}");
            storyText.text = storySequence[currentTextIndex];
        }
        else
        {
            Debug.LogError("[TwistBodyUI] Cannot advance text - storyText is null!");
        }
    }

    // Call this method to close the UI
    public void CloseUI()
    {
        Debug.Log("[TwistBodyUI] CloseUI called");
        if (autoChangeCoroutine != null)
        {
            StopCoroutine(autoChangeCoroutine);
        }
        gameObject.SetActive(false);
    }
} 