using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Photon.Realtime;
using UnityEngine;

public class TurnManager
{
    private const float TURN_DURATION = 10f;
    private const float PLACE_EXPLODE_BACK_DURATION = 10f;
    public int CurrentTurn
    {
        get => currentTurn;
        private set => currentTurn = value;
    }
    
    public bool IsMyTurn => currentRoomPlayers[CurrentTurn].IsLocal;

    private int currentTurn = -1;
    private List<int> deadActors = new();
    
    private Player[] currentRoomPlayers;
    private Dictionary<int, GamePlayer> actorIdToGamePlayerMap;
    private GameCanvasController gameCanvasController;
    private Action onTurnTimeEnded;

    private CancellationTokenSource turnTimerCancellationTokenSource;

    public TurnManager(Player[] currentRoomPlayers, Dictionary<int, GamePlayer> actorIdToGamePlayerMap, 
        GameCanvasController gameCanvasController, Action onTurnTimeEnded)
    {
        this.currentRoomPlayers = currentRoomPlayers;
        this.actorIdToGamePlayerMap = actorIdToGamePlayerMap;
        this.gameCanvasController = gameCanvasController;
        this.onTurnTimeEnded = onTurnTimeEnded;
    }

    public void Dispose()
    {
        turnTimerCancellationTokenSource?.Cancel();
        turnTimerCancellationTokenSource?.Dispose();
    }

    public void MarkDead(int actorNumber)
    {
        if (deadActors.Contains(actorNumber))
        {
            Player deadPlayer = currentRoomPlayers.ToList().Find(x => x.ActorNumber == actorNumber);
            Logger.Error($"{deadPlayer.NickName} is already marked dead!");
            return;
        }
        
        deadActors.Add(actorNumber);
    }
    
    public void EndTurn()
    {
        if (currentTurn >= 0)
        {
            Logger.Log($"Turn Ended for {GetPhotonPlayerForCurrentTurn().NickName}");
        }

        int currentTurnPlayerActorIndex;
        do
        {
            currentTurn++;
            currentTurn %= currentRoomPlayers.Length;
            currentTurnPlayerActorIndex = GetPhotonPlayerForCurrentTurn().ActorNumber;

        } while (deadActors.Contains(currentTurnPlayerActorIndex));

        UpdateTurnUI();
    }
    
    public void SetActorsTurn(int targetActorNumber)
    {
        Logger.Log($"Turn Ended for {GetPhotonPlayerForCurrentTurn().NickName}");
        
        // Wraps it so it starts from 1 till player count, NOT FROM 0
        targetActorNumber = (targetActorNumber - 1) % currentRoomPlayers.Length + 1;
        int arrayIndex = currentRoomPlayers.ToList().FindIndex(p => p.ActorNumber == targetActorNumber);
        currentTurn = arrayIndex;

        UpdateTurnUI();
    }
    
    public Player GetPhotonPlayerForCurrentTurn()
    {
        return currentRoomPlayers[currentTurn];
    }

    public GamePlayer GetGamePlayerForCurrentTurn()
    {
        Player currentTurnPlayer = GetPhotonPlayerForCurrentTurn();
        int currentTurnPlayerActorNumber = currentTurnPlayer.ActorNumber;
        return actorIdToGamePlayerMap[currentTurnPlayerActorNumber];
    }

    public void StopTurnTimer()
    {
        turnTimerCancellationTokenSource?.Cancel();
    }

    private void UpdateTurnUI()
    {
        turnTimerCancellationTokenSource?.Cancel();
        turnTimerCancellationTokenSource?.Dispose();
        turnTimerCancellationTokenSource = new CancellationTokenSource();
        
        foreach (GamePlayer gamePlayer in actorIdToGamePlayerMap.Values)
        {
            gamePlayer.EndTurn();
        }
        GamePlayer currentTurnGamePlayer = GetGamePlayerForCurrentTurn();
        currentTurnGamePlayer.StartTurn();

        _ = StartTurnTimerAsync(turnTimerCancellationTokenSource.Token);
        
        gameCanvasController.UpdateTurn(GetPhotonPlayerForCurrentTurn().NickName);
    }

    private async Task StartTurnTimerAsync(CancellationToken token)
    {
        double turnEndTime = PhotonNetworkController.GetPhotonTime() + TURN_DURATION;
        Logger.Log($"{GetPhotonPlayerForCurrentTurn().NickName} Turn Starting at {PhotonNetworkController.GetPhotonTime()} and ending at {turnEndTime}");

        try
        {
            while (PhotonNetworkController.GetPhotonTime() < turnEndTime)
            {
                token.ThrowIfCancellationRequested();

                float remaining = (float)(turnEndTime - PhotonNetworkController.GetPhotonTime());
                float progress = Mathf.Clamp01(remaining / TURN_DURATION);
                GetGamePlayerForCurrentTurn().UpdateTurnTime(progress);
                await Task.Yield();
            }

            // If timer finished
            if (IsMyTurn)
            {
                onTurnTimeEnded?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Log("Turn timer stopped");
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            throw;
        }
    }
    
    public async Task StartExplodePuttingBackTimerAsync(CancellationToken token, Action onTimerEndedAction)
    {
        double turnEndTime = PhotonNetworkController.GetPhotonTime() + PLACE_EXPLODE_BACK_DURATION;
        Logger.Log($"{GetPhotonPlayerForCurrentTurn().NickName} Turn Starting at {PhotonNetworkController.GetPhotonTime()} and ending at {turnEndTime}");

        try
        {
            while (PhotonNetworkController.GetPhotonTime() < turnEndTime)
            {
                token.ThrowIfCancellationRequested();

                float remaining = (float)(turnEndTime - PhotonNetworkController.GetPhotonTime());
                float progress = Mathf.Clamp01(remaining / PLACE_EXPLODE_BACK_DURATION);
                GetGamePlayerForCurrentTurn().UpdateTurnTime(progress);
                await Task.Yield();
            }

            // If timer finished
            if (IsMyTurn)
            {
                onTimerEndedAction?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            Logger.Log("PLACE_EXPLODE_BACK timer stopped");
        }
        catch (Exception e)
        {
            Logger.Error(e.Message);
            throw;
        }
    }
}
