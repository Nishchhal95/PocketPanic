using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuWindow : MonoBehaviour
{
    public static event Action OnMenuWindowClosed;
    
    [SerializeField] private GameObject menuContainer;
    [SerializeField] private Button closeButton;

    protected virtual void OnEnable()
    {
        closeButton.onClick.AddListener(Close);
    }

    protected virtual void OnDisable()
    {
        closeButton.onClick.RemoveListener(Close);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        OnMenuWindowClosed?.Invoke();
    }
}
