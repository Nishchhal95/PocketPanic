using UnityEngine;

public class MainMenuCanvasController : MonoBehaviour
{
    [SerializeField] private GameObject photonLoadingPanel;

    private void OnEnable()
    {
        PhotonNetworkController.ConnectedToMaster += OnConnectedToMaster;
    }

    private void OnDisable()
    {
        PhotonNetworkController.ConnectedToMaster -= OnConnectedToMaster;
    }

    private void OnConnectedToMaster()
    {
        if (photonLoadingPanel.activeSelf)
        {
            photonLoadingPanel.SetActive(false);
        }
    }
}
