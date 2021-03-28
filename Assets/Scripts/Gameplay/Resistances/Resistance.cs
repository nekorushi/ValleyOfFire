using System.Collections.Generic;
using UnityEngine;

public enum ResistanceType
{
    Status,
    Damage,
}

public abstract class Resistance : ScriptableObject
{
    private Dictionary<Unit, int> activeTimers = new Dictionary<Unit, int>();

    public bool isPermanent;
    public int duration;

    protected ResistanceType type;

    public void OnAdd(Unit immuneUnit)
    {
        if (!isPermanent)
        {
            if (activeTimers.ContainsKey(immuneUnit))
            {
                activeTimers[immuneUnit] = duration;
            } else
            {
                activeTimers.Add(immuneUnit, duration);
            }
        }
    }

    public bool OnTick(Unit immuneUnit)
    {
        if (!isPermanent)
        {
            if (activeTimers.ContainsKey(immuneUnit))
            {
                activeTimers[immuneUnit]--;
                return activeTimers[immuneUnit] <= 0;
            } else
            {
                return true;
            }
        }

        return false;
    }
}
