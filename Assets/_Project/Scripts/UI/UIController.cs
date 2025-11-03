using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject gameCanvas;
    
    private void OnEnable()
    {
        GameEvents.OnGameStarted += EnableGameUI;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= EnableGameUI;
    }

    private void EnableGameUI()
    {
        menuCanvas.SetActive(false);
        gameCanvas.SetActive(true);
    }
}
