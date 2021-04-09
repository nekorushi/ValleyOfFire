using System;
using UnityEngine;

public class StatusManager
{
    public event Action StatusChanged;

    private Unit owner;
    private ResistancesManager resistancesManager;

    private UnitStatus _inflictedStatus;
    public UnitStatus InflictedStatus
    {
        get { return _inflictedStatus; }
        private set {
            _inflictedStatus = value;
            StatusChanged.Invoke();
        }
    }

    public StatusManager(Unit unit, ResistancesManager manager)
    {
        owner = unit;
        resistancesManager = manager;
    }

    public void ApplyStatus()
    {
        if (InflictedStatus != null)
        {
            AudioClip sound = InflictedStatus.tickSound;
            if (sound != null) owner.PlaySound(sound);

            bool shouldRemoveStatus = InflictedStatus.OnTick(owner);
            if (shouldRemoveStatus) RemoveStatus();
        }
    }

    public void InflictStatus(UnitStatus newStatus)
    {
        bool wasBlockedByResistance = resistancesManager.CheckAgainstStatus(owner, newStatus);
        if (wasBlockedByResistance)
        {
            AudioClip sound = newStatus.deflectSound;
            if (sound != null) owner.PlaySound(sound);
        } else {
            RemoveStatus();
            InflictedStatus = newStatus;
            newStatus.OnAdd(owner);

            AudioClip sound = newStatus.inflictSound;
            if (sound != null) owner.PlaySound(sound);
        }
    }

    public void RemoveStatus()
    {
        if (InflictedStatus != null)
        {
            InflictedStatus.OnRemove(owner);
            InflictedStatus = null;
        }
    }

    public bool HasStatus(System.Type statusType)
    {
        return InflictedStatus != null && InflictedStatus.GetType() == statusType;
    }

}
