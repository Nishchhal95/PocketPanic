using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image image;
    [field: SerializeField] public CardType CardType { get; private set; }

    public event Action<CardType> OnClick;
    public event Action FAKEADD;

    public void Init(CardType cardType, Sprite sprite)
    {
        CardType = cardType;
        image.sprite = sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                Logger.Log($"Player played {CardType} card");
                OnClick?.Invoke(CardType);
                break;

            case PointerEventData.InputButton.Right:
                Logger.Log($"Right-clicked on {CardType}");
                FAKEADD?.Invoke();
                break;
        }
    }
}
