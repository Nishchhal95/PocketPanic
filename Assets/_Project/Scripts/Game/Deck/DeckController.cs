using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    [SerializeField] private DeckConfig deckConfig;

    private List<CardType> drawDeck = new();
    private List<CardType> playedCardsDeck = new();

    public void BuildDeckWithoutExplodeAndDiffuse()
    {
        foreach (DeckConfig.CardEntry cardEntry in deckConfig.cards)
        {
            if (cardEntry.type is CardType.Explode or CardType.Defuse)
            {
                continue;
            }
            
            for (int i = 0; i < cardEntry.count; i++)
            {
                drawDeck.Add(cardEntry.type);
            }
        }
    }
    
    public void AddExplode(int playerCount)
    {
        int explodingCards = playerCount - 1;
        DeckConfig.CardEntry cardEntry = deckConfig.cards.Find(x => x.type == CardType.Explode);
        for (int i = 0; i < explodingCards; i++)
        {
            drawDeck.Add(cardEntry.type);
        }
    }

    public void AddDefuse(int extraDefuseCount)
    {
        DeckConfig.CardEntry cardEntry = deckConfig.cards.Find(x => x.type == CardType.Defuse);
        for (int i = 0; i < extraDefuseCount; i++)
        {
            drawDeck.Add(cardEntry.type);
        }
    }
    
    public void Shuffle(int seed)
    {
        var rng = new System.Random(seed);
        int n = drawDeck.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (drawDeck[k], drawDeck[n]) = (drawDeck[n], drawDeck[k]);
        }
    }

    public CardType DrawSpecificCard(CardType cardType)
    {
        int cardIndex = drawDeck.FindIndex(x => x == cardType);
        CardType card = drawDeck[cardIndex];
        drawDeck.RemoveAt(cardIndex);
        return card;
    }

    public CardType DrawCardTop()
    {
        if (drawDeck.Count == 0)
        {
            Logger.Error("Deck is Empty");
            return CardType.Nope;
        }

        CardType cardType = drawDeck[0];
        drawDeck.RemoveAt(0);
        return cardType;
    }
    
    public CardType DrawCardBottom()
    {
        if (drawDeck.Count == 0)
        {
            Logger.Error("Deck is Empty");
            return CardType.Nope;
        }

        CardType cardType = drawDeck[^1];
        drawDeck.RemoveAt(drawDeck.Count - 1);
        return cardType;
    }
    
    public CardType PeekCardAtIndexFromTop(int index)
    {
        if (drawDeck.Count == 0)
        {
            Logger.Error("Deck is Empty");
            return CardType.Nope;
        }

        CardType cardType = drawDeck[index];
        return cardType;
    }

    public void PlayCard(CardType cardType)
    {
        playedCardsDeck.Add(cardType);
    }
}
