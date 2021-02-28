using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class UnitsConfig : MonoBehaviour
{
    public static UnitsConfig Instance;

    public List<string> unitTypes;
    [HideInInspector]
    public SerializableDictionary<UnitTypes, SerializableDictionary<UnitTypes, float>> damageValues =
        new SerializableDictionary<UnitTypes, SerializableDictionary<UnitTypes, float>>();

    private void Start()
    {
        Instance = this;
    }

    public float GetDamageValue(UnitTypes attacker, UnitTypes defender)
    {
        if (damageValues.ContainsKey(attacker))
        {
            if (damageValues[attacker].ContainsKey(defender))
            {
                return damageValues[attacker][defender];
            }
        }

        return 1;
    }

    public void SetDamageValue(UnitTypes attacker, UnitTypes defender, float value)
    {
        if (!damageValues.ContainsKey(attacker)) damageValues.Add(attacker, new SerializableDictionary<UnitTypes, float>());

        damageValues[attacker][defender] = value;
    }
}
