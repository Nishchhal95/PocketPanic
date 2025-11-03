using System.Collections.Generic;
using UnityEngine;

public class CardDatabase : MonoBehaviour
{
    public static CardDatabase Instance { get; private set; }

    private readonly Dictionary<CardType, CardData> cards = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllCards();
    }

    private void LoadAllCards()
    {
        var allCards = Resources.LoadAll<CardData>("Cards");

        foreach (var card in allCards)
        {
            cards.TryAdd(card.type, card);
        }

        Logger.Log($"Loaded {cards.Count} card types into CardDatabase.");
    }

    public CardData Get(CardType type)
    {
        if (cards.TryGetValue(type, out var data))
        {
            return data;
        }

        Logger.Warning($"Card type {type} not found in CardDatabase!");
        return null;
    }
}
