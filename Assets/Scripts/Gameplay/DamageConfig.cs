using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Normal,
    Fire,
    Heal,
}

public struct DamageValue
{
    private readonly Dictionary<DamageType, Color> typeColors;

    public readonly float baseFlatDmg;
    public readonly float extraFlatDamage;
    public readonly float totalFlatDmg;

    public readonly DamageType type;

    public DamageValue(float skillDmg, float extraDmg, DamageType damageType)
    {
        baseFlatDmg = skillDmg;
        totalFlatDmg = skillDmg + extraDmg;
        extraFlatDamage = totalFlatDmg - skillDmg;

        type = damageType;
        typeColors = new Dictionary<DamageType, Color>()
            {
                { DamageType.Normal, Color.red },
                { DamageType.Fire, new Color(1, .5f, 0) },
                { DamageType.Heal, Color.green }
            };
    }

    public Color Color {
        get { return typeColors[type]; }
    }

    public bool ShouldIgnoreShield
    {
        get { return type == DamageType.Heal; }
    }

    public float DamageAfterShield(float shield)
    {
        return ShouldIgnoreShield  ? -totalFlatDmg : -totalFlatDmg * (1 + (100 - shield) / 100);
    }
}