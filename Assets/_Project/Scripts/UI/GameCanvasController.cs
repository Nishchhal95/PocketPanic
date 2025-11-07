using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvasController : MonoBehaviour
{
    public event Action CardDeckClicked;
    
    [SerializeField] private TMP_Text currentTurnNameIndicatorTextField;
    [SerializeField] private Button cardDeckButton;
    
    [SerializeField] private ShowTheFutureUIController showTheFutureUI;
    
    // 1 Player cannot Play alone and Max is 10
    [field: SerializeField] public PlayerCountToSlots[] PlayerCountToSlotsArray { get; private set; } = new PlayerCountToSlots[9];

    private void OnEnable()
    {
        cardDeckButton.onClick.AddListener(OnCardDeckClicked);
    }

    private void OnDisable()
    {
        cardDeckButton.onClick.RemoveListener(OnCardDeckClicked);
    }

    private void OnCardDeckClicked()
    {
        CardDeckClicked?.Invoke();
    }

    public void UpdateTurn(string playerName)
    {
        currentTurnNameIndicatorTextField.SetText(playerName);
        cardDeckButton.interactable = GameController.Instance.TurnManager.IsMyTurn;
    }

    public void ShowTheFutureUI(DeckController deckController, int count)
    {
        showTheFutureUI.Init(deckController, count, FutureViewMode.SeeFuture);
    }

    public void AlterTheFutureUI(DeckController deckController, int count, Action<List<CardType>> onConfirm = null)
    {
        showTheFutureUI.Init(deckController, count, FutureViewMode.AlterFuture, onConfirm);
    }
}
