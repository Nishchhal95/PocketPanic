using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    public static event Action<int> PlayerSelected;
    public event Action<int, Guid, CardType> OnCardSelected;
    
    [SerializeField] private TMP_Text playerNameTextField;
    [SerializeField] private Button playCardButton;
    [SerializeField] private Button selectButton;
    [SerializeField] private CardController cardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Sprite backFace;
    [SerializeField] private int actorNumber;
    [SerializeField] private string userId;

    [SerializeField] private float localPlayerCardWidth;
    [SerializeField] private float localPlayerCardHeight;
    
    [SerializeField] private float remotePlayerCardWidth;
    [SerializeField] private float remotePlayerCardHeight;

    [SerializeField] private bool showCards;

    [SerializeField] private HandLayoutManager handLayoutManager;

    private bool initializedCards;
    private bool isLocal;
    private List<CardType> cards = new();
    private List<CardController> cardControllers = new();

    private List<CardController> selectedCards = new();

    private bool isSelectingTarget;
    private TaskCompletionSource<int> selectionTask;
    private TaskCompletionSource<(int, Guid, CardType)> waitingForCardTask;
    private bool canSelectCardToGiveAway = false;
    
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            return;    
        }
        
        handLayoutManager?.UpdateHandLayout();

        if (initializedCards && cardControllers is { Count: > 0 })
        {
            if (showCards)
            {
                foreach (CardController cardController in cardControllers)
                {
                    CardData cardData = CardDatabase.Instance.Get(cardController.CardType);
                    cardController.SetSprite(cardData.artwork);
                }
            }
            else
            {
                foreach (CardController cardController in cardControllers)
                {
                    cardController.SetSprite(backFace);
                }
            }
        }
    }

    private void OnEnable()
    {
        PlayerSelected += OnTargetActorSelected;
    }

    private void OnDisable()
    {
        if (playCardButton != null)
        {
            playCardButton.onClick.RemoveListener(OnPlayClicked);
        }
        
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnPlayerSelectedClicked);
        }
        
        PlayerSelected -= OnTargetActorSelected;
    }

    public void Init(string playerName, int actorNumber, string userId, bool isLocal)
    {
        playerNameTextField.SetText(playerName);
        this.actorNumber = actorNumber;
        this.userId = userId;
        this.isLocal = isLocal;

        if (this.isLocal)
        {
            LocalSetup();
        }
        else
        {
            RemoteSetup();
        }
    }

    private void LocalSetup()
    {
        playCardButton.gameObject.SetActive(true);
        playCardButton.interactable = false;
        
        playCardButton.onClick.AddListener(OnPlayClicked);
    }

    private void RemoteSetup()
    {
        selectButton.gameObject.SetActive(false);
        
        selectButton.onClick.AddListener(OnPlayerSelectedClicked);
    }

    public void InitCards(List<CardType> cards)
    {
        this.cards = cards;
        Logger.Log($"{playerNameTextField.text} has {string.Join(", ", cards)}");

        handLayoutManager = new HandLayoutManager(cardControllers,
            cardPrefab.GetComponent<RectTransform>().rect.width,
            ((RectTransform)cardContainer).rect.width, 
            isLocal ? 160f : 30f);
        
        CreateHandCards();
        UpdatePlayButtonState();

        initializedCards = true;
    }
    
    private void CreateHandCards()
    {
        foreach (var cardType in cards)
        {
            AddCardVisual(cardType);
        }
        
        handLayoutManager.UpdateHandLayout();
    }

    #region Card Functions

    public void AddCard(CardType cardType)
    {
        cards.Add(cardType);
        AddCardVisual(cardType);
        
        DeselectSelectedCards();
        handLayoutManager.UpdateHandLayout();
    }
    
    public CardType GetCard(int index)
    {
        return cards[index];
    }
    
    public bool HasCard(CardType cardType)
    {
        return cards.Contains(cardType);
    }
    
    public void RemoveCard(CardType cardType)
    {
        int index = cardControllers.FindIndex(x => x.CardType == cardType);
        if (index == -1)
        {
            return;
        }

        cards.RemoveAt(index);

        CardController cardController = cardControllers[index];
        cardController.OnClick -= OnCardClicked;
        cardControllers.RemoveAt(index);
        Destroy(cardController.gameObject);

        DeselectSelectedCards();
        handLayoutManager.UpdateHandLayout();
    }
    
    public void RemoveCard(Guid cardInstanceLocal)
    {
        int index = cardControllers.FindIndex(x => x.CardInstanceLocal == cardInstanceLocal);
        if (index == -1)
        {
            return;
        }

        cards.RemoveAt(index);

        CardController cardController = cardControllers[index];
        cardController.OnClick -= OnCardClicked;
        cardControllers.RemoveAt(index);
        Destroy(cardController.gameObject);

        DeselectSelectedCards();
        handLayoutManager.UpdateHandLayout();
    }
    
    public void RemoveCard(int index)
    {
        if (index == -1)
        {
            return;
        }

        cards.RemoveAt(index);

        CardController cardController = cardControllers[index];
        cardController.OnClick -= OnCardClicked;
        cardControllers.RemoveAt(index);
        Destroy(cardController.gameObject);

        DeselectSelectedCards();
        handLayoutManager.UpdateHandLayout();
    }

    #endregion
    
    private void AddCardVisual(CardType cardType)
    {
        CardData cardData = CardDatabase.Instance.Get(cardType);
        CardController cardController = Instantiate(cardPrefab, cardContainer);
        RectTransform cardRectTransform = cardController.GetComponent<RectTransform>();
        if (isLocal)
        {
            cardRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, localPlayerCardHeight);
            cardRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, localPlayerCardWidth);
        }
        else
        {
            cardRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, remotePlayerCardHeight);
            cardRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, remotePlayerCardWidth);
        }
        
        cardController.Init(cardType, Guid.NewGuid(), isLocal ? cardData.artwork : backFace, isLocal);
        cardController.OnClick += OnCardClicked;
        cardControllers.Add(cardController);
    }
    
    private void OnPlayerSelectedClicked()
    {
        PlayerSelected?.Invoke(actorNumber);
    }

    private void OnTargetActorSelected(int targetActor)
    {
        if (!isSelectingTarget)
        {
            return;
        }

        GameController.Instance.HideTargetSelection();
        isSelectingTarget = false;
        
        selectionTask.SetResult(actorNumber);
    }

    private void OnPlayClicked()
    {
        if (selectedCards.Count == 0)
        {
            Logger.Error("Cannot play Empty Selection");
            return;
        }

        _ = HandleCardPlayed();
    }

    private async Task HandleCardPlayed()
    {
        int targetActorNumber = -1;
        if (selectedCards.Count == 1)
        {
            CardType playedCardType = selectedCards[0].CardType;

            if (Utilites.DoesCardNeedToSelectTarget(playedCardType))
            {
                targetActorNumber = await SelectTargetActorAsync();
            }

            GameController.Instance.LocalPlayerPlaysCard(actorNumber, playedCardType, 
                selectedCards[0].CardInstanceLocal, targetActorNumber);
        }
        else
        {
            targetActorNumber = await SelectTargetActorAsync();
            
            List<(CardType, Guid)> tupleList = selectedCards.Select(cardController => 
                (cardController.CardType, cardController.CardInstanceLocal)).ToList();
            GameController.Instance.LocalPlayerPlaysCards(actorNumber, tupleList, targetActorNumber);
        }
        
        selectedCards.Clear();
    }

    private Task<int> SelectTargetActorAsync()
    {
        if (isSelectingTarget)
        {
            Logger.Warning("Target selecting in Progress");
            return selectionTask?.Task ?? Task.FromResult(-1);
        }

        isSelectingTarget = true;
        selectionTask = new TaskCompletionSource<int>();
        GameController.Instance.ShowTargetSelection();

        return selectionTask.Task;
    }

    private void OnCardClicked(Guid selectedCardInstanceGuid, CardType selectedCardType)
    {
        if (canSelectCardToGiveAway)
        {
            OnCardSelected?.Invoke(actorNumber, selectedCardInstanceGuid, selectedCardType);
            return;
        }
        
        if (!GameController.Instance.TurnManager.IsMyTurn)
        {
            DeselectSelectedCards();
            return;
        }
        
        CardController clickedCard = 
            cardControllers.Find(x => x.CardInstanceLocal == selectedCardInstanceGuid);

        // --- CASE 1: Already selected → Deselect it
        if (selectedCards.Contains(clickedCard))
        {
            clickedCard.Deselect();
            selectedCards.Remove(clickedCard);
            UpdatePlayButtonState();
            return;
        }

        // --- CASE 2: No cards selected yet → select the first one
        if (selectedCards.Count == 0)
        {
            SelectCard(clickedCard);
            UpdatePlayButtonState();
            return;
        }

        // --- CASE 3: Multiple selection logic (Cat + WildCat only)
        bool isClickedCat = Utilites.IsCatCard(clickedCard.CardType);
        bool isSelectedCats = selectedCards.All(c => Utilites.IsCatCard(c.CardType));

        // Only Cat cards can be multi-selected
        if (!isClickedCat || !isSelectedCats)
        {
            ResetSelection(clickedCard);
            UpdatePlayButtonState();
            return;
        }

        // Check if this card type is compatible with current selection
        bool isCompatible = selectedCards.All(c =>
            c.CardType == clickedCard.CardType ||
            clickedCard.CardType == CardType.WildCat ||
            c.CardType == CardType.WildCat);

        // If invalid combination or limit exceeded → reset
        if (!isCompatible || selectedCards.Count >= 3)
        {
            ResetSelection(clickedCard);
            UpdatePlayButtonState();
            return;
        }

        // --- CASE 4: Add new matching card
        SelectCard(clickedCard);

        // TODO: Feedback
        if (selectedCards.Count == 2)
        {
            Logger.Log("Double Cat Combo!");
        }
        else if (selectedCards.Count == 3)
        {
            Logger.Log("Triple Cat Combo!");
        }

        UpdatePlayButtonState();
    }
    
    private void SelectCard(CardController card)
    {
        selectedCards.Add(card);
        card.Select();
    }

    private void DeselectSelectedCards()
    {
        foreach (CardController card in selectedCards)
        {
            card.Deselect();
        }
    }

    private void ResetSelection(CardController newCard)
    {
        DeselectSelectedCards();
        selectedCards.Clear();
        SelectCard(newCard);
    }
    
    private void UpdatePlayButtonState()
    {
        if (!isLocal)
        {
            return;
        }
        
        if (selectedCards.Count <= 0)
        {
            playCardButton.interactable = false;
            return;
        }
        
        playCardButton.interactable = true;
    }

    public void ShowSelectionUI()
    {
        selectButton.gameObject.SetActive(true);
    }
    
    public void HideSelectionUI()
    {
        selectButton.gameObject.SetActive(false);
    }
    
    public async Task<(int, Guid, CardType)> RequestCardToGiveAsync()
    {
        Logger.Log($"{playerNameTextField.text} is asked to give a Favor card.");
    
        canSelectCardToGiveAway = true;
        waitingForCardTask = new TaskCompletionSource<(int, Guid, CardType)>();

        OnCardSelected += OnCardChosen;

        (int, Guid, CardType) tuple = await waitingForCardTask.Task;

        Logger.Log($"{playerNameTextField.text} selected {tuple.Item3} as favor card");
        return tuple;
    }
    
    private void OnCardChosen(int actorNum, Guid cardGuid, CardType cardType)
    {
        OnCardSelected -= OnCardChosen;
        canSelectCardToGiveAway = false;
        waitingForCardTask.TrySetResult((actorNum, cardGuid, cardType));
    }
}
