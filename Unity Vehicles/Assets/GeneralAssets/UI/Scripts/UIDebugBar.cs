using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIDebugBar : MonoBehaviour
{
    public string Label = "Label";
    public float Range = 1f;
    public float Value = 0f;
    public float WarningThreshold = 0.9f;

    [Header("Visuals")]
    public Color BarColorPositive = new Color(0.8f, 0.8f, 0.8f);
    public Color BarColorNegative = new Color(0.8f, 0.3f, 0.3f);
    public Color BarColorWarning = new Color(0.8f, 0.6f, 0.3f);
    public Image FillBar;
    public TMP_Text LabelTextBox;
    public TMP_Text ValueTextBox;


    public void Start()
    {
        LabelTextBox.text = Label;
    }
    public void SetCurrentValue (float value)
    {
        Value = value;
        float fillPercentage = Mathf.Clamp01(Value / Range);
        FillBar.fillAmount = fillPercentage;

        if (Value >= WarningThreshold)
        {
            FillBar.color = BarColorWarning;
        }
        else
        {
            if (Value >= 0f)
            {
                FillBar.color = BarColorPositive;
            }
            else
            {
                FillBar.color = BarColorNegative;
            }
        }

        ValueTextBox.text = Value.ToString("F1");
    }

}
