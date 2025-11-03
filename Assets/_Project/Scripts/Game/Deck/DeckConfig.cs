using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DeckConfig", menuName = "Cards/Deck Config")]
public class DeckConfig : ScriptableObject
{
    [Serializable]
    public class CardEntry
    {
        public CardType type;
        public int count;
    }

    public List<CardEntry> cards;
}
