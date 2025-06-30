using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SquatCounterUI : MonoBehaviour
{
    [Header("Squat Counting")]
    public TextMeshProUGUI counterText;
    public int maxSquats = 15;

    [Header("Running Phase")]
    public GameObject hero;
    public GameObject vehicle;
    public float runDuration = 20f;

    [Header("Rest Phase")]
    public float restDuration = 30f;

    private int currentSquats = 0;
    private bool isCounting = false;
    private bool isResting = false;
    private bool isRunning = false;
    private float runTimeLeft = 0f;
    private float restTimeLeft = 0f;

    private Vector3 heroStartPos;
    private Vector3 runTargetPos;
    private Animator heroAnimator;

    void OnEnable()
    {
        currentSquats = 0;
        isCounting = false;
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

        if (counterText != null)
        {
            counterText.text = $"Squat : {currentSquats} / {maxSquats}";
        }
        else
        {
            Debug.LogError(">> counterText is NULL (OnEnable)");
        }
    }

    void Update()
    {
        if (counterText != null && Camera.main != null)
            counterText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        if (isResting)
        {
            restTimeLeft -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(restTimeLeft);
            counterText.text = $"Take a rest: {seconds} sec";

            if (restTimeLeft <= 0f)
            {
                isResting = false;
                StartRunning();
            }
            return;
        }

        if (isRunning)
        {
            runTimeLeft -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(runTimeLeft);
            counterText.text = $"Count Running : {seconds} sec";

            if (hero != null && vehicle != null)
            {
                float totalDistance = Vector3.Distance(heroStartPos, runTargetPos);
                float step = totalDistance / runDuration * Time.deltaTime;
                Vector3 flatTarget = new Vector3(runTargetPos.x, hero.transform.position.y, hero.transform.position.z);
                hero.transform.position = Vector3.MoveTowards(hero.transform.position, flatTarget, step);
            }

            if (runTimeLeft <= 0f)
            {
                isRunning = false;
                counterText.text = "Complete!";
                Invoke(nameof(LoadNextScene), 2f);
            }
            return;
        }

        if (!isCounting) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentSquats++;
            if (currentSquats > maxSquats) currentSquats = maxSquats;

            if (counterText != null)
                counterText.text = $"Squat : {currentSquats} / {maxSquats}";
            else
                Debug.LogError(">> counterText is NULL in Update");

            if (currentSquats >= maxSquats)
            {
                isCounting = false;
                StartResting();
            }
        }
    }

    public void StartCounting()
    {
        currentSquats = 0;
        isCounting = true;

        if (counterText != null)
        {
            counterText.gameObject.SetActive(true);
            counterText.text = $"Squat : {currentSquats} / {maxSquats}";
        }
        else
        {
            Debug.LogError("counterText not here");
        }
    }

    private void StartResting()
    {
        isResting = true;
        restTimeLeft = restDuration;

        if (counterText != null)
        {
            counterText.text = $"Take a rest: {Mathf.CeilToInt(restTimeLeft)} sec";
        }
    }

    private void StartRunning()
    {
        isRunning = true;
        runTimeLeft = runDuration;

        if (counterText != null)
        {
            counterText.text = "Start Running!!";
        }

        if (hero != null && vehicle != null)
        {
            heroStartPos = hero.transform.position;
            runTargetPos = vehicle.transform.position;
        }

        if (heroAnimator != null)
        {
            Debug.Log(">> SetTrigger(Run) called");
            heroAnimator.SetTrigger("Run");

            Debug.Log(">> Current State (just after SetTrigger): " +
                heroAnimator.GetCurrentAnimatorStateInfo(0).IsName("HeroKnight_Run"));
        }
        else
        {
            Debug.LogError(">> heroAnimator is null in StartRunning!");
        }

        Invoke(nameof(BeginRunCountdown), 1f);
    }

    private void BeginRunCountdown()
    {
        Debug.Log(">> Running countdown started...");
    }

    private void LoadNextScene()
    {
        Debug.Log(">> Loading next scene: Win");
        SceneManager.LoadScene("Win");
    }
}
