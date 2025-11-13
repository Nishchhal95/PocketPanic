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

    [SerializeField] private Sprite faceUpSprite;
    [SerializeField] private Sprite faceDownSprite;

    private float fullCardWidth;
    private float fullCardHeight;
    
    private float smallCardWidth;
    private float smallCardHeight;

    public event Action<Guid, CardType> OnClick;
    private bool isSelected;
    private bool isLocal;

    public void Init(CardRuntimeData cardRuntimeData)
    {
        CardType = cardRuntimeData.CardType;
        CardInstanceLocal = cardRuntimeData.CardLocalGuid;
        faceUpSprite = cardRuntimeData.FaceUpSprite;
        faceDownSprite = cardRuntimeData.FaceDownSprite;
        isLocal = cardRuntimeData.IsLocal;

        fullCardWidth = cardRuntimeData.FullCardWidth;
        fullCardHeight = cardRuntimeData.FullCardHeight;
        
        smallCardWidth = cardRuntimeData.SmallCardWidth;
        smallCardHeight = cardRuntimeData.SmallCardHeight;

        image.sprite = isLocal ? faceUpSprite : faceDownSprite;
    }

    public void FaceUp()
    {
        image.sprite = faceUpSprite;
    }

    public void FaceDown()
    {
        image.sprite = faceDownSprite;
    }

    public void SetCardSizeFull()
    {
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullCardHeight);
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fullCardWidth);
    }

    public void SetCardSizeSmall()
    {
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, smallCardHeight);
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, smallCardWidth);
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

public class CardRuntimeData
{
    public CardType CardType;
    public Guid CardLocalGuid;
    public Sprite FaceUpSprite;
    public Sprite FaceDownSprite;
    public bool IsLocal;
    public float FullCardWidth;
    public float FullCardHeight;
    public float SmallCardWidth;
    public float SmallCardHeight;

    public CardRuntimeData(CardType cardType, Guid cardLocalGuid, Sprite faceUpSprite, Sprite faceDownSprite, 
        bool isLocal, float fullCardWidth, float fullCardHeight, float smallCardWidth, float smallCardHeight)
    {
        CardType = cardType;
        CardLocalGuid = cardLocalGuid;
        FaceUpSprite = faceUpSprite;
        FaceDownSprite = faceDownSprite;
        IsLocal = isLocal;
        FullCardWidth = fullCardWidth;
        FullCardHeight = fullCardHeight;
        SmallCardWidth = smallCardWidth;
        SmallCardHeight = smallCardHeight;
    }
}
