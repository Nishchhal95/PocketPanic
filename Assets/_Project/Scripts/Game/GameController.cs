using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameController : MonoBehaviourPun
{
    public static GameController Instance { get; private set; }

    [SerializeField] private DeckController deckController;
    [SerializeField] private GameCanvasController gameCanvasController;
    [SerializeField] private GamePlayer localGamePlayerPrefab;
    [SerializeField] private GamePlayer remoteGamePlayerPrefab;

    private Dictionary<int, Player> actorIdToPhotonPlayerMap = new();
    private Dictionary<int, GamePlayer> actorIdToGamePlayerMap = new();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TestCardsSpreadNoNetwork();
    }

    private void TestCardsSpreadNoNetwork()
    {
        int numberOfCards = 10;
        List<Transform> playerSlots = GetPlayerSlotsFromPlayerCount(2);
        GamePlayer gamePlayer = Instantiate(localGamePlayerPrefab, playerSlots[0]);
        gamePlayer.Init("Demo", 1, "DemoUserId");

        deckController.BuildDeckWithoutExplodeAndDiffuse();
        deckController.Shuffle(1);
        List<CardType> cards = new List<CardType>(numberOfCards) { CardType.Defuse };
        for (int j = 0; j < numberOfCards - 1; j++)
        {
            cards.Add(deckController.DrawCardTop());
        }
        gamePlayer.InitCards(cards, true);
    }

    #region NetworkCalls

    public void SendStartGameToAll()
    {
        if (!PhotonNetworkController.IsMasterClient())
        {
            Logger.Error("Only Master Clients can Start the Game");
            return;
        }
        
        // Generate and Shuffle Deck for all players
        int randomSeedInitial = UnityEngine.Random.Range(1, 99999);
        int randomSeedFinal = UnityEngine.Random.Range(1, 99999);
        photonView.RPC(nameof(GameStartedRPC), RpcTarget.All, randomSeedInitial, randomSeedFinal);
    }

    #endregion

    #region RPCs
    
    [PunRPC]
    private void GameStartedRPC(int seedInitial, int seedFinal)
    {
        GameEvents.RaiseGameStarted();
        SetupGame(seedInitial, seedFinal);
    }

    #endregion
    

    private void SetupGame(int seedInitial, int seedFinal)
    {
        deckController.BuildDeckWithoutExplodeAndDiffuse();
        deckController.Shuffle(seedInitial);
        
        SpawnPlayers();
        DistributeCards();
        
        deckController.AddExplode(PhotonNetworkController.GetPlayerCountInCurrentRoom());
        deckController.AddDefuse(2);
        deckController.Shuffle(seedFinal);
    }

    private void SpawnPlayers()
    {
        Player[] players = PhotonNetworkController.GetPlayersCurrentRoom();
        List<Transform> playerSlots = GetPlayerSlotsFromPlayerCount(players.Length);
        
        int localPlayerIndex = Array.FindIndex(players, p => p.IsLocal);

        for (int i = 0; i < players.Length; i++)
        {
            // This makes sure we always start with Local Player to make them seat always first which is in the bottom
            int playerIndex = (localPlayerIndex + i) % players.Length;
            Player player = players[playerIndex];
            Transform playerSlot = playerSlots[i];

            GamePlayer gamePlayer = Instantiate(player.IsLocal ? localGamePlayerPrefab : remoteGamePlayerPrefab, playerSlot);
            gamePlayer.Init(player.NickName, player.ActorNumber, player.UserId);
            
            actorIdToPhotonPlayerMap.Add(player.ActorNumber, player);
            actorIdToGamePlayerMap.Add(player.ActorNumber, gamePlayer);
        }
    }

    private void DistributeCards()
    {
        Player[] players = PhotonNetworkController.GetPlayersCurrentRoom();

        foreach (Player player in players)
        {
            List<CardType> cards = new List<CardType>(8) { CardType.Defuse };
            for (int j = 0; j < 7; j++)
            {
                cards.Add(deckController.DrawCardTop());
            }
            actorIdToGamePlayerMap[player.ActorNumber].InitCards(cards, player.IsLocal);
        }
    }

    private List<Transform> GetPlayerSlotsFromPlayerCount(int playerCount)
    {
        foreach (PlayerCountToSlots item in gameCanvasController.PlayerCountToSlotsArray)
        {
            if (item.PlayerCount == playerCount)
            {
                return item.PlayerSlotTransforms;
            }
        }

        Logger.Error($"Could not find Slots Map for {playerCount} Players");
        return null;
    }
}
