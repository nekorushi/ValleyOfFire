using System.Collections.Generic;
using UnityEngine;

public enum ResistanceType
{
    Status,
    Damage,
}

public enum ResistanceFX
{
    None,
    CurveShield,
    StraightShield
}

public abstract class Resistance : ScriptableObject
{
    private Dictionary<Unit, int> activeTimers = new Dictionary<Unit, int>();

    public ResistanceFX fx;
    public bool isPermanent;
    public int duration;
    public AudioClip tickSound;

    protected ResistanceType type;

    private Dictionary<ResistanceFX, string> fxNames = new Dictionary<ResistanceFX, string>()
    {
        { ResistanceFX.StraightShield, "straightShield" },
        { ResistanceFX.CurveShield, "curveShield" },
    };

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

        SetFX(immuneUnit, fx, true);
    }

    public bool OnTick(Unit immuneUnit)
    {
        if (tickSound != null) immuneUnit.PlaySound(tickSound);
        bool shouldRemoveResistance = false;

        if (!isPermanent)
        {
            if (activeTimers.ContainsKey(immuneUnit))
            {
                activeTimers[immuneUnit]--;
                shouldRemoveResistance = activeTimers[immuneUnit] <= 0;
            } else
            {
                shouldRemoveResistance = true;
            }
        }

        if (shouldRemoveResistance)
        {
            SetFX(immuneUnit, fx, false);
            immuneUnit.fxAnimator.SetTrigger("Dispel");
        }

        return shouldRemoveResistance;
    }

    private void SetFX(Unit unit, ResistanceFX fxType, bool activate)
    {
        if (fxNames.ContainsKey(fxType))
        {
            unit.fxAnimator.SetBool(fxNames[fxType], activate);
        }
        
    }
}
