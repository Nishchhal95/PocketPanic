using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlaceExplodeBackUIController : MonoBehaviour
{
    [SerializeField] private Button confirmButton;
    [SerializeField] private Slider uiSlider;
    [SerializeField] private Transform cardParent;
    [SerializeField] private GameObject stackCardPrefab;
    [SerializeField] private TMP_Text sliderValueTextField;
    [SerializeField] private Image timerFill;
    private Action<int> OnConfirmAction;

    private int selectedValue = 0;
    
    private bool timerStarted;
    private float timerDuration;
    private float currentTime;

    public void Init(int maxValue, float timer, Action<int> onConfirm)
    {
        confirmButton.onClick.RemoveListener(OnConfirmClick);
        confirmButton.onClick.AddListener(OnConfirmClick);

        uiSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        
        uiSlider.maxValue = maxValue;
        uiSlider.onValueChanged.AddListener(OnSliderValueChanged);

        OnConfirmAction = onConfirm;
        timerDuration = timer;
        currentTime = timer;
        timerStarted = true;
        
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!timerStarted || currentTime <= 0)
        {
            return;
        }

        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            OnConfirmClick();
        }

        timerFill.fillAmount = currentTime / timerDuration;
    }

    private void OnConfirmClick()
    {
        OnConfirmAction?.Invoke(selectedValue);

        timerStarted = false;
        timerDuration = 0;
        gameObject.SetActive(false);
    }

    private void OnSliderValueChanged(float value)
    {
        selectedValue = (int)value;
        sliderValueTextField.SetText(selectedValue.ToString());
    }
}
