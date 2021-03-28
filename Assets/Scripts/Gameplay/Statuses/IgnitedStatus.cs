
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IgnitedStatus", menuName = "GDS/Statuses/IgnitedStatus")]
public class IgnitedStatus : UnitStatus
{
    private Dictionary<Unit, int> activeStatuses = new Dictionary<Unit, int>();

    [SerializeField] private int _duration = 0;
    [SerializeField] private int _damagePerTick = 0;

    public override void OnAdd(Unit afflictedUnit)
    {
        if (activeStatuses.ContainsKey(afflictedUnit))
        {
            activeStatuses[afflictedUnit] = _duration;
        } else
        {
            activeStatuses.Add(afflictedUnit, _duration);
        }
        afflictedUnit.fxAnimator.SetTrigger("Ignite");
        afflictedUnit.barFxAnimator.SetBool("Burning", true);
    }

    public override bool OnTick(Unit afflictedUnit)
    {
        bool shouldRemoveStatus;
        if (activeStatuses.ContainsKey(afflictedUnit) && activeStatuses[afflictedUnit] > 0)
        {
            afflictedUnit.fxAnimator.SetTrigger("Fire");
            afflictedUnit.ModifyHealth(new DamageValue(_damagePerTick, 1, DamageType.Fire, DamageTrajectory.SelfInflicted));

            activeStatuses[afflictedUnit]--;
            shouldRemoveStatus = activeStatuses[afflictedUnit] <= 0;
        } else
        {
            shouldRemoveStatus = true;
        }
        
        return shouldRemoveStatus;
    }

    public override void OnRemove(Unit afflictedUnit)
    {
        afflictedUnit.barFxAnimator.SetBool("Burning", false);
    }
}
