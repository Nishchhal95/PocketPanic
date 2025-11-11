using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public static class CardAnimationController
{
    public static async Task AnimateCardToAnchored(CardController cardController, Vector3 anchoredPos, float duration)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(cardController.RectTransform.DOAnchorPos(anchoredPos, duration).SetEase(Ease.InOutBack));
        await sequence.AsyncWaitForCompletion();
    }
    
    public static async Task AnimateCardToPosition(CardController cardController, Vector3 to, float duration)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(cardController.RectTransform.DOAnchorPos(to, duration).SetEase(Ease.InOutBack));
        await sequence.AsyncWaitForCompletion();
    }
}
