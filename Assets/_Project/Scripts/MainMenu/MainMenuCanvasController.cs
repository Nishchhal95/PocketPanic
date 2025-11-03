using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuCanvasController : MonoBehaviour
{
    [SerializeField] private GameObject photonLoadingPanel;
    
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private TMP_InputField roomCodeInputField;
    
    [SerializeField] private Button hostButton;
    [SerializeField] private Button findGameButton;
    [SerializeField] private Button joinButton;

    [SerializeField] private GameObject selfMenu;
    [SerializeField] private MenuWindow hostMenu;
    [SerializeField] private MenuWindow findGameMenu;
    [SerializeField] private MenuWindow waitingForPlayersMenu;

    private void Awake()
    {
        photonLoadingPanel.SetActive(true);
        hostButton.interactable = false;
        findGameButton.interactable = false;
        roomCodeInputField.interactable = false;
        joinButton.interactable = false;
    }

    private void OnEnable()
    {
        PhotonNetworkController.ConnectedToMaster += OnConnectedToMaster;
        MenuWindow.OnMenuWindowClosed += OnMenuWindowClosed;
        PhotonNetworkController.JoinedRoom += OnPlayerJoinedRoom;
        
        playerNameInputField.onValueChanged.AddListener(OnPlayerNameInputFieldValueChanged);
        roomCodeInputField.onValueChanged.AddListener(OnRoomCodeInputFieldValueChanged);
        
        hostButton.onClick.AddListener(OnHostButtonClicked);
        findGameButton.onClick.AddListener(OnFindGameButtonClicked);
        joinButton.onClick.AddListener(OnJoinButtonClicked);
    }

    private void OnDisable()
    {
        PhotonNetworkController.ConnectedToMaster -= OnConnectedToMaster;
        MenuWindow.OnMenuWindowClosed -= OnMenuWindowClosed;
        PhotonNetworkController.JoinedRoom -= OnPlayerJoinedRoom;
        
        playerNameInputField.onValueChanged.RemoveListener(OnPlayerNameInputFieldValueChanged);
        roomCodeInputField.onValueChanged.RemoveListener(OnRoomCodeInputFieldValueChanged);
        
        hostButton.onClick.RemoveListener(OnHostButtonClicked);
        findGameButton.onClick.RemoveListener(OnFindGameButtonClicked);
        joinButton.onClick.RemoveListener(OnJoinButtonClicked);
    }

    private void OnConnectedToMaster()
    {
        if (photonLoadingPanel.activeSelf)
        {
            photonLoadingPanel.SetActive(false);
        }
    }

    private void OnMenuWindowClosed()
    {
        selfMenu.SetActive(true);
    }

    private void OnPlayerJoinedRoom(RoomInfo _)
    {
        photonLoadingPanel.SetActive(false);
        hostMenu.CloseWithoutCallback();
        findGameMenu.CloseWithoutCallback();
        
        selfMenu.SetActive(false);
        waitingForPlayersMenu.Show();
    }
    
    private void OnPlayerNameInputFieldValueChanged(string newValue)
    {
        bool enable = !string.IsNullOrEmpty(newValue) && !string.IsNullOrWhiteSpace(newValue);
        hostButton.interactable = enable;
        findGameButton.interactable = enable;
        roomCodeInputField.interactable = enable;
        
        PhotonNetworkController.SetPlayerName(newValue);
    }

    private void OnRoomCodeInputFieldValueChanged(string newValue)
    {
        joinButton.interactable = !string.IsNullOrEmpty(newValue) && !string.IsNullOrWhiteSpace(newValue);
    }

    private void OnHostButtonClicked()
    {
        selfMenu.SetActive(false);
        hostMenu.Show();
    }

    private void OnFindGameButtonClicked()
    {
        selfMenu.SetActive(false);
        findGameMenu.Show();
    }

    private void OnJoinButtonClicked()
    {
        string roomCodeText = roomCodeInputField.text;

        if (string.IsNullOrEmpty(roomCodeText) || string.IsNullOrWhiteSpace(roomCodeText))
        {
            Debug.LogError("Cannot Join an Empty or Null Room!!");
            return;
        }

        PhotonNetworkController.JoinRoom(roomCodeText);
    }
}
