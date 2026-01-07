using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviourPun
{
    public static GameController Instance { get; private set; }

    [SerializeField] private DeckController deckController;
    [SerializeField] private GameCanvasController gameCanvasController;
    [SerializeField] private GamePlayer localGamePlayerPrefab;
    [SerializeField] private GamePlayer remoteGamePlayerPrefab;
    [SerializeField] private CardController playerCardPrefab;
    [SerializeField] private Sprite backCardFace;

    [SerializeField] private float localPlayerCardWidth = 200f;
    [SerializeField] private float localPlayerCardHeight = 300f;
    
    [SerializeField] private float remotePlayerCardWidth = 100f;
    [SerializeField] private float remotePlayerCardHeight = 130f;

    // This is an ordered list by ActorNumber
    private Dictionary<int, Player> actorIdToPhotonPlayerMap = new();
    private Dictionary<int, GamePlayer> actorIdToGamePlayerMap = new();
    private List<int> aliveActors;

    private int GetLocalActorNumber() => PhotonNetworkController.GetLocalPlayer().ActorNumber;
    public TurnManager TurnManager { get; private set; }
    public bool SetupCompleted { get; private set; } = false;
    [field: SerializeField] public NetworkManager NetworkManager { get; private set; }

    private GamePlayer testPlayer;
    [SerializeField] private CardType testAddCardType;
    [SerializeField] private CardType testRemoveCardType;

    private bool canSelectCard;
    
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
        gameCanvasController.CardDeckClicked += OnDrawCardClicked;

        NetworkManager.OnGameStarted += OnGameStarted;
        NetworkManager.OnCardDrawn += OnCardDrawn;
        NetworkManager.OnEndTurn += OnEndTurn;
        NetworkManager.OnSetActorTurn += OnSetActorTurn;
        NetworkManager.OnDefuseUsed += OnDefuseUsed;
        NetworkManager.OnPlayerExploded += OnPlayerExploded;
        NetworkManager.OnActionCardPlayed += OnActionCardPlayed;
        NetworkManager.OnShuffleDeck += OnShuffleDeck;
        NetworkManager.OnRequestFavorCard += OnRequestFavorCard;
        NetworkManager.OnResponseFavorCard += OnResponseFavorCard;
        NetworkManager.OnAlterTheFutureCardsChanged += OnAlterTheFutureCardsChanged;
        NetworkManager.OnStealCardWith2CardCombo += OnStealCardWith2CardCombo;
    }

    private void OnDisable()
    {
        gameCanvasController.CardDeckClicked -= OnDrawCardClicked;
        
        NetworkManager.OnGameStarted -= OnGameStarted;
        NetworkManager.OnCardDrawn -= OnCardDrawn;
        NetworkManager.OnEndTurn -= OnEndTurn;
        NetworkManager.OnSetActorTurn -= OnSetActorTurn;
        NetworkManager.OnDefuseUsed -= OnDefuseUsed;
        NetworkManager.OnPlayerExploded -= OnPlayerExploded;
        NetworkManager.OnActionCardPlayed -= OnActionCardPlayed;
        NetworkManager.OnShuffleDeck -= OnShuffleDeck;
        NetworkManager.OnRequestFavorCard -= OnRequestFavorCard;
        NetworkManager.OnResponseFavorCard -= OnResponseFavorCard;
        NetworkManager.OnAlterTheFutureCardsChanged -= OnAlterTheFutureCardsChanged;
        NetworkManager.OnStealCardWith2CardCombo -= OnStealCardWith2CardCombo;

        TurnManager?.Dispose();
    }

    private void Start()
    {
        //TestCardsSpreadNoNetwork();
    }

    private void InitializeManagers()
    {
        aliveActors = actorIdToPhotonPlayerMap.Values.ToList().ConvertAll(x => x.ActorNumber);
        
        TurnManager = new TurnManager(PhotonNetworkController.GetPlayersCurrentRoom(), actorIdToGamePlayerMap, 
            gameCanvasController, () => NetworkManager.SendEndTurn());
    }

    private void TestCardsSpreadNoNetwork()
    {
        int numberOfCards = 10;
        List<Transform> playerSlots = GetPlayerSlotsFromPlayerCount(2);
        testPlayer = Instantiate(localGamePlayerPrefab, playerSlots[0]);
        testPlayer.Init("Demo", 1, "DemoUserId", true);

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

        foreach (CardType card in cards)
        {
            testPlayer.AddCard(card);
        }
    }

    public void TestAdd()
    {
        testPlayer.AddCard(testAddCardType);
    }

    public void TestRemove()
    {
        testPlayer.RemoveCard(testRemoveCardType);
    }

    public void StartGameNetworked()
    {
        if (!PhotonNetworkController.IsMasterClient())
        {
            Logger.Error("Only Master Clients can Start the Game");
            return;
        }
        
        // Generate and Shuffle Deck for all players
        int randomSeedInitial = UnityEngine.Random.Range(1, 99999);
        int randomSeedFinal = UnityEngine.Random.Range(1, 99999);
        
        NetworkManager.SendStartGame(randomSeedInitial, randomSeedFinal);
    }

    #region RPC Handlers

    private void OnGameStarted(int seed1, int seed2)
    {
        GameEvents.RaiseGameStarted();
        _ = SetupGame(seed1, seed2);
    }

    private void OnCardDrawn(int actorNumber, int count, bool top)
    {
        if (deckController.IsDeckEmpty())
        {
            Logger.Error("Draw Deck is Empty!");
            return;
        }

        _ = OnCardDrawAsync(actorNumber, count, top);
    }
    
    private async Task OnCardDrawAsync(int actorNumber, int count, bool top)
    {
        // Instantly pause the Turn timer because we draw a card.
        TurnManager.PauseTimer();

        CardType[] cardsDrawn = await HandleCardDrawUIAsync(actorNumber, count, top);

        if (!cardsDrawn.Contains(CardType.Explode))
        {
            // Only the Players whose turn it is can End Turn
            if (actorNumber == GetLocalActorNumber())
            {
                NetworkManager.SendEndTurn();
            }
            return;
        }
        
        for (int i = 0; i < cardsDrawn.Length; i++)
        {
            CardType drawnCard = cardsDrawn[i];
            if (drawnCard == CardType.Explode)
            {
                Logger.Log($"{actorIdToPhotonPlayerMap[actorNumber].NickName} draws Explode!");
                await HandleExplodingDrawnAsync(actorNumber);
            }
        }
    }

    private async Task<CardType[]> HandleCardDrawUIAsync(int actorNumber, int count, bool top)
    { 
        /* So why do I do this (Why do I draw cards first and loop again to check if I draw an Explode?)
        This is because there could be a case when I draw 2 cards,
        1 could be Explode and the 2nd could be a Defuse,
        so to handle that case I think it is always better to draw all the required cards and then iterate over them
        to check if I did Explode or not? */
        CardType[] cardsDrawn = new CardType[count];
        for (int i = 0; i < count; i++)
        {
            GamePlayer gamePlayer = actorIdToGamePlayerMap[actorNumber];
            Player photonPlayer = actorIdToPhotonPlayerMap[actorNumber];
            bool isLocal = photonPlayer.IsLocal;

            CardType drawnCard = top ? deckController.DrawCardTop() : deckController.DrawCardBottom();

            CardController cardController = await CreateAndDealCardToPlayer(drawnCard, gamePlayer, isLocal);
            gamePlayer.AddCard(cardController);
            cardsDrawn[i] = drawnCard;

            if (count > 1)
            {
                await Task.Delay(TimeSpan.FromSeconds(1f));
            }
        }

        return cardsDrawn;
    }

    private async Task HandleExplodingDrawnAsync(int actorNumber)
    {
        GamePlayer gamePlayer = actorIdToGamePlayerMap[actorNumber];
        bool hasDefuse = gamePlayer.HasCard(CardType.Defuse);

        await Task.Delay(TimeSpan.FromSeconds(1f));

        if (hasDefuse)
        {
            Logger.Log("Placing Explode back in the Deck");
            int explodeIndex = 1;
            if (actorNumber == GetLocalActorNumber())
            {
                // Show some UI to put back Explode only to the player who got explode
                gameCanvasController.ShowPlaceExplodeBackUI(deckController.GetCount(), 
                    GameConfig.PUTTING_EXPLODE_BACK_TO_DECK_DURATION,
                    index =>
                    {
                        NetworkManager.SendDefuseUsed(actorNumber, index);
                        NetworkManager.SendEndTurn();
                    });
            }
            else
            {
                // Maybe show other users some UI
            }
        }
        else
        {
            if (actorNumber == GetLocalActorNumber())
            {
                NetworkManager.SendPlayerExploded(actorNumber);
            }
        }
    }

    private void OnEndTurn()
    {
        TurnManager.EndTurn();
    }

    private void OnSetActorTurn(int actorTurn)
    {
        TurnManager.SetActorsTurn(actorTurn);
    }

    private void OnDefuseUsed(int actorNumber, int newExplodeIndex)
    {
        GamePlayer gamePlayer = actorIdToGamePlayerMap[actorNumber];
        _ = OnDefuseUsedAsync(gamePlayer, newExplodeIndex);
    }

    private async Task OnDefuseUsedAsync(GamePlayer gamePlayer, int newExplodeIndex)
    {
        // Play Defuse (Move it to Discard Deck)
        CardController defuseCardController = gamePlayer.GetCardController(CardType.Defuse);
        defuseCardController.FaceUp();
        defuseCardController.SetCardSizeFull();
        await MoveCardToDiscardDeck(defuseCardController);
        gamePlayer.RemoveCard(CardType.Defuse);
        
        // Play Explode (Move it to Draw Deck)
        CardController explodeCardController = gamePlayer.GetCardController(CardType.Explode);
        explodeCardController.FaceUp();
        explodeCardController.SetCardSizeFull();
        await MoveCardToDrawDeck(explodeCardController);
        gamePlayer.RemoveCard(CardType.Explode);
        Destroy(explodeCardController.gameObject);
                
        deckController.InsertCardAtIndex(CardType.Explode, newExplodeIndex);
    }

    private void OnPlayerExploded(int actorNumber)
    {
        Logger.Log($"{TurnManager.GetPhotonPlayerForCurrentTurn().NickName} Exploded");
        MarkPlayerDead(actorNumber);
    }

    private void OnActionCardPlayed(int actorNumber, int cardTypeInt)
    {
        TurnManager.RestartTimer();
        
        // Locally we remove card by Instance GUID already
        if (actorNumber == PhotonNetworkController.GetLocalPlayer().ActorNumber)
        {
            return;
        }
        
        _ = OnActionCardPlayedForRemotePlayer(actorNumber, cardTypeInt);
    }

    private async Task OnActionCardPlayedForRemotePlayer(int actorNumber, int cardTypeInt)
    {
        GamePlayer gamePlayer = actorIdToGamePlayerMap[actorNumber];
        CardType cardType = (CardType)cardTypeInt;
        
        CardController playedCardController = gamePlayer.GetCardController(cardType);
        playedCardController.FaceUp();
        playedCardController.SetCardSizeFull();
        await MoveCardToDiscardDeck(playedCardController);
        gamePlayer.RemoveCard(cardType);
        
        deckController.PlayCard(cardType);
        
        // Maybe Start a Timer for a Nope Card??
    }

    private void OnShuffleDeck(int shuffleSeed)
    {
        deckController.Shuffle(shuffleSeed);
    }

    private void OnRequestFavorCard(int from, int to)
    {
        Logger.Log($"{actorIdToPhotonPlayerMap[from].NickName} " +
                   $"requested a Favor from {actorIdToPhotonPlayerMap[to].NickName}");
        
        if (PhotonNetworkController.GetLocalPlayer().ActorNumber != to)
        {
            return;
        }
        
        RequestingFavorCardAsync(from, to);
    }

    private async void RequestingFavorCardAsync(int from, int to)
    {
        Logger.Log("Showing Favor Request UI...");
        GamePlayer targetPlayer = actorIdToGamePlayerMap[to];

        (int, Guid, CardType) result = await targetPlayer.RequestCardToGiveAsync();
        GamePlayer fromGamePlayer = actorIdToGamePlayerMap[from];
        GamePlayer toGamePlayer = actorIdToGamePlayerMap[to];
        
        fromGamePlayer.RemoveCard(result.Item2);
        toGamePlayer.AddCard(result.Item3);
        
        NetworkManager.SendResponseFavorCard(to, from, (int)result.Item3);
    }

    private void OnResponseFavorCard(int from, int to, int cardTypeInt)
    {
        // We removed card locally for the player requesting favor from
        if (PhotonNetworkController.GetLocalPlayer().ActorNumber == from)
        {
            return;
        }
        
        CardType cardType = (CardType)cardTypeInt;
        
        Logger.Log($"{actorIdToPhotonPlayerMap[from].NickName} " +
                   $"responded with a {cardType.ToString()} card to {actorIdToPhotonPlayerMap[to].NickName}");

        GamePlayer fromGamePlayer = actorIdToGamePlayerMap[from];
        GamePlayer toGamePlayer = actorIdToGamePlayerMap[to];
        
        fromGamePlayer.RemoveCard(cardType);
        toGamePlayer.AddCard(cardType);
    }

    private void OnAlterTheFutureCardsChanged(int actorNumber, int[] cards)
    {
        Logger.Log($"{actorIdToPhotonPlayerMap[actorNumber].NickName} altered cards and top 3 cards are {string.Join(", ", cards)}");
        
        for (int i = 0; i < cards.Length; i++)
        {
            deckController.DrawCardTop();
        }

        for (int i = 0; i < cards.Length; i++)
        {
            deckController.InsertCardAtIndex((CardType)cards[i], i);
        }
    }

    private void OnStealCardWith2CardCombo(int from, int to, int cardType)
    {
        if (PhotonNetworkController.GetLocalPlayer().ActorNumber == to)
        {
            return;
        }
        _ = OnStealCardWith2CardComboAsync(from, to, cardType);
    }

    private async Task OnStealCardWith2CardComboAsync(int from, int to, int cardType)
    {
        Logger.Log($"OnStealCardWith2CardComboAsync {(CardType)cardType} " +
                   $"from {actorIdToPhotonPlayerMap[from].NickName} to {actorIdToPhotonPlayerMap[to].NickName}");
        GamePlayer fromGamePlayer = actorIdToGamePlayerMap[from];
        GamePlayer toGamePlayer = actorIdToGamePlayerMap[to];

        CardController addedCardController = await CreateAndDealCardFromPlayerToPlayer((CardType)cardType, 
            fromGamePlayer, toGamePlayer, false);
        fromGamePlayer.RemoveCard((CardType)cardType);
        toGamePlayer.AddCard(addedCardController);
    }

    #endregion
    
    private async Task SetupGame(int seedInitial, int seedFinal)
    {
        deckController.BuildDeckWithoutExplodeAndDiffuse();
        deckController.Shuffle(seedInitial);
        
        SpawnPlayers();
        InitializeManagers();
        await DealCardsToPlayers();
        
        deckController.AddExplode(PhotonNetworkController.GetPlayerCountInCurrentRoom());
        deckController.AddDefuse(2);
        deckController.Shuffle(seedFinal);
        
        deckController.InsertCardAtIndex(CardType.Explode);
        
        TurnManager.EndTurn();
        SetupCompleted = true;
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
            gamePlayer.Init(player.NickName, player.ActorNumber, player.UserId, player.IsLocal);
            gamePlayer.OnCardSelected += OnCardSelected;
            
            actorIdToPhotonPlayerMap.Add(player.ActorNumber, player);
            actorIdToGamePlayerMap.Add(player.ActorNumber, gamePlayer);
        }
    }

    private async Task DealCardsToPlayers()
    {
        Player[] players = PhotonNetworkController.GetPlayersCurrentRoom();

        foreach (Player player in players)
        {
            GamePlayer currentPlayer = actorIdToGamePlayerMap[player.ActorNumber];
            Player photonPlayer = actorIdToPhotonPlayerMap[player.ActorNumber];
            
            // Adding first as Defuse and then the rest of the cards
            for (int j = 0; j < 8; j++)
            {
                CardType currentCard = j == 0 ? CardType.Defuse : deckController.DrawCardTop();

                CardController cardController = await CreateAndDealCardToPlayer(currentCard, currentPlayer, 
                    photonPlayer.IsLocal);
                currentPlayer.AddCard(cardController);
            }
        }
    }

    private async Task<CardController> CreateAndDealCardToPlayer(CardType cardType, GamePlayer gamePlayer, bool isLocal)
    {
        CardController currentCardController = CreateCardController(cardType,
            gamePlayer.CardContainer, gameCanvasController.DrawDeckTransform, isLocal);
        await CardAnimationController.AnimateCardToAnchored(currentCardController, 
            gamePlayer.HandLayoutManager.GetNextCardPosition(), 
            GameConfig.ANIMATION_DEAL_CARD_DURATION);
        return currentCardController;
    }
    
    private async Task<CardController> CreateAndDealCardFromPlayerToPlayer(CardType cardType, GamePlayer from, GamePlayer to, bool isLocal)
    {
        CardController currentCardController = CreateCardController(cardType,
            to.CardContainer, from.CardContainer, isLocal);
        await CardAnimationController.AnimateCardToAnchored(currentCardController, 
            to.HandLayoutManager.GetNextCardPosition(), 
            GameConfig.ANIMATION_DEAL_CARD_DURATION);
        return currentCardController;
    }

    private CardController CreateCardController(CardType cardType, Transform parent, Transform deckTransform, bool isLocal)
    {
        CardData cardData = CardDatabase.Instance.Get(cardType);
        CardController cardController = Instantiate(playerCardPrefab, deckTransform);
        RectTransform cardRectTransform = cardController.RectTransform;
        cardRectTransform.SetParent(parent, true);
        cardRectTransform.name = $"{cardType.ToString()}";
        cardController.Init(new CardRuntimeData(cardType, Guid.NewGuid(), cardData.artwork, backCardFace, 
            isLocal, localPlayerCardWidth, localPlayerCardHeight, remotePlayerCardWidth, remotePlayerCardHeight));
        
        if (isLocal)
        {
            cardController.SetCardSizeFull();
        }
        else
        {
            cardController.SetCardSizeSmall();
        }
        
        return cardController;
    }

    private void OnCardSelected(int actorNumber, Guid cardInstanceId, CardType cardType)
    {
        
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
    
    // Draw Card
    private void OnDrawCardClicked()
    {
        if (!TurnManager.IsMyTurn)
        {
            return;
        }
        
        NetworkManager.SendDrawCard(TurnManager.GetPhotonPlayerForCurrentTurn().ActorNumber, 1, true);
    }
    
    // Play A Card
    public async Task LocalPlayerPlaysCard(int actorNumber, CardType cardType, Guid cardLocalInstance, int targetActorNumber)
    {
        GamePlayer currentGamePlayer = actorIdToGamePlayerMap[actorNumber];
        Player currentPhotonPlayer = actorIdToPhotonPlayerMap[actorNumber];
        
        Logger.Log($"Normal Play: {currentPhotonPlayer.NickName} plays {cardType}");
        
        CardAction cardAction = CardActionFactory.Get(cardType);
        if (cardAction == null)
        {
            Logger.Warning($"No handler for {cardType}");
            return;
        }
        
        // Network Call To Send Played CardAction
        NetworkManager.SendPlayActionCard(actorNumber, (int)cardType);
        
        // Animate Card from Player to Discard Deck
        await MoveCardToDiscardDeck(currentGamePlayer.GetCardController(cardLocalInstance));
        
        // Remove Card Locally
        currentGamePlayer.RemoveCard(cardLocalInstance);
        
        // If the card needs to select a Target
        if (Utilites.DoesCardNeedToSelectTarget(cardType))
        {
            targetActorNumber = await currentGamePlayer.SelectTargetActorAsync();
        }

        // Execute the card and perform a Network call if needed based on the Card Played
        cardAction.Execute(actorNumber, targetActorNumber);
    }
    
    public async Task LocalPlayerPlaysCards(int actorNumber, List<(CardType, Guid)> playedCards, int targetActorNumber)
    {
        Logger.Log($"Cat Combo Play: {actorIdToPhotonPlayerMap[actorNumber].NickName} plays " +
                   $"{playedCards[0].Item1} with {playedCards.Count} cards");
        
        GamePlayer currentGamePlayer = actorIdToGamePlayerMap[actorNumber];
        Player currentPhotonPlayer = actorIdToPhotonPlayerMap[actorNumber];
        
        Logger.Log($"Waiting for Selecting Target for {currentPhotonPlayer.NickName}");
        targetActorNumber = await currentGamePlayer.SelectTargetActorAsync();
        
        GamePlayer targetGamePlayer = actorIdToGamePlayerMap[targetActorNumber];
        Player targetPhotonPlayer = actorIdToPhotonPlayerMap[targetActorNumber];
        
        Logger.Log($"Target Selected is {targetActorNumber} and Name is {targetPhotonPlayer.NickName}");

        if (playedCards.Count == 2)
        {
            // Can Pick a Card from the User
            int randomCardIndex = Random.Range(0, targetGamePlayer.GetCardCount() - 1);
            CardType cardType = targetGamePlayer.GetCard(randomCardIndex);
            
            foreach (var playedCard in playedCards)
            {
                CardController cardController = currentGamePlayer.GetCardController(playedCard.Item2);
                await MoveCardToDiscardDeck(cardController);
                currentGamePlayer.RemoveCard(playedCard.Item2);
            }
            
            Logger.Log($"Stealing {cardType} card from {targetPhotonPlayer.NickName} to {currentPhotonPlayer.NickName}");
            CardController addedCardController = await CreateAndDealCardFromPlayerToPlayer(cardType, 
                targetGamePlayer, currentGamePlayer, true);
            targetGamePlayer.RemoveCard(cardType);
            currentGamePlayer.AddCard(addedCardController);
            
            NetworkManager.SendStealCardWith2CardCombo(targetActorNumber, actorNumber, (int)cardType);
        }
        else if (playedCards.Count == 3)
        {
            // Can Request any Card from the User.
        }
    }

    #region CardActions Region
    
    public void ForcePlayerTakeTwoTurn(int targetActor)
    {
        // Wraps it so it starts from 1 till player count, NOT FROM 0
        targetActor = (targetActor - 1) % actorIdToGamePlayerMap.Count + 1;
        NetworkManager.SendSetActorTurn(targetActor);
        NetworkManager.SendDrawCard(targetActor, 2, true);
    }
    
    public void ForceEndTurn()
    {
        NetworkManager.SendEndTurn();
    }
    
    public void ShuffleCards(int shuffleSeed)
    {
        NetworkManager.SendShuffleDeck(shuffleSeed);
    }
    
    public void ShowPlayerTopCards(int cardCount)
    {
        for (int i = 0; i < cardCount; i++)
        {
            Logger.Log($"Player sees Card {deckController.PeekCardAtIndexFromTop(i)} at Pos {i + 1}");
        }

        gameCanvasController.ShowTheFutureUI(deckController, cardCount);
    }
    
    public void ShowAndAlterTopCard(int actorNumber, int cardCount)
    {
        for (int i = 0; i < cardCount; i++)
        {
            Logger.Log($"Player sees Card {deckController.PeekCardAtIndexFromTop(i)} at Pos {i + 1}");
        }

        gameCanvasController.AlterTheFutureUI(deckController, cardCount, cards =>
        {
            OnAlterTheFutureConfirmed(actorNumber, cards);
        });
    }
    
    public void RequestCardFromPlayer(int requestingActorNumber, int targetActorNumber, int cardIndex)
    {
        // Wraps it so it starts from 1 till player count, NOT FROM 0
        targetActorNumber = (targetActorNumber - 1) % actorIdToGamePlayerMap.Count + 1;
        NetworkManager.SendRequestFavorCard(requestingActorNumber, targetActorNumber);
    }
    
    public void DrawFromBottomAndEndTurn(int actorNumber)
    {
        NetworkManager.SendDrawCard(actorNumber, 1, false);
    }

    #endregion

    private void MarkPlayerDead(int deadActorNumber)
    {
        if (!aliveActors.Contains(deadActorNumber))
        {
            Logger.Error($"Player {actorIdToPhotonPlayerMap[deadActorNumber].NickName} was already dead!");
            return;
        }
        
        aliveActors.Remove(deadActorNumber);
        TurnManager.MarkDead(deadActorNumber);

        if (aliveActors.Count == 1)
        {
            GameOver();
        }
    }
    
    private void GameOver()
    {
        if (aliveActors.Count != 1)
        {
            Logger.Error($"Game Over failed because {aliveActors.Count} actors are alive!");
            return;
        }
        
        Logger.Log($"Winning Player : {actorIdToPhotonPlayerMap[aliveActors[0]].NickName}");
    }

    private void OnAlterTheFutureConfirmed(int actorNumber, List<CardType> cards)
    {
        bool futureAltered = false;
        
        for (int i = 0; i < cards.Count; i++)
        {
            if (deckController.PeekCardAtIndexFromTop(i) != cards[i])
            {
                futureAltered = true;
                break;
            }
        }

        if (futureAltered)
        {
            int[] cardIntArray = new int[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                cardIntArray[i] = (int)cards[i];
            }
            
            NetworkManager.SendAlterTheFutureCardsChanged(actorNumber, cardIntArray);
        }
    }

    public void ShowTargetSelection()
    {
        foreach (Player player in actorIdToPhotonPlayerMap.Values)
        {
            if (player.IsLocal)
            {
                continue;
            }

            GamePlayer gamePlayer = actorIdToGamePlayerMap[player.ActorNumber];
            gamePlayer.ShowSelectionUI();
        }
    }

    public void HideTargetSelection()
    {
        foreach (Player player in actorIdToPhotonPlayerMap.Values)
        {
            if (player.IsLocal)
            {
                continue;
            }

            GamePlayer gamePlayer = actorIdToGamePlayerMap[player.ActorNumber];
            gamePlayer.HideSelectionUI();
        }
    }

    private async Task MoveCardToDiscardDeck(CardController cardController)
    {
        await CardAnimationController.AnimateCardToPositionWithRandomRotation(cardController,
            gameCanvasController.DiscardDeckTransform.position, GameConfig.ANIMATION_PLAY_CARD_DURATION);
        
        cardController.RectTransform.SetParent(gameCanvasController.DiscardDeckTransform);
        cardController.RectTransform.localPosition = Vector3.zero;
    }
    
    private async Task MoveCardToDrawDeck(CardController cardController)
    {
        await CardAnimationController.AnimateCardToPosition(cardController,
            gameCanvasController.DrawDeckTransform.position, GameConfig.ANIMATION_PLAY_CARD_DURATION);
        
        cardController.RectTransform.SetParent(gameCanvasController.DrawDeckTransform);
        cardController.RectTransform.localPosition = Vector3.zero;
    }
}
