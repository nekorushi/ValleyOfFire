using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ValueBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform barFill;

    [SerializeField]
    private TMP_Text valueText;

    [SerializeField]
    private Color fillColor;

    private void Awake()
    {
        barFill.GetComponent<Image>().color = fillColor;
    }

    public void SetValue(float currentValue, float maxValue)
    {
        barFill.anchorMax = new Vector2(currentValue / maxValue, barFill.anchorMax.y);
        valueText.text = UIHelpers.FormatHealth(currentValue);
    }
}
