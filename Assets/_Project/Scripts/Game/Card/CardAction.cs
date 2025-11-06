using System.Collections.Generic;

public static class CardActionFactory
{
    private static readonly Dictionary<CardType, CardAction> cardActionMap = new()
    {
        { CardType.Attack, new AttackAction() },
        { CardType.TargetAttack, new TargetAttackAction() },
        { CardType.Skip, new SkipAction() },
        { CardType.Shuffle, new ShuffleAction() },
        { CardType.SeeTheFuture, new SeeTheFutureAction() },
        { CardType.Favor, new FavorAction() },
        { CardType.AlterTheFuture, new SeeTheFutureAction() },
        { CardType.DrawFromBottom, new DrawFromBottomAction() },
        { CardType.Nope, new NopeAction() },
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

public class AttackAction : CardAction
{
    public AttackAction() : base(CardType.Attack) { }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played ATTACK!");
        GameController.Instance.ForcePlayerTakeTwoTurn(actorNumber + 1);
    }
}

public class TargetAttackAction : CardAction
{
    public TargetAttackAction() : base(CardType.TargetAttack) { }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played TARGET ATTACK!");
        GameController.Instance.ForcePlayerTakeTwoTurn(actorNumber + 1);
    }
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

public class SeeTheFutureAction : CardAction
{
    public SeeTheFutureAction() : base(CardType.SeeTheFuture){ }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played SEE THE FUTURE!");
        GameController.Instance.ShowPlayerTopCards(3);
    }
}

public class FavorAction : CardAction
{
    public FavorAction() : base(CardType.Favor){ }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played FAVOR!");
        GameController.Instance.RequestCardFromPlayer(actorNumber, actorNumber + 1, 1);
    }
}

public class AlterTheFutureAction : CardAction
{
    public AlterTheFutureAction() : base(CardType.AlterTheFuture){ }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played ALTER THE FUTURE!");
        int randomShuffleSeed = UnityEngine.Random.Range(1, 99999);
        GameController.Instance.ShuffleCards(randomShuffleSeed);
    }
}

public class DrawFromBottomAction : CardAction
{
    public DrawFromBottomAction() : base(CardType.DrawFromBottom){ }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played DRAW FROM BOTTOM!");
        GameController.Instance.DrawFromBottomAndEndTurn(actorNumber);
    }
}

public class NopeAction : CardAction
{
    public NopeAction() : base(CardType.Nope){ }

    public override void Execute(int actorNumber, int targetActorNumber = -1)
    {
        Logger.Log($"Player {actorNumber} played SHUFFLE!");
        int randomShuffleSeed = UnityEngine.Random.Range(1, 99999);
        GameController.Instance.ShuffleCards(randomShuffleSeed);
    }
}