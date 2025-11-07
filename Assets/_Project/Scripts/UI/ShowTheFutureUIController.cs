using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTheFutureUIController : MonoBehaviour
{
    [SerializeField] private Transform container;
    [SerializeField] private FutureCardBehaviour futureCardPrefab;
    [SerializeField] private float spacing = 220f;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private List<FutureCardBehaviour> cardViews = new();
    private FutureViewMode mode;
    private System.Action<List<CardType>> onConfirm;

    public void Init(DeckController deckController, int count, FutureViewMode mode, System.Action<List<CardType>> onConfirm = null)
    {
        gameObject.SetActive(true);
        
        this.mode = mode;
        this.onConfirm = onConfirm;

        float startX = -((count - 1) * spacing) / 2f;

        for (int i = 0; i < count; i++)
        {
            CardType cardType = deckController.PeekCardAtIndexFromTop(i);
            CardData data = CardDatabase.Instance.Get(cardType);

            var card = Instantiate(futureCardPrefab, container);
            card.Init(data.artwork, cardType, mode == FutureViewMode.AlterFuture, this);
            card.SetTargetPosition(startX + spacing * i, 0);
            cardViews.Add(card);
        }

        confirmButton.gameObject.SetActive(mode == FutureViewMode.AlterFuture);
        cancelButton.gameObject.SetActive(true);

        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(Close);
    }

    public void UpdateCardOrder(FutureCardBehaviour draggedCard)
    {
        if (mode != FutureViewMode.AlterFuture)
        {
            return;
        }

        float dragX = draggedCard.RectTransform.anchoredPosition.x;
        int newIndex = Mathf.RoundToInt((dragX + ((cardViews.Count - 1) * spacing) / 2f) / spacing);
        newIndex = Mathf.Clamp(newIndex, 0, cardViews.Count - 1);

        int currentIndex = cardViews.IndexOf(draggedCard);
        if (newIndex != currentIndex)
        {
            cardViews.RemoveAt(currentIndex);
            cardViews.Insert(newIndex, draggedCard);
            UpdateLayout();
        }
    }

    public void UpdateLayout()
    {
        for (int i = 0; i < cardViews.Count; i++)
        {
            float x = -((cardViews.Count - 1) * spacing) / 2f + spacing * i;
            cardViews[i].SetTargetPosition(x, 0, animate: true);
        }
    }

    private void OnConfirm()
    {
        List<CardType> newOrder = new();
        foreach (var card in cardViews)
        {
            newOrder.Add(card.CardType);
        }

        onConfirm?.Invoke(newOrder);
        Close();
    }

    private void Close()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
        
        cardViews.Clear();
        gameObject.SetActive(false);
    }
}

public enum FutureViewMode
{
    SeeFuture,
    AlterFuture
}
