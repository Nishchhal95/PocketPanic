using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostMenuWindow : MenuWindow
{
    [SerializeField] private TMP_InputField playerCountInputField;
    [SerializeField] private Button hostGameButton;
    [SerializeField] private GameObject loading;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        playerCountInputField.onValueChanged.AddListener(OnPlayerCountChanged);
        hostGameButton.onClick.AddListener(OnHostGameClicked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        playerCountInputField.onValueChanged.RemoveListener(OnPlayerCountChanged);
        hostGameButton.onClick.RemoveListener(OnHostGameClicked);
    }

    private void OnPlayerCountChanged(string newValue)
    {
        if (string.IsNullOrEmpty(newValue) || string.IsNullOrWhiteSpace(newValue))
        {
            hostGameButton.interactable = false;
            return;
        }

        if (!int.TryParse(newValue, out _))
        {
            hostGameButton.interactable = false;
            return;
        }

        hostGameButton.interactable = true;
    }

    private void OnHostGameClicked()
    {
        string playerCountText = playerCountInputField.text;
        if (string.IsNullOrEmpty(playerCountText) || 
            string.IsNullOrWhiteSpace(playerCountText) || 
            !int.TryParse(playerCountText, out var playerCount))
        {
            return;
        }

        PhotonNetworkController.CreateGame(new PhotonGameConfig
        {
            PlayerCount = playerCount
        });
        
        loading.SetActive(true);
    }
}
