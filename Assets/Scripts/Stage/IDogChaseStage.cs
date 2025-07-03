namespace Core
{
    public interface IDogChaseStage
    {
        bool IsActive { get; }
        bool IsTransitioning { get; }
        void StartDogChase();
        void CompleteDogChase(bool success);
        void ResetDogPosition();
        void StartDogAnimation(bool isRunning);
        void TransitionToNextScene();
    }
} 