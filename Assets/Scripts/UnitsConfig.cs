﻿using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class UnitsConfig : MonoBehaviour
{
    public static UnitsConfig Instance;

    public List<string> unitTypes;
    [HideInInspector]
    public SerializableDictionary<UnitTypes, SerializableDictionary<UnitTypes, float>> damageMultipliers =
        new SerializableDictionary<UnitTypes, SerializableDictionary<UnitTypes, float>>();

    private void Start()
    {
        Instance = this;
    }

    public DamageValue GetDamageValue(float baseDmg, UnitTypes attacker, UnitTypes defender)
    {
        if (damageMultipliers.ContainsKey(attacker))
        {
            if (damageMultipliers[attacker].ContainsKey(defender))
            {
                float multplier = damageMultipliers[attacker][defender];
                return new DamageValue(baseDmg, multplier);
            }
        }

        return new DamageValue(baseDmg, 1f);
    }

    public float GetDamageMultiplier(UnitTypes attacker, UnitTypes defender)
    {
        if (damageMultipliers.ContainsKey(attacker))
        {
            if (damageMultipliers[attacker].ContainsKey(defender))
            {
                return damageMultipliers[attacker][defender];
            }
        }

        return 1;
    }

    public void SetDamageMultiplier(UnitTypes attacker, UnitTypes defender, float value)
    {
        if (!damageMultipliers.ContainsKey(attacker)) damageMultipliers.Add(attacker, new SerializableDictionary<UnitTypes, float>());

        damageMultipliers[attacker][defender] = value;
    }
}
