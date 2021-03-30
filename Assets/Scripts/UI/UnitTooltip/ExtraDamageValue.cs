using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtraDamageValue : MonoBehaviour
{
    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text className;
    [SerializeField] private TMP_Text extraDamageValue;

    private float damageValue;
    public float DamageValue { get { return damageValue; } }

    public void SetValue(UnitConfig unitClass, float extraValue, bool moreIsBetter = true)
    {
        classIcon.sprite = unitClass.icon;
        className.text = unitClass.Type.ToString();

        damageValue = extraValue;
        extraDamageValue.text = string.Format("{0}{1}",
            damageValue > 0
                ? "+"
                : damageValue < 0 ? "-" : "",
            damageValue
        );

        extraDamageValue.color = damageValue == 0
            ? Color.white
            : moreIsBetter && extraValue > 0 || !moreIsBetter && extraValue < 0
                ? Color.green : Color.red;
    }
}
