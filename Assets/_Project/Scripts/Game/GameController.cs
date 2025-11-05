using System;
using System.Collections;
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

    // Turn Variable start from 1 so we can make Turn and ActorNumber as interchangable.
    private int currentTurn = 0;
    
    public bool IsMyTurn => PhotonNetworkController.GetLocalPlayer().ActorNumber == currentTurn;

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

    private void OnEnable()
    {
        gameCanvasController.CardDeckClicked += LocalPlayerPicksACard;
    }

    private void OnDisable()
    {
        gameCanvasController.CardDeckClicked -= LocalPlayerPicksACard;
    }

    private void Start()
    {
        //TestCardsSpreadNoNetwork();
    }

    private void TestCardsSpreadNoNetwork()
    {
        int numberOfCards = 10;
        List<Transform> playerSlots = GetPlayerSlotsFromPlayerCount(2);
        GamePlayer gamePlayer = Instantiate(localGamePlayerPrefab, playerSlots[0]);
        gamePlayer.Init("Demo", 1, "DemoUserId");

        deckController.BuildDeckWithoutExplodeAndDiffuse();
        deckController.Shuffle(1);
        List<CardType> cards = new List<CardType>(numberOfCards) 
            {   CardType.Defuse, 
                CardType.BeardCat, 
                CardType.BeardCat, 
                CardType.BeardCat, 
                CardType.WildCat, 
                CardType.TacoCat, 
                CardType.TacoCat, 
                CardType.Shuffle,
                CardType.Attack,
                CardType.Attack
            };
        // for (int j = 0; j < numberOfCards - 1; j++)
        // {
        //     cards.Add(deckController.DrawCardTop());
        // }
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

    private void SendEndTurnToAll()
    {
        photonView.RPC(nameof(EndTurnRPC), RpcTarget.All);
    }
    
    private void SendSetTurnForAll(int turn)
    {
        photonView.RPC(nameof(SetTurnRPC), RpcTarget.All, turn);
    }

    private void SendCardPickedVisualToAll()
    {
        photonView.RPC(nameof(CardPickedVisualRPC), RpcTarget.All);
    }

    private void SendCardPlayerVisualToAll(int actorNumber, CardType cardType)
    {
        photonView.RPC(nameof(CardPlayedVisualRPC), RpcTarget.All, actorNumber, (int)cardType);
    }

    private void SendShuffleToAll(int randomSeed)
    {
        photonView.RPC(nameof(ShuffleDeckRPC), RpcTarget.All, randomSeed);
    }

    private void SendForcePickCardsForCurrentTurn(int count)
    {
        photonView.RPC(nameof(ForcePickCardsForCurrentTurnRPC), RpcTarget.All, count);
    }

    #endregion

    #region RPCs
    
    [PunRPC]
    private void GameStartedRPC(int seedInitial, int seedFinal)
    {
        GameEvents.RaiseGameStarted();
        SetupGame(seedInitial, seedFinal);
    }

    [PunRPC]
    private void EndTurnRPC()
    {
        SetCurrentTurn(currentTurn++);
    }
    
    [PunRPC]
    private void SetTurnRPC(int turn)
    {
        SetCurrentTurn(turn);
    }
    
    [PunRPC]
    private void CardPickedVisualRPC()
    {
        CardType cardType = deckController.DrawCardTop();
        actorIdToGamePlayerMap[currentTurn].AddCard(cardType);
    }
    
    [PunRPC]
    private void CardPlayedVisualRPC(int actorNumber, int cardType)
    {
        // Locally we remove card by Instance GUID
        if (actorNumber == PhotonNetworkController.GetLocalPlayer().ActorNumber)
        {
            return;
        }
        actorIdToGamePlayerMap[actorNumber].RemoveCard((CardType)cardType);
    }
    
    [PunRPC]
    private void ShuffleDeckRPC(int randomSeed)
    {
        deckController.Shuffle(randomSeed);
    }
    
    [PunRPC]
    private void ForcePickCardsForCurrentTurnRPC(int count)
    {
        CurrentTurnPicksCards(count);
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
        
        EndTurnRPC();
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

    private void SetCurrentTurn(int value)
    {
        currentTurn = value;
        currentTurn = (currentTurn % PhotonNetworkController.GetPlayerCountInCurrentRoom()) + 1;
        gameCanvasController.UpdateTurn(actorIdToPhotonPlayerMap[currentTurn].NickName);
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
    
    private void LocalPlayerPicksACard()
    {
        SendCardPickedVisualToAll();
        SendEndTurnToAll();
    }

    private void CurrentTurnPicksCards(int count)
    {
        StartCoroutine(CurrentTurnPicksCardRoutine(count));
    }

    private IEnumerator CurrentTurnPicksCardRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CardType cardType = deckController.DrawCardTop();
            actorIdToGamePlayerMap[currentTurn].AddCard(cardType);

            yield return new WaitForSecondsRealtime(1f);
        }

        if (PhotonNetworkController.IsMasterClient())
        {
            SendEndTurnToAll();
        }
    }

    public void LocalPlayerPlaysCard(int actorNumber, CardType cardType, Guid cardLocalInstance, int targetActorNumber)
    {
        Logger.Log($"Normal Play: {actorIdToPhotonPlayerMap[actorNumber].NickName} plays {cardType}");
        CardAction cardAction = CardActionFactory.Get(cardType);
        if (cardAction == null)
        {
            Logger.Warning($"No handler for {cardType}");
            return;
        }
        
        // Remove Card Locally
        actorIdToGamePlayerMap[actorNumber].RemoveCard(cardLocalInstance);
        
        // Network Call
        SendCardPlayerVisualToAll(actorNumber, cardType);

        cardAction.Execute(actorNumber, targetActorNumber);
    }
    
    public void LocalPlayerPlaysCards(int actorNumber, List<(CardType, Guid)> playedCards)
    {
        Logger.Log($"Cat Combo Play: {actorIdToPhotonPlayerMap[actorNumber].NickName} plays " +
                   $"{playedCards[0].Item1} with {{playedCards.Count}} cards");
        
        // Cat Cards
    }

    #region CardActions Region
    
    public void ForceEndTurn()
    {
        SendEndTurnToAll();
    }

    public void ForcePlayerTakeTwoTurn(int targetActor)
    {
        SendSetTurnForAll(targetActor);
        SendForcePickCardsForCurrentTurn(2);
    }
    
    public void ShuffleCards(int randomShuffleSeed)
    {
        SendShuffleToAll(randomShuffleSeed);
    }

    #endregion
}
