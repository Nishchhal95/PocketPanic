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
        sequence.Append(cardController.RectTransform.DOMove(to, duration).SetEase(Ease.InOutBack));
        sequence.Join(cardController.RectTransform.DOScale(Vector3.one, duration).SetEase(Ease.OutQuad));
        await sequence.AsyncWaitForCompletion();
    }
    
    public static async Task AnimateCardToPositionWithRandomRotation(CardController cardController, Vector3 to, float duration)
    {
        float randomZ = Random.Range(-10f, 10f);
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(cardController.RectTransform.DOMove(to, duration).SetEase(Ease.InOutBack));
        sequence.Join(cardController.RectTransform.DORotate(new Vector3(0, 0, randomZ), duration).SetEase(Ease.OutQuad));
        sequence.Join(cardController.RectTransform.DOScale(Vector3.one, duration).SetEase(Ease.OutQuad));
        await sequence.AsyncWaitForCompletion();
    }
}
