using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShuffleModeConfig", menuName = "Game/Shuffle Mode Config")]
public class ShuffleModeConfig : ScriptableObject
{
    [Header("UI Configuration")]
    public GameObject shuffleModeUIPrefab;

    [Header("Stage Configuration")]
    public List<string> stageScenes = new List<string>();
    public int maxFailCount = 3;
} 