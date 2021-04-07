using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitTooltip : MonoBehaviour
{
    private static UnitTooltip _instance;
    public static UnitTooltip Instance {
        get
        {
            if (_instance == null) _instance = FindObjectOfType<UnitTooltip>();
            return _instance;
        }
    }

    [SerializeField] private GameObject wrapper;

    [SerializeField] private Image classIcon;
    [SerializeField] private TMP_Text className;
    [SerializeField] private Image portrait;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text shieldText;

    [SerializeField] private List<ExtraDamageValue> dmgDealtFields;
    [SerializeField] private List<ExtraDamageValue> dmgReceivedFields;

    [SerializeField] private List<UnitConfig> unitClasses;

    private void Awake()
    {
        wrapper.SetActive(false);
    }

    public void SetUnit(Unit unit)
    {
        if (unit != null)
        {
            classIcon.sprite = unit.unitClass.icon;
            className.text = unit.unitClass.Type.ToString();
            portrait.sprite = unit.unitClass.portraits[unit.Player.faction];
            healthText.text = UIHelpers.FormatHealth(unit.Health);
            shieldText.text = UIHelpers.FormatShield(unit.Shield);

            RenderExtraDamageList(unit.unitClass.Type, true, dmgDealtFields);
            RenderExtraDamageList(unit.unitClass.Type, false, dmgReceivedFields);
        }

        wrapper.SetActive(unit != null);
    }

    private void RenderExtraDamageList(UnitType unitType, bool asAttacker, List<ExtraDamageValue> fields)
    {
        int currentField = -1;
        foreach (KeyValuePair<UnitType, float> extraDamage in GetExtraDamage(unitType, asAttacker))
        {
            currentField++;

            UnitConfig unitClass = unitClasses.Find(unitConfig => unitConfig.Type == extraDamage.Key);
            fields[currentField].SetValue(unitClass, extraDamage.Value, asAttacker);
        }
        SortChildrenByDamage(fields);
    }

    private Dictionary<UnitType, float> GetExtraDamage(UnitType unitClass, bool asAttacker)
    {
        Dictionary<UnitType, float> result = new Dictionary<UnitType, float>();
        UnitType[] enemyTypes = Array.FindAll(
                (UnitType[])Enum.GetValues(typeof(UnitType)),
                unitType => unitType != unitClass
            );

        foreach(UnitType enemyType in enemyTypes)
        {
            result.Add(enemyType, asAttacker
                ? UnitsConfig.Instance.GetExtraDamage(unitClass, enemyType)
                : UnitsConfig.Instance.GetExtraDamage(enemyType, unitClass)
            );
        }

        return result;
    }


    public static void SortChildrenByDamage(List<ExtraDamageValue> fields)
    {
        fields.Sort((ExtraDamageValue v1, ExtraDamageValue v2) => {
            if (v1.DamageValue > v2.DamageValue) return 1;
            if (v1.DamageValue < v2.DamageValue) return -1;
            return 0;
        });

        for (int i = 0; i < fields.Count; ++i)
        {
            Transform valueTransform = fields[i].gameObject.transform;
            valueTransform.SetSiblingIndex(i);
        }
    }
}
