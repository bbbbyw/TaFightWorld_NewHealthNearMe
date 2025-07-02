using UnityEngine;
using System.Collections.Generic;

public class StageBlockManager : MonoBehaviour
{
    [Header("Stage Configuration")]
    [SerializeField] private List<GameObject> storyModeSequence;
    [SerializeField] private List<GameObject> randomPool;
    [SerializeField] private float blockWidth = 20f;
    [SerializeField] private bool isShuffleMode;
    [SerializeField] private float jumpForce = 17f;

    [Header("References")]
    [SerializeField] private Transform prefabSpawnPoint;
    [SerializeField] private Rigidbody2D playerRigidbody;

    private Queue<GameObject> activeStages = new Queue<GameObject>();
    private int currentStoryIndex = 0;
    private System.Random random;

    private void Start()
    {
        random = new System.Random();
        SpawnInitialStage();
    }

    private void SpawnInitialStage()
    {
        if (storyModeSequence == null || storyModeSequence.Count == 0)
        {
            Debug.LogError("No stage prefabs assigned to story mode sequence!");
            return;
        }

        GameObject initialStage = Instantiate(
            isShuffleMode ? GetRandomStagePrefab() : storyModeSequence[0],
            prefabSpawnPoint.position,
            Quaternion.identity
        );

        activeStages.Enqueue(initialStage);
        SetupStageColliders(initialStage);
    }

    public void SpawnNextStage()
    {
        if (activeStages.Count == 0) return;

        // Calculate spawn position based on the last stage
        Vector3 lastStagePosition = activeStages.Peek().transform.position;
        Vector3 spawnPosition = lastStagePosition + Vector3.right * blockWidth;

        // Get next prefab
        GameObject nextStagePrefab = null;
        if (isShuffleMode)
        {
            nextStagePrefab = GetRandomStagePrefab();
        }
        else
        {
            currentStoryIndex++;
            if (currentStoryIndex >= storyModeSequence.Count)
            {
                if (isShuffleMode)
                {
                    // In shuffle mode, continue with random stages
                    nextStagePrefab = GetRandomStagePrefab();
                }
                else
                {
                    Debug.Log("Reached end of story sequence!");
                    return;
                }
            }
            else
            {
                nextStagePrefab = storyModeSequence[currentStoryIndex];
            }
        }

        // Spawn new stage
        if (nextStagePrefab != null)
        {
            GameObject newStage = Instantiate(nextStagePrefab, spawnPosition, Quaternion.identity);
            activeStages.Enqueue(newStage);
            SetupStageColliders(newStage);

            // Remove oldest stage if we have more than 2 active
            if (activeStages.Count > 2)
            {
                GameObject oldestStage = activeStages.Dequeue();
                Destroy(oldestStage);
            }
        }
    }

    private GameObject GetRandomStagePrefab()
    {
        if (randomPool == null || randomPool.Count == 0)
        {
            Debug.LogError("No stage prefabs in random pool!");
            return null;
        }
        return randomPool[random.Next(randomPool.Count)];
    }

    private void SetupStageColliders(GameObject stage)
    {
        // Setup stage transition trigger
        BoxCollider2D transitionTrigger = stage.AddComponent<BoxCollider2D>();
        transitionTrigger.isTrigger = true;
        transitionTrigger.size = new Vector2(1f, 10f); // Tall trigger zone
        transitionTrigger.offset = new Vector2(blockWidth * 0.4f, 0f); // Place near the end of stage

        // Find all JumpPoint objects and setup their triggers
        foreach (Transform child in stage.transform)
        {
            if (child.CompareTag("JumpPoint"))
            {
                BoxCollider2D jumpTrigger = child.gameObject.AddComponent<BoxCollider2D>();
                jumpTrigger.isTrigger = true;
                jumpTrigger.size = new Vector2(2f, 2f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject triggerObject = other.GetComponent<Collider2D>().gameObject;
            
            // Check if it's a stage transition trigger
            if (triggerObject.GetComponent<BoxCollider2D>() != null)
            {
                SpawnNextStage();
            }
            // Check if it's a jump point
            else if (triggerObject.CompareTag("JumpPoint") && playerRigidbody != null)
            {
                playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
        }
    }
} 