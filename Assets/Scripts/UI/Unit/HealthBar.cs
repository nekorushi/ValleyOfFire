using UnityEngine;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform barFill;

    [SerializeField]
    private TMP_Text valueText;

    public void SetValue(float health, float maxHealth)
    {
        barFill.anchorMax = new Vector2(health / maxHealth, barFill.anchorMax.y);
        valueText.text = UIHelpers.FormatHealth(health);
    }
}
