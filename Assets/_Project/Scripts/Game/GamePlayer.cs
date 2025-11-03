using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class GamePlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameTextField;
    [SerializeField] private HandCardController handCardPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private Sprite backFace;
    [SerializeField] private int actorNumber;
    [SerializeField] private string userId;
    
    [SerializeField] private float cardSpacing = 160f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float padding = 20f;

    private bool isLocal;
    private List<CardType> cards = new();
    private List<HandCardController> handCardControllers = new();

    private float containerWidth = -1f;
    private float cardWidth = -1f;

    private void OnValidate()
    {
        UpdateHandLayout();
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
        Logger.Log($"{playerNameTextField.text} has {string.Join(", ", cards)}");

        CreateHandCards();
    }

    private void CreateHandCards()
    {
        foreach (var cardType in cards)
        {
            AddCardVisual(cardType);
        }
        
        UpdateHandLayout();
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
        handCardController.Init(cardType, cardData.artwork);
        handCardController.OnClick += RemoveCard;
        handCardController.FAKEADD += FakeAdd;
        handCardControllers.Add(handCardController);
    }

    private void FakeAdd()
    {
        Array values = Enum.GetValues(typeof(CardType));
        CardType random = (CardType)values.GetValue(Random.Range(0, values.Length));
        AddCard(random);
    }

    public void RemoveCard(CardType cardType)
    {
        int index = cards.IndexOf(cardType);
        if (index == -1)
        {
            return;
        }

        cards.RemoveAt(index);

        HandCardController handCardController = handCardControllers[index];
        handCardController.OnClick -= RemoveCard;
        handCardController.FAKEADD -= FakeAdd;
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
            StartCoroutine(MoveCard(rect, new Vector2(xPos, 0)));
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
    
    private IEnumerator MoveCard(RectTransform rect, Vector2 targetPos)
    {
        Vector2 startPos = rect.anchoredPosition;

        float timeElapsed = 0f;
        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * moveSpeed;
            rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, timeElapsed);
            yield return null;
        }

        rect.anchoredPosition = targetPos;
    }
}
