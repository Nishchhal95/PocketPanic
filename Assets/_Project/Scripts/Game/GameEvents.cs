using System;

public static class GameEvents
{
    public static event Action OnGameStarted;

    public static void RaiseGameStarted()
    {
        OnGameStarted?.Invoke();
    }
}
