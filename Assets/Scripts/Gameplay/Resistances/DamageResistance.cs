using System.Collections.Generic;
using UnityEngine;

public enum DamageResistanceEffect
{
    CancelDamage,
    ReduceDamage,
    SetDamageToFixedAmount
}

[CreateAssetMenu(fileName = "DamageResistance", menuName = "GDS/Resistances/DamageResistance")]
public class DamageResistance : Resistance
{
    public static Dictionary<DamageResistanceEffect, int> effectPriorities =
        new Dictionary<DamageResistanceEffect, int> {
            { DamageResistanceEffect.CancelDamage, 3 },
            { DamageResistanceEffect.SetDamageToFixedAmount, 2 },
            { DamageResistanceEffect.ReduceDamage, 1 },
        };

    public bool anyDamage = false;
    public bool shouldResistTypes = false;
    public DamageType damageType;
    public bool shouldResistTrajectories = false;
    public DamageTrajectory damageTrajectory;
    public DamageResistanceEffect resistanceEffect;
    public float effectAmount;

    private void Awake()
    {
        type = ResistanceType.Damage;
    }

    public bool WillApplyTo(DamageValue damage)
    {
        if (anyDamage
            || (shouldResistTypes && damageType == damage.type)
            || (shouldResistTrajectories && damageTrajectory == damage.trajectory)
        ) return true;
        return false;
    }

    public float ProcessDamage(DamageValue damage)
    {
        if (resistanceEffect == DamageResistanceEffect.CancelDamage)
            return 0;
        if (resistanceEffect == DamageResistanceEffect.ReduceDamage)
            return damage.totalFlatDmg - effectAmount;
        if (resistanceEffect == DamageResistanceEffect.SetDamageToFixedAmount)
            return effectAmount;

        return damage.totalFlatDmg;
    }
}
