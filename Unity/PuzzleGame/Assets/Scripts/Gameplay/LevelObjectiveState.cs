using System;

public sealed class LevelObjectiveState
{
    public LevelObjectiveState(int requiredCollectibles)
    {
        RequiredCollectibles = Math.Max(0, requiredCollectibles);
    }

    public int RequiredCollectibles { get; }
    public int CollectedCollectibles { get; private set; }
    public bool IsPortalActive => CollectedCollectibles >= RequiredCollectibles;

    public event Action<LevelObjectiveState> Changed;
    public event Action PortalActivated;

    public bool CollectOne()
    {
        if (CollectedCollectibles >= RequiredCollectibles)
            return false;

        bool wasPortalActive = IsPortalActive;
        CollectedCollectibles++;

        Changed?.Invoke(this);

        if (!wasPortalActive && IsPortalActive)
            PortalActivated?.Invoke();

        return true;
    }
}
