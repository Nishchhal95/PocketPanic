using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public CardType type;
    public string displayName;
    public string description;
    public Sprite artwork;
}

public enum CardType
{
    Explode = 0,
    Defuse = 1,
    
    //Cat Cards
    WatermelonCat = 2,
    BeardCat = 3,
    TacoCat = 4,
    RainbowCat = 5,
    WildCat = 6,
    
    //Action Cards
    Attack = 7,
    TargetAttack = 8,
    Skip = 9,
    Shuffle = 10,
    SeeTheFuture = 11,
    Favor = 12,
    AlterTheFuture = 13,
    DrawFromBottom = 14,
    Nope = 15,
}
