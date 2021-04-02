using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "GDS/AttackEffects/HealEffect")]
public class HealEffect : AttackEffect
{
    [SerializeField] 
    private float healAmount;

    public override IEnumerator Execute(
        Unit attackerUnit,
        Unit targetUnit,
        Vector3Int targetPos,
        LevelTile targetTile
    ) {
        if (targetUnit != null)
        {
            targetUnit.ModifyHealth(new DamageValue(healAmount, 1, DamageType.Heal, DamageTrajectory.SelfInflicted));
        }

        yield return new WaitForEndOfFrame();
    }
}
