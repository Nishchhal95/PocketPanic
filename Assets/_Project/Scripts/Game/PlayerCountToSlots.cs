using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerCountToSlots
{
    public string name;
    public int PlayerCount;
    public List<Transform> PlayerSlotTransforms = new();
}
