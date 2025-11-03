using TMPro;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameTextField;
    [SerializeField] private int actorNumber;
    [SerializeField] private string userId;
    
    public void Init(string playerName, int actorNumber, string userId)
    {
        playerNameTextField.SetText(playerName);
        this.actorNumber = actorNumber;
        this.userId = userId;
    }
}
