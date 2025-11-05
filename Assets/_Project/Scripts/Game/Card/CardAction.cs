using System.Collections.Generic;

public static class CardActionFactory
{
    private static readonly Dictionary<CardType, CardAction> cardActionMap = new()
    {
        { CardType.Skip, new SkipAction() },
        { CardType.Shuffle, new ShuffleAction() },
        { CardType.Attack, new AttackAction() },
    };

    public static CardAction Get(CardType cardType)
    {
        return cardActionMap.GetValueOrDefault(cardType);
    }
}

public abstract class CardAction
{
    public CardType CardType { get; private set; }
    public bool CanBeNoped { get; protected set; } = true;

    protected CardAction(CardType cardType)
    {
        CardType = cardType;
    }

    public abstract void Execute(int actorNumber, int targetActorNumber = -1);
}

public class SkipAction : CardAction
{
    public SkipAction() : base(CardType.Skip) { }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played SKIP and ends their turn!");
        GameController.Instance.ForceEndTurn();
    }
}

public class AttackAction : CardAction
{
    public AttackAction() : base(CardType.Attack) { }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played ATTACK!");
        GameController.Instance.ForcePlayerTakeTwoTurn(actorNumber + 1);
    }
}

public class ShuffleAction : CardAction
{
    public ShuffleAction() : base(CardType.Shuffle){ }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played SHUFFLE!");
        int randomShuffleSeed = UnityEngine.Random.Range(1, 99999);
        GameController.Instance.ShuffleCards(randomShuffleSeed);
    }
}