using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class HandLayoutManager
{
    [SerializeField] private float cardSpacing = 160f;
    [SerializeField] private float padding = 20f;

    private List<CardController> cardControllers;
    private float cardWidth;
    private float containerWidth;

    private bool initialized;

    public HandLayoutManager(List<CardController> cardControllers, float cardWidth, float containerWidth, float cardSpacing = 160f)
    {
        this.cardControllers = cardControllers;
        this.cardWidth = cardWidth;
        this.containerWidth = containerWidth;
        this.cardSpacing = cardSpacing;

        initialized = true;
    }

    public void UpdateHandLayout()
    {
        if (!initialized)
        {
            return;
        }
        
        int count = cardControllers.Count;
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
            
            RectTransform rect = cardControllers[i].RectTransform;
            rect.DOAnchorPos(new Vector2(xPos, 0), GameConfig.ANIMATION_HAND_ADJUST_DURATION).SetEase(Ease.OutQuad);
            cardControllers[i].SetPosition(new Vector2(xPos, 0));
            cardControllers[i].transform.SetSiblingIndex(i);

        }
    }
    
    public Vector2 GetNextCardPosition()
    {
        if (!initialized)
        {
            return Vector2.zero;
        }

        int count = cardControllers.Count + 1;
        float spacing = GetValidCardSpacing();
        float totalWidth = (count - 1) * spacing;
        float startX = -totalWidth / 2f;
        float xPos = startX + (count - 1) * spacing;

        return new Vector2(xPos, 0);
    }
    
    private float GetValidCardSpacing()
    {
        int cardCount = cardControllers.Count;
        if (cardCount <= 1)
        {
            return cardSpacing;
        }
        
        float maxSpacing = (containerWidth - cardWidth - padding)/ (cardCount - 1);
        return Mathf.Min(cardSpacing, maxSpacing);
    }
}
