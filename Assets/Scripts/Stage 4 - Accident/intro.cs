using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject introPanel;
    public GameObject startPanel;
    public GameObject videoPanel;

    [Header("Buttons")]
    public Button skipButton;
    public Button watchButton;
    public Button videoSkipButton;

    [Header("Text")]
    public TextMeshProUGUI storyText;

    [TextArea]
    public string fullStory = "การผจญภัยของคุณกำลังจะเริ่มต้นขึ้น...\n\nในเส้นทางที่จะหล่อหลอมให้คุณกลายเป็นอัศวินผู้กล้าหาญ!";
    public float typingSpeed = 0.05f;

    private bool isTyping = false;

    // ✅ เพิ่มตัวแปรอ้างถึง SquatCounterUI
    public SquatCounterUI squatCounter;

    void Start()
    {
        introPanel.SetActive(true);
        startPanel.SetActive(false);
        videoPanel.SetActive(false);

        skipButton.onClick.AddListener(SkipIntro);
        watchButton.onClick.AddListener(WatchVideo);
        videoSkipButton.onClick.AddListener(SkipVideo);

        StartCoroutine(TypeStory());
    }

    private System.Collections.IEnumerator TypeStory()
    {
        isTyping = true;
        storyText.text = "";

        foreach (char letter in fullStory.ToCharArray())
        {
            storyText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    public void SkipIntro()
    {
        StopAllCoroutines();
        introPanel.SetActive(false);
        videoPanel.SetActive(true);
    }

    public void WatchVideo()
    {
        introPanel.SetActive(false);
        videoPanel.SetActive(true);
    }

    public void SkipVideo()
    {
        videoPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    public void StartGame()
    {
        startPanel.SetActive(false);

        if (squatCounter != null)
        {
            squatCounter.StartCounting();
        }
        else
        {
            Debug.LogError("SquatCounter ไม่ได้อ้างถึงใน IntroManager!");
        }
    }
}
