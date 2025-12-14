using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;
using UnityEngine;

public class TurnManager : IDisposable
{
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

    private GameSyncTimer turnTimer;

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
        turnTimer?.Dispose();
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
        Debug.Log("<<<<<<<EndTurn");
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

    public void PauseTimer()
    {
        turnTimer?.Pause();
    }

    public void ResumeTimer()
    {
        turnTimer.Resume();
    }

    public void RestartTimer()
    {
        turnTimer.Restart();
    }

    private void UpdateTurnUI()
    {
        foreach (GamePlayer gamePlayer in actorIdToGamePlayerMap.Values)
        {
            gamePlayer.EndTurn();
        }
        GamePlayer currentTurnGamePlayer = GetGamePlayerForCurrentTurn();
        currentTurnGamePlayer.StartTurn();

        turnTimer?.Dispose();
        turnTimer = new GameSyncTimer(GameConfig.TURN_DURATION, UpdateGameTimerForTurnPlayer, 
            OnTimerComplete, null);
        _ = turnTimer.Start();
        gameCanvasController.UpdateTurn(GetPhotonPlayerForCurrentTurn().NickName);
    }

    private void UpdateGameTimerForTurnPlayer(float remaining, float progress)
    {
        GetGamePlayerForCurrentTurn().UpdateTurnTime(progress);
    }

    private void OnTimerComplete()
    {
        if (IsMyTurn)
        {
            onTurnTimeEnded?.Invoke();
        }
    }
}
