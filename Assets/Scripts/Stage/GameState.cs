// Class to hold save data (can be serialized)
[System.Serializable]
public class GameState
{
    public bool IsCompleted;
    public int CurrentFailCount;
    public bool HasShownCompletion;
    public int StarCount;
} 
