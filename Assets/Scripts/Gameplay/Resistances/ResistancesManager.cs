using System.Collections.Generic;

public class ResistancesManager
{
    private List<Resistance> resistances = new List<Resistance>();

    public ResistancesManager(List<Resistance> initialResistances)
    {
        foreach (Resistance resistance in initialResistances)
        {
            AddResistance(resistance);
        }
    }

    public void AddResistance(Resistance resistance)
    {
        if (resistance != null && !resistances.Contains(resistance))
        {
            resistances.Add(resistance);
        }
    }

    private void RemoveResistance(Resistance resistance)
    {
        if (resistances.Contains(resistance))
        {
            resistances.Remove(resistance);
        }
    }

    public bool CheckAgainstStatus(Unit unit, UnitStatus status)
    {
        StatusResistance resistance = GetResistance(status);

        if (resistance != null)
        {
            bool shouldRemove = resistance.OnTick(unit);
            if (shouldRemove) RemoveResistance(resistance);
            return true;
        }

        return false;
    }

    public float CheckAgainstDamage(Unit unit, DamageValue damage)
    {
        DamageResistance resistance = GetResistance(damage);

        if (resistance != null)
        {
            bool shouldRemove = resistance.OnTick(unit);
            if (shouldRemove) RemoveResistance(resistance);
            return resistance.ProcessDamage(damage);
        }

        return damage.DamageAfterShield(unit);
    }

    private StatusResistance GetResistance(UnitStatus status)
    {
        return resistances
            .Find(resistance => {
                if (resistance.GetType() == typeof(StatusResistance))
                {
                    StatusResistance foundResistance = resistance as StatusResistance;
                    return foundResistance.PreventedStatus.GetType() == status.GetType();
                }
                else return false;

            }) as StatusResistance;
    }

    private DamageResistance GetResistance(DamageValue damage)
    {
        List<Resistance> matchingResistances = resistances
            .FindAll(resistance =>
            {
                if (resistance.GetType() == typeof(DamageResistance))
                {
                    DamageResistance foundResistance = resistance as DamageResistance;
                    return foundResistance.WillApplyTo(damage);
                }
                return false;
            });

        DamageResistance chosenResistance = null;
        foreach(DamageResistance resistance in matchingResistances)
        {
            Dictionary<DamageResistanceEffect, int> priorities = DamageResistance.effectPriorities;

            if (chosenResistance == null
                || priorities[resistance.resistanceEffect] > priorities[chosenResistance.resistanceEffect]
            ) {
                chosenResistance = resistance;
            }
        }

        return chosenResistance;
    }
}
