using TMPro;
using UnityEngine;

public sealed class NumberDial : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;
    [SerializeField] private int minimumValue;
    [SerializeField] private int maximumValue = 9;
    public int CurrentValue { get; private set; }
    private void Awake() => SetValue(minimumValue);
    public void IncreaseValue() => SetValue(CurrentValue >= maximumValue ? minimumValue : CurrentValue + 1);
    public void DecreaseValue() => SetValue(CurrentValue <= minimumValue ? maximumValue : CurrentValue - 1);

    public void SetValue(int value)
    {
        CurrentValue = Mathf.Clamp(value, minimumValue, maximumValue); 
        
        if (numberText != null) numberText.text = CurrentValue.ToString();
    }
}
