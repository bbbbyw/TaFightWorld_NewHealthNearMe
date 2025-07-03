using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class IntroManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject introPanel;

    [Header("Buttons")]
    public Button skipButton;

    [Header("Text")]
    public TextMeshProUGUI storyText;

    [TextArea]
    public string fullStory = "การผจญภัยของคุณกำลังจะเริ่มต้นขึ้น\nในเส้นทางที่จะหล่อหลอมให้คุณกลายเป็นอัศวินผู้กล้าหาญ!\nแต่รถม้าคุณดันคว่ำรีบเก็บของแล้ววิ่งไปรถม้าเร็ว!";
    public float typingSpeed = 0.05f;

    [Header("Audio")]
    public AudioSource introMusic;

    private bool isTyping = false;

    public SquatCounterUI squatCounter;

    void Start()
    {
        introPanel.SetActive(true);

        if (introMusic != null)
        {   
            introMusic.loop = true;
            introMusic.Play();
        }
        else
        {
            Debug.LogWarning("🎵 Intro Music AudioSource is not assigned!");
        }

        StartCoroutine(TypeStory());
        skipButton.onClick.AddListener(SkipIntro);
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
        StartGame();
    }

    public void StartGame()
    {
        if (introMusic != null && introMusic.isPlaying)
        {
            introMusic.Stop();
        }

        if (squatCounter != null)
        {
            squatCounter.StartSquat();
        }
        else
        {
            Debug.LogError("SquatCounter is not referenced in IntroManager!");
        }
    }
}