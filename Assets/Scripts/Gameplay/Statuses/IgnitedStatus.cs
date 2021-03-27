
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IgnitedStatus", menuName = "GDS/UnitStatuses/IgnitedStatus", order = 1)]
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
        afflictedUnit.bgFxAnimator.SetBool("Burning", true);
    }

    public override bool OnTick(Unit afflictedUnit)
    {
        bool shouldRemoveStatus = false;
        if (activeStatuses.ContainsKey(afflictedUnit) && activeStatuses[afflictedUnit] > 0)
        {
            afflictedUnit.fxAnimator.SetTrigger("Fire");
            afflictedUnit.ModifyHealth(new DamageValue(_damagePerTick, 1, DamageType.Fire));

            activeStatuses[afflictedUnit]--;
            shouldRemoveStatus = activeStatuses[afflictedUnit] <= 0;
        } else
        {
            shouldRemoveStatus = true;
        }

        if (shouldRemoveStatus) afflictedUnit.bgFxAnimator.SetBool("Burning", false);
        return shouldRemoveStatus;
    }
}
