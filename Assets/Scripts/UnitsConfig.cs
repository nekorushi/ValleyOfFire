using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class UnitsConfig : MonoBehaviour
{
    public static UnitsConfig Instance;

    public List<string> unitTypes;
    [HideInInspector]
    public SerializableDictionary<UnitType, SerializableDictionary<UnitType, float>> vsClassExtraDamages =
        new SerializableDictionary<UnitType, SerializableDictionary<UnitType, float>>();

    private void Start()
    {
        Instance = this;
    }

    public float GetExtraDamage(UnitType attacker, UnitType defender)
    {
        if (vsClassExtraDamages.ContainsKey(attacker))
        {
            if (vsClassExtraDamages[attacker].ContainsKey(defender))
            {
                return vsClassExtraDamages[attacker][defender];
            }
        }

        return 1;
    }

    public void SetExtraDamage(UnitType attacker, UnitType defender, float value)
    {
        if (!vsClassExtraDamages.ContainsKey(attacker)) vsClassExtraDamages.Add(attacker, new SerializableDictionary<UnitType, float>());

        vsClassExtraDamages[attacker][defender] = value;
    }


}
