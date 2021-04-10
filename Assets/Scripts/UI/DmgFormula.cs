using TMPro;
using UnityEngine;

public class DmgFormula : MonoBehaviour
{
    [SerializeField] private TMP_Text baseDamageText;
    [SerializeField] private TMP_Text extraDamageText;

    [SerializeField] private Color positiveDmgColor;
    [SerializeField] private Color negativeDmgColor;

    public void SetValue(DamageValue damage, Unit unit)
    {
        DealtDamage damageDealt = damage.DamageDealt(unit, false);

        baseDamageText.gameObject.SetActive(false);
        extraDamageText.gameObject.SetActive(false);

        if (
            damageDealt.resistanceEffect == DamageResistanceEffect.CancelDamage
            || damageDealt.resistanceEffect == DamageResistanceEffect.SetDamageToFixedAmount
        )
        {
            ShowBaseDamage(damageDealt.value);
        } else
        {
            ShowBaseDamage(damage.baseFlatDmg);
            ShowExtraDamage(damage.extraFlatDamage);
        }
    }
    private void ShowBaseDamage(float value)
    {
        baseDamageText.gameObject.SetActive(true);
        baseDamageText.text = Mathf.Abs(value).ToString();
    }

    private void ShowExtraDamage(float value)
    {
        if (value != 0)
        {
            extraDamageText.gameObject.SetActive(true);
            extraDamageText.color = value > 0 ? positiveDmgColor : negativeDmgColor;
            extraDamageText.text = string.Format("({0}{1})", value > 0 ? "+" : "-", Mathf.Abs(value));
        }
    }
}
