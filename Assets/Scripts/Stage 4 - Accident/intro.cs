using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class IntroManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject introPanel;
    public GameObject startPanel;

    [Header("Buttons")]
    public Button skipButton;

    [Header("Text")]
    public TextMeshProUGUI storyText;

    [TextArea]
    public string fullStory = "การผจญภัยของคุณกำลังจะเริ่มต้นขึ้น...\n\nในเส้นทางที่จะหล่อหลอมให้คุณกลายเป็นอัศวินผู้กล้าหาญ!";
    public float typingSpeed = 0.05f;

    private bool isTyping = false;

    public SquatCounterUI squatCounter;

    void Start()
    {
        introPanel.SetActive(true);
        startPanel.SetActive(false);

        skipButton.onClick.AddListener(SkipIntro);

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
        skipButton.gameObject.SetActive(false);
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
