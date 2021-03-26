
using UnityEngine;

[CreateAssetMenu(fileName = "IgnitedStatus", menuName = "GDS/UnitStatuses/IgnitedStatus", order = 1)]
public class IgnitedStatus : UnitStatus
{
    private int _durationLeft = 0;
    [SerializeField] private int _duration = 0;
    [SerializeField] private int _damagePerTick = 0;

    public override void OnAdd(Unit afflictedUnit)
    {
        _durationLeft = _duration;
        afflictedUnit.fxAnimator.SetTrigger("Ignite");
    }

    public override bool OnTick(Unit afflictedUnit)
    {
        afflictedUnit.fxAnimator.SetTrigger("Fire");
        afflictedUnit.ModifyHealth(_damagePerTick, DamageConfig.Types.Fire);
        _durationLeft--;
        return _durationLeft <= 0;
    }
}
