using UnityEngine;

public class GameCanvasController : MonoBehaviour
{
    // 1 Player cannot Play alone and Max is 10
    [field: SerializeField] public PlayerCountToSlots[] PlayerCountToSlotsArray { get; private set; } = new PlayerCountToSlots[9];
}
