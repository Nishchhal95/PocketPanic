using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class GameSyncTimer : IDisposable
{
    private float timerDuration;
    private Action<float, float> onTimerTick;
    private Action onTimerComplete;
    private Action onTimerCancelled;
    
    private double pauseStartTime = 0;
    private double totalPausedDuration = 0;
    private bool isPaused;
    
    private CancellationTokenSource cancellationTokenSource;
    
    public GameSyncTimer(float duration, Action<float, float> onTimerTick = null, Action onTimerComplete = null, Action onTimerCancelled = null)
    {
        timerDuration = duration;
        this.onTimerTick = onTimerTick;
        this.onTimerComplete = onTimerComplete;
        this.onTimerCancelled = onTimerCancelled;
    }
    
    public async Task Start()
    {
        Cancel();
        cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = cancellationTokenSource.Token;

        double startTime = PhotonNetworkController.GetPhotonTime();
        double endTime = startTime + timerDuration;
        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                
                if (isPaused)
                {
                    await Task.Yield();
                    continue;
                }

                double now = PhotonNetworkController.GetPhotonTime();
                double effectiveTime = now - totalPausedDuration;
                double remaining = endTime - effectiveTime;

                if (remaining <= 0f)
                {
                    break;
                }

                float progress = Mathf.Clamp01((float)remaining / timerDuration);
                onTimerTick?.Invoke((float)remaining, progress);
                await Task.Yield();
            }

            onTimerComplete?.Invoke();
        }
        catch (OperationCanceledException)
        {
            Logger.Log("Timer Cancelled");
            onTimerCancelled?.Invoke();
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            Logger.Error(e.StackTrace);
            throw;
        }
    }

    public void Pause()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;
        pauseStartTime = PhotonNetworkController.GetPhotonTime();
    }

    public void Resume()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;

        double resumedAt = PhotonNetworkController.GetPhotonTime();
        totalPausedDuration += resumedAt - pauseStartTime;
    }

    public void Restart()
    {
        Cancel();
        ResetState();
        _ = Start();
    }

    public void Cancel()
    {
        cancellationTokenSource?.Cancel();
    }

    public void Dispose()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }
    }

    private void ResetState()
    {
        isPaused = false;
        totalPausedDuration = 0;
        pauseStartTime = 0;
    }
}
