using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class FindMenuWindow : MenuWindow
{
    [SerializeField] private GameListUIItem gameListItemPrefab;
    [SerializeField] private Transform content;

    private List<GameListUIItem> listItems = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        
        CreateRoomUI(PhotonNetworkController.GetRoomListPhoton());
        PhotonNetworkController.RoomListUpdated += CreateRoomUI;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        PhotonNetworkController.RoomListUpdated -= CreateRoomUI;

        ClearUI();
    }

    private void CreateRoomUI(List<RoomInfo> rooms)
    {
        ClearUI();
        foreach (RoomInfo room in rooms)
        {
            GameListUIItem listItem = Instantiate(gameListItemPrefab, content);
            listItem.Init(room.Name, room.PlayerCount, room.MaxPlayers);
            listItems.Add(listItem);
        }
    }

    private void ClearUI()
    {
        foreach (GameListUIItem listItem in listItems)
        {
            Destroy(listItem.gameObject);
        }
        
        listItems.Clear();
    }
}
