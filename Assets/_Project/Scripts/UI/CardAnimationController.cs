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
    
    public static async Task AnimateCardMoveToTargetPosition(
        CardController cardController,
        Vector2 targetPos,
        CardFace cardFace)
    {
        RectTransform rect = cardController.RectTransform;

        float duration = GameConfig.ANIMATION_DEAL_CARD_DURATION;

        Sequence mainSequence = DOTween.Sequence();

        float randomZ = Random.Range(-12f, 12f);

        // Movement
        mainSequence.Join(
            rect.DOAnchorPos(targetPos, duration)
                .SetEase(Ease.OutCubic)
        );

        mainSequence.Join(
            rect.DORotate(new Vector3(0, 0, randomZ), duration)
                .SetEase(Ease.OutSine)
        );

        // Flip
        if (cardController.CardFace != cardFace)
        {
            mainSequence.Join(CreateFlipSequence(cardController, duration));
        }

        // Landing pop
        mainSequence.Append(
            rect.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad)
        );

        mainSequence.Append(
            rect.DOScale(1f, 0.1f).SetEase(Ease.InQuad)
        );

        mainSequence.Join(
            rect.DORotate(Vector3.zero, 0.2f)
                .SetEase(Ease.OutSine)
        );

        await mainSequence.AsyncWaitForCompletion();
    }
    
    
    public static async Task AnimateCardMoveToTargetPosition(
        CardController cardController,
        Vector2 targetPos,
        CardFace cardFace,
        bool makeCardSmall)
    {
        RectTransform rect = cardController.RectTransform;

        float duration = GameConfig.ANIMATION_DEAL_CARD_DURATION;

        Sequence mainSequence = DOTween.Sequence();

        float randomZ = Random.Range(-12f, 12f);

        // Movement
        mainSequence.Join(
            rect.DOAnchorPos(targetPos, duration)
                .SetEase(Ease.OutCubic)
        );

        mainSequence.Join(
            rect.DORotate(new Vector3(0, 0, randomZ), duration)
                .SetEase(Ease.OutSine)
        );

        // Flip
        if (cardController.CardFace != cardFace)
        {
            mainSequence.Join(CreateFlipSequence(cardController, duration));
        }

        mainSequence.Join(makeCardSmall
            ? AnimateToSmallSize(cardController, duration)
            : AnimateToFullSize(cardController, duration)
        );

        // Landing pop
        mainSequence.Append(
            rect.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad)
        );

        mainSequence.Append(
            rect.DOScale(1f, 0.1f).SetEase(Ease.InQuad)
        );

        mainSequence.Join(
            rect.DORotate(Vector3.zero, 0.2f)
                .SetEase(Ease.OutSine)
        );

        await mainSequence.AsyncWaitForCompletion();
    }

    private static Sequence CreateFlipSequence(CardController cardController, float duration)
    {
        RectTransform rect = cardController.RectTransform;

        Sequence flip = DOTween.Sequence();

        flip.AppendInterval(0.05f);

        flip.Append(
            rect.DOScaleX(0, duration * 0.4f)
                .SetEase(Ease.InBack)
        );

        flip.AppendCallback(() =>
        {
            if (cardController.CardFace == CardFace.UP)
                cardController.FaceDown();
            else
                cardController.FaceUp();
        });

        flip.Append(
            rect.DOScaleX(1, duration * 0.4f)
                .SetEase(Ease.OutBack)
        );

        flip.Join(
            rect.DOScaleY(1.1f, duration * 0.4f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
        );

        return flip;
    }

    private static Sequence AnimateToFullSize(CardController cardController, float duration)
    {
        RectTransform rect = cardController.RectTransform;
        Sequence seq = DOTween.Sequence();

        seq.Join(
            rect.DOSizeDelta(
                new Vector2(CardRuntimeData.FullCardWidth, CardRuntimeData.FullCardHeight),
                duration
            ).SetEase(Ease.OutCubic)
        );

        return seq;
    }

    private static Sequence AnimateToSmallSize(CardController cardController, float duration)
    {
        RectTransform rect = cardController.RectTransform;
        Sequence seq = DOTween.Sequence();

        seq.Join(
            rect.DOSizeDelta(
                new Vector2(CardRuntimeData.SmallCardWidth, CardRuntimeData.SmallCardHeight),
                duration
            ).SetEase(Ease.OutCubic)
        );

        return seq;
    }
}
