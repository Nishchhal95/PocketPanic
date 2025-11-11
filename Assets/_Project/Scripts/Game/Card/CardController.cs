using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image image;
    [field: SerializeField] public CardType CardType { get; private set; }
    [field: SerializeField] public Guid CardInstanceLocal { get; private set; }
    [field: SerializeField] public RectTransform RectTransform { get; private set; }
    [field: SerializeField] public Vector2 OriginalPos { get; private set; }
    
    [SerializeField] private float hoverMoveDistance = 100f;
    [SerializeField] private Vector3 selectedScale = new(1.2f, 1.2f, 1.2f);
    [SerializeField] private float tweenDuration = 0.25f;

    public event Action<Guid, CardType> OnClick;
    private bool isSelected;
    private bool isLocal;

    public void Init(CardType cardType, Guid cardInstanceLocal, Sprite sprite, bool isLocal)
    {
        CardType = cardType;
        CardInstanceLocal = cardInstanceLocal;
        image.sprite = sprite;
        this.isLocal = isLocal;
    }

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {       
        if (!isLocal || isSelected)
        {
            return;
        }
        
        RectTransform.DOKill(true);
        RectTransform.localScale = Vector3.one;
        RectTransform.DOScale(selectedScale, tweenDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isLocal || isSelected)
        {
            return;
        }
        
        RectTransform.DOKill(true);
        RectTransform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isLocal)
        {
            return;
        }
        OnClick?.Invoke(CardInstanceLocal, CardType);
    }

    public void SetPosition(Vector3 position)
    {
        OriginalPos = position;
    }

    public void Select()
    {
        isSelected = true;
        
        RectTransform.DOKill(true);
        DOTween.Sequence()
            .Append(RectTransform.DOAnchorPos(OriginalPos + Vector2.up * hoverMoveDistance, tweenDuration)
                .SetEase(Ease.OutQuad))
            .Join(RectTransform.DOScale(selectedScale, tweenDuration)
                .SetEase(Ease.OutQuad));
    }

    public void Deselect()
    {
        isSelected = false;
        
        RectTransform.DOKill(true);
        DOTween.Sequence().Append(RectTransform.DOAnchorPos(OriginalPos, tweenDuration).SetEase(Ease.OutQuad))
            .Join(RectTransform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutQuad));
    }
}
