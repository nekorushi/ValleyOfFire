using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class UnitsConfig : MonoBehaviour
{
    public static UnitsConfig Instance;

    public List<string> unitTypes;
    [HideInInspector]
    public SerializableDictionary<UnitTypes, SerializableDictionary<UnitTypes, float>> vsClassExtraDamages =
        new SerializableDictionary<UnitTypes, SerializableDictionary<UnitTypes, float>>();

    private void Start()
    {
        Instance = this;
    }

    public float GetExtraDmgVsClass(UnitTypes attacker, UnitTypes defender)
    {
        bool shouldApplyClassModifier = vsClassExtraDamages.ContainsKey(attacker)
            && vsClassExtraDamages[attacker].ContainsKey(defender);

        return shouldApplyClassModifier ? vsClassExtraDamages[attacker][defender] : 1f;
    }

    public float GetExtraDamage(UnitTypes attacker, UnitTypes defender)
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

    public void SetExtraDamage(UnitTypes attacker, UnitTypes defender, float value)
    {
        if (!vsClassExtraDamages.ContainsKey(attacker)) vsClassExtraDamages.Add(attacker, new SerializableDictionary<UnitTypes, float>());

        vsClassExtraDamages[attacker][defender] = value;
    }
}
