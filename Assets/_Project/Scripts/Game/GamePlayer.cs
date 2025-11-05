using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameTextField;
    [SerializeField] private Button playCardButton;
    [SerializeField] private HandCardController handCardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Sprite backFace;
    [SerializeField] private int actorNumber;
    [SerializeField] private string userId;
    
    [SerializeField] private float cardSpacing = 160f;
    [SerializeField] private float moveDuration = 0.3f;
    [SerializeField] private float padding = 20f;

    private bool isLocal;
    private List<CardType> cards = new();
    private List<HandCardController> handCardControllers = new();

    private float containerWidth = -1f;
    private float cardWidth = -1f;
    
    private List<HandCardController> selectedCards = new();

    private void OnValidate()
    {
        UpdateHandLayout();
    }

    private void OnEnable()
    {
        if (playCardButton != null)
        {
            playCardButton.onClick.AddListener(OnPlayClicked);
        }
    }

    private void OnDisable()
    {
        if (playCardButton != null)
        {
            playCardButton.onClick.RemoveListener(OnPlayClicked);
        }
    }

    public void Init(string playerName, int actorNumber, string userId)
    {
        playerNameTextField.SetText(playerName);
        this.actorNumber = actorNumber;
        this.userId = userId;
    }

    public void InitCards(List<CardType> cards, bool isLocal)
    {
        this.cards = cards;
        this.isLocal = isLocal;
        if (playCardButton != null)
        {
            playCardButton.gameObject.SetActive(isLocal);
        }
        Logger.Log($"{playerNameTextField.text} has {string.Join(", ", cards)}");

        CreateHandCards();
        UpdatePlayButtonState();
    }

    private void OnPlayClicked()
    {
        if (selectedCards.Count == 0)
        {
            Logger.Error("Cannot play Empty Selection");
            return;
        }
        
        if (selectedCards.Count == 1)
        {
            GameController.Instance.LocalPlayerPlaysCard(actorNumber, selectedCards[0].CardType, 
                selectedCards[0].CardInstanceLocal, -1);
        }
        else
        {
            List<(CardType, Guid)> tupleList = selectedCards.Select(cardController => 
                (cardController.CardType, cardController.CardInstanceLocal)).ToList();
            GameController.Instance.LocalPlayerPlaysCards(actorNumber, tupleList);
        }
    }

    private void CreateHandCards()
    {
        foreach (var cardType in cards)
        {
            AddCardVisual(cardType);
        }
        
        UpdateHandLayout();
    }

    public bool HasCard(CardType cardType)
    {
        return cards.Contains(cardType);
    }

    public void AddCard(CardType cardType)
    {
        cards.Add(cardType);
        AddCardVisual(cardType);
        UpdateHandLayout(); 
    }
    
    private void AddCardVisual(CardType cardType)
    {
        CardData cardData = CardDatabase.Instance.Get(cardType);
        HandCardController handCardController = Instantiate(handCardPrefab, cardContainer);
        handCardController.Init(cardType, Guid.NewGuid(), cardData.artwork);
        handCardController.OnClick += OnCardClicked;
        handCardControllers.Add(handCardController);
    }

    private void OnCardClicked(Guid selectedCardInstanceGuid, CardType selectedCardType)
    {
        if (!GameController.Instance.IsMyTurn)
        {
            DeselectSelectedCards();
            return;
        }
        
        HandCardController clickedCard = 
            handCardControllers.Find(x => x.CardInstanceLocal == selectedCardInstanceGuid);

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
    
    private void SelectCard(HandCardController card)
    {
        selectedCards.Add(card);
        card.Select();
    }

    private void DeselectSelectedCards()
    {
        foreach (HandCardController card in selectedCards)
        {
            card.Deselect();
        }
    }

    private void ResetSelection(HandCardController newCard)
    {
        DeselectSelectedCards();
        selectedCards.Clear();
        SelectCard(newCard);
    }

    public CardType GetCard(int index)
    {
        return cards[index];
    }
    
    public void RemoveCard(int index)
    {
        if (index == -1)
        {
            return;
        }

        cards.RemoveAt(index);

        HandCardController handCardController = handCardControllers[index];
        handCardController.OnClick -= OnCardClicked;
        handCardControllers.RemoveAt(index);
        Destroy(handCardController.gameObject);

        UpdateHandLayout();
    }

    public void RemoveCard(Guid cardInstanceLocal)
    {
        int index = handCardControllers.FindIndex(x => x.CardInstanceLocal == cardInstanceLocal);
        if (index == -1)
        {
            return;
        }

        cards.RemoveAt(index);

        HandCardController handCardController = handCardControllers[index];
        handCardController.OnClick -= OnCardClicked;
        handCardControllers.RemoveAt(index);
        Destroy(handCardController.gameObject);

        UpdateHandLayout();
    }
    
    public void RemoveCard(CardType cardType)
    {
        int index = handCardControllers.FindIndex(x => x.CardType == cardType);
        if (index == -1)
        {
            return;
        }

        cards.RemoveAt(index);

        HandCardController handCardController = handCardControllers[index];
        handCardController.OnClick -= OnCardClicked;
        handCardControllers.RemoveAt(index);
        Destroy(handCardController.gameObject);

        UpdateHandLayout();
    }
    
    private void UpdateHandLayout()
    {
        int count = handCardControllers.Count;
        if (count == 0)
        {
            return;
        }

        float spacing = GetValidCardSpacing();
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < count; i++)
        {
            float xPos = startX + i * spacing;

            RectTransform rect = handCardControllers[i].GetComponent<RectTransform>();
            rect.DOAnchorPos(new Vector2(xPos, 0), moveDuration).SetEase(Ease.OutQuad);
            handCardControllers[i].transform.SetSiblingIndex(i);
        }
    }
    
    private float GetValidCardSpacing()
    {
        int cardCount = handCardControllers.Count;
        if (cardCount <= 1)
        {
            return cardSpacing;
        }

        if (Mathf.Approximately(cardWidth, -1f))
        {
            cardWidth = handCardPrefab.GetComponent<RectTransform>().rect.width;
        }
        
        if (Mathf.Approximately(containerWidth, -1f))
        {
            containerWidth = ((RectTransform)cardContainer).rect.width;
        }
        
        float maxSpacing = (containerWidth - cardWidth - padding)/ (cardCount - 1);
        return Mathf.Max(cardSpacing, maxSpacing);
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
}
