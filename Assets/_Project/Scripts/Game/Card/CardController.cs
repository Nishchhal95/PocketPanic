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
    
    [SerializeField] private float hoverMoveDistance = 100f;
    [SerializeField] private Vector3 selectedScale = new(1.2f, 1.2f, 1.2f);
    [SerializeField] private float tweenDuration = 0.25f;

    public event Action<Guid, CardType> OnClick;
    
    private RectTransform rectTransform;
    private Vector2 originalPos;
    private bool isSelected;
    private bool isLocal;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

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
        
        rectTransform.DOKill(true);
        rectTransform.localScale = Vector3.one;
        rectTransform.DOScale(selectedScale, tweenDuration).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isLocal || isSelected)
        {
            return;
        }
        
        rectTransform.DOKill(true);
        rectTransform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutQuad);
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
        originalPos = position;
    }

    public void Select()
    {
        isSelected = true;
        
        rectTransform.DOKill(true);
        DOTween.Sequence()
            .Append(rectTransform.DOAnchorPos(originalPos + Vector2.up * hoverMoveDistance, tweenDuration)
                .SetEase(Ease.OutQuad))
            .Join(rectTransform.DOScale(selectedScale, tweenDuration)
                .SetEase(Ease.OutQuad));
    }

    public void Deselect()
    {
        isSelected = false;
        
        rectTransform.DOKill(true);
        DOTween.Sequence().Append(rectTransform.DOAnchorPos(originalPos, tweenDuration).SetEase(Ease.OutQuad))
            .Join(rectTransform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutQuad));
    }
}
