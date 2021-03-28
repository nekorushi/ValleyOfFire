
using UnityEngine;

[CreateAssetMenu(fileName = "SwampedStatus", menuName = "GDS/Statuses/SwampedStatus")]
public class SwampedStatus : UnitStatus
{
    public override void OnAdd(Unit afflictedUnit)
    {
        afflictedUnit.barFxAnimator.SetBool("Swamped", true);
    }

    public override bool OnTick(Unit afflictedUnit) {
        return false;
    }

    public override void OnRemove(Unit afflictedUnit)
    {
        afflictedUnit.barFxAnimator.SetBool("Swamped", false);
    }
}
