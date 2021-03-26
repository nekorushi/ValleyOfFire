using System.Collections.Generic;
using UnityEngine;

public struct DamageValue
{
    public DamageValue(float baseDmg, float multiplier)
    {
        baseDamage = baseDmg;
        totalDamage = baseDmg * multiplier;
        bonusDamage = totalDamage - baseDmg;
    }

    public readonly float baseDamage;
    public readonly float bonusDamage;
    public readonly float totalDamage;
}

public class DamageConfig
{
    public enum Types
    {
        Heal,
        Fire,
        Normal
    }

    public static Dictionary<Types, Color> Colors = new Dictionary<Types, Color>() {
        { Types.Normal, Color.red },
        { Types.Fire, new Color(1, .5f, 0) },
        { Types.Heal, Color.green }
    };
}
