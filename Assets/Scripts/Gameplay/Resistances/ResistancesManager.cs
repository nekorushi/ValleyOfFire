using System.Collections.Generic;

public class ResistancesManager
{
    private List<Resistance> resistances = new List<Resistance>();

    public ResistancesManager(List<Resistance> initialResistances)
    {
        foreach (Resistance resistance in initialResistances)
        {
            AddImmunity(resistance);
        }
    }

    public void AddImmunity(Resistance resistance)
    {
        if (!resistances.Contains(resistance))
        {
            resistances.Add(resistance);
        }
    }

    public void RemoveImmunity(Resistance resistance)
    {
        if (resistances.Contains(resistance))
        {
            resistances.Remove(resistance);
        }
    }

    public bool TestAgainstStatus(Unit unit, UnitStatus status)
    {
        Resistance resistance = GetImmunity(status);

        if (resistance != null)
        {
            bool shouldRemove = resistance.OnTick(unit);
            if (shouldRemove) RemoveImmunity(resistance);
            return true;
        }

        return false;
    }

    public void TestAgainstDamage()
    {

    }

    private Resistance GetImmunity(UnitStatus status)
    {
        return resistances
            .Find(resistance => resistance.PreventedStatus.GetType() == status.GetType());
    }
}
