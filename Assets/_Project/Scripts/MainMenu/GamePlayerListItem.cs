using TMPro;
using UnityEngine;

public class GamePlayerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameTextField;
    [field: SerializeField] public string UserId { get; private set; }
    
    public void Init(string playerName, string userId)
    {
        playerNameTextField.SetText(playerName);
        this.UserId = userId;
    }
}
