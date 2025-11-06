using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayersMenuWindow : MenuWindow
{
    [SerializeField] private Transform content;
    [SerializeField] private GamePlayerListItem listPlayerPrefab;
    [SerializeField] private Button startGameButton;
    
    private List<GamePlayerListItem> listItems = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        bool isMasterClient = PhotonNetworkController.IsMasterClient();
        startGameButton.gameObject.SetActive(isMasterClient);
        if (isMasterClient)
        {
            startGameButton.onClick.AddListener(OnStartButtonClicked);
        }
        
        CreatePlayersUI(PhotonNetworkController.GetPlayersCurrentRoom());
        PhotonNetworkController.PlayerEnteredRoom += CreateListPlayer;
        PhotonNetworkController.PlayerLeftRoom += RemoveListPlayer;
        PhotonNetworkController.MasterClientSwitched += MasterClientSwitched;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        startGameButton.onClick.RemoveListener(OnStartButtonClicked);
        
        PhotonNetworkController.PlayerEnteredRoom -= CreateListPlayer;
        PhotonNetworkController.PlayerLeftRoom -= RemoveListPlayer;
        PhotonNetworkController.MasterClientSwitched -= MasterClientSwitched;
        
        ClearUI();
    }

    protected override void OnClose()
    {
        base.OnClose();
        PhotonNetworkController.LeaveRoom();
    }

    private void CreatePlayersUI(Player[] players)
    {
        ClearUI();
        foreach (Player player in players)
        {
            CreateListPlayer(player);
        }
    }
    
    private void ClearUI()
    {
        foreach (GamePlayerListItem listItem in listItems)
        {
            Destroy(listItem.gameObject);
        }
        
        listItems.Clear();
    }
    
    private void CreateListPlayer(Player player)
    {
        GamePlayerListItem listItem = Instantiate(listPlayerPrefab, content);
        listItem.Init(player.NickName, player.UserId);
        listItems.Add(listItem);
    }

    private void RemoveListPlayer(Player player)
    {
        foreach (GamePlayerListItem listItem in listItems)
        {
            if (listItem.UserId.Equals(player.UserId))
            {
                Destroy(listItem.gameObject);
                return;
            }
        }
    }

    private void MasterClientSwitched(Player newMasterClient)
    {
        if (!newMasterClient.IsLocal)
        {
            return;
        }
        
        startGameButton.onClick.RemoveListener(OnStartButtonClicked);
        startGameButton.gameObject.SetActive(true);
        startGameButton.onClick.AddListener(OnStartButtonClicked);
    }

    private void OnStartButtonClicked()
    {
        if (!PhotonNetworkController.IsMasterClient())
        {
            Logger.Error("Only Master Clients can Start the Game");
            return;
        }
        
        CloseWithoutCallback();
        GameController.Instance.StartGameNetworked();
    }
}
