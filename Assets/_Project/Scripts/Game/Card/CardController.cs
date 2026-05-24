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
    [field: SerializeField] public CardFace CardFace { get; private set; }
    
    [SerializeField] private float hoverMoveDistance = 100f;
    [SerializeField] private Vector3 selectedScale = new(1.2f, 1.2f, 1.2f);
    [SerializeField] private float tweenDuration = 0.25f;

    [SerializeField] private Sprite faceUpSprite;

    public event Action<Guid, CardType> OnClick;
    private bool isSelected;
    private bool isLocal;

    public void Init(CardRuntimeData cardRuntimeData)
    {
        CardType = cardRuntimeData.CardType;
        CardInstanceLocal = cardRuntimeData.CardLocalGuid;
        faceUpSprite = cardRuntimeData.FaceUpSprite;
        isLocal = cardRuntimeData.IsLocal;

        FaceDown();
    }

    public void ChangeOwnerShip(bool isLocal)
    {
        this.isLocal = isLocal;
    }

    public void FaceUp()
    {
        image.sprite = faceUpSprite;
        CardFace = CardFace.UP;
    }

    public void FaceDown()
    {
        image.sprite = CardRuntimeData.FaceDownSprite;
        CardFace = CardFace.DOWN;
    }

    public void SetCardSizeFull()
    {
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardRuntimeData.FullCardHeight);
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardRuntimeData.FullCardWidth);
    }

    public void SetCardSizeSmall()
    {
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, CardRuntimeData.SmallCardHeight);
        RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CardRuntimeData.SmallCardWidth);
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
    public static float FullCardWidth;
    public static float FullCardHeight;
    public static float SmallCardWidth;
    public static float SmallCardHeight;
    public static Sprite FaceDownSprite;
    
    public CardType CardType;
    public Guid CardLocalGuid;
    public Sprite FaceUpSprite;
    public bool IsLocal;

    public CardRuntimeData(CardType cardType, Guid cardLocalGuid, Sprite faceUpSprite, bool isLocal)
    {
        CardType = cardType;
        CardLocalGuid = cardLocalGuid;
        FaceUpSprite = faceUpSprite;
        IsLocal = isLocal;
    }
}

public enum CardFace
{
    UP = 1,
    DOWN = 2
}
