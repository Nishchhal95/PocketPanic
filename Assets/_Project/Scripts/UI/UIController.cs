using Photon.Realtime;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject gameCanvas;
    
    private void OnEnable()
    {
        PhotonNetworkController.JoinedRoom += EnableGameUI;
    }

    private void OnDisable()
    {
        PhotonNetworkController.JoinedRoom -= EnableGameUI;
    }

    private void EnableGameUI(RoomInfo _)
    {
        menuCanvas.SetActive(false);
        gameCanvas.SetActive(true);
    }
}
