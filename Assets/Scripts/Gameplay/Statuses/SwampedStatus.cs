
using UnityEngine;

[CreateAssetMenu(fileName = "SwampedStatus", menuName = "GDS/Statuses/SwampedStatus")]
public class SwampedStatus : UnitStatus
{
    public override void OnAdd(Unit afflictedUnit)
    {
        afflictedUnit.fxAnimator.SetTrigger("Swamp");
    }

    public override bool OnTick(Unit afflictedUnit) {
        return false;
    }
}
