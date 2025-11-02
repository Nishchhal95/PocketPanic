using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameListUIItem : MonoBehaviour
{
    private const string PLAYER_STRING_FORMAT = "{0}/{1}";
    
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text roomCodeTextField;
    [SerializeField] private TMP_Text playersTextField;

    private string roomCode = "";

    private void OnEnable()
    {
        joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnDisable()
    {
        joinButton.onClick.RemoveListener(OnJoinButtonClicked);
    }

    public void Init(string roomCode, int currentPlayers, int maxPlayers)
    {
        this.roomCode = roomCode;
        roomCodeTextField.SetText(roomCode);
        playersTextField.SetText(string.Format(PLAYER_STRING_FORMAT, currentPlayers, maxPlayers));
    }

    private void OnJoinButtonClicked()
    {
        PhotonNetworkController.JoinRoom(roomCode);
    }
}
