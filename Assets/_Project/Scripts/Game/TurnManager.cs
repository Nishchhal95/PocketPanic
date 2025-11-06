using System.Collections.Generic;
using System.Linq;
using Photon.Realtime;

public class TurnManager
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

    public TurnManager(Player[] currentRoomPlayers, Dictionary<int, GamePlayer> actorIdToGamePlayerMap, 
        GameCanvasController gameCanvasController)
    {
        this.currentRoomPlayers = currentRoomPlayers;
        this.actorIdToGamePlayerMap = actorIdToGamePlayerMap;
        this.gameCanvasController = gameCanvasController;
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

    private void UpdateTurnUI()
    {
        gameCanvasController.UpdateTurn(GetPhotonPlayerForCurrentTurn().NickName);
    }
}
