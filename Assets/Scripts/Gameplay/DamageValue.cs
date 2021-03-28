using System.Collections.Generic;
using UnityEngine;

public enum DamageType
{
    Normal,
    Fire,
    Heal,
}

public enum DamageTrajectory
{
    SelfInflicted,
    Straight,
    Curve
}

public struct DamageValue
{
    private readonly Dictionary<DamageType, Color> typeColors;

    public readonly float baseFlatDmg;
    public readonly float extraFlatDamage;
    public readonly float totalFlatDmg;

    public readonly DamageType type;
    public readonly DamageTrajectory trajectory;

    public DamageValue(
        float skillDmg,
        float extraDmg,
        DamageType damageType,
        DamageTrajectory damageTrajectory
    ) {
        baseFlatDmg = skillDmg;
        totalFlatDmg = skillDmg + extraDmg;
        extraFlatDamage = totalFlatDmg - skillDmg;

        type = damageType;
        trajectory = damageTrajectory;
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

    public float DamageAfterShield(Unit unit)
    {
        return ShouldIgnoreShield  ? -totalFlatDmg : -totalFlatDmg * (1 + (100 - unit.Shield) / 100);
    }

    public float DamageDealt(Unit unit)
    {
        return unit.resistancesManager.CheckAgainstDamage(unit, this)
            * (type == DamageType.Heal ? 1f : -1f);
    }
}