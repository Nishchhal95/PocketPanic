using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FutureCardBehaviour : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image cardImage;
    [SerializeField] private TMP_Text cardPlaceText;
    [SerializeField] private CanvasGroup canvasGroup;

    private bool draggable;
    private ShowTheFutureUIController controller;
    public RectTransform RectTransform { get; private set; }

    public CardType CardType { get; private set; }

    private Vector2 dragOffset;
    private Vector2 originalPosition;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Init(Sprite sprite, CardType cardType, bool draggable, ShowTheFutureUIController controller)
    {
        cardImage.sprite = sprite;
        this.CardType = cardType;
        this.draggable = draggable;
        this.controller = controller;
        cardPlaceText.gameObject.SetActive(!draggable);
    }

    public void SetTargetPosition(float x, float y, bool animate = false)
    {
        if (animate)
            RectTransform.DOAnchorPos(new Vector2(x, y), 0.25f).SetEase(Ease.OutQuad);
        else
            RectTransform.anchoredPosition = new Vector2(x, y);

        originalPosition = RectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!draggable) return;

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false;
        dragOffset = eventData.position - (Vector2)((RectTransform)transform).position;
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!draggable) return;

        Vector2 screenPos = eventData.position;
        RectTransform.position = screenPos - dragOffset;

        // inform controller so it can live-update order
        controller.UpdateCardOrder(this);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!draggable) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        controller.UpdateLayout(); // snap to final order
    }
}
