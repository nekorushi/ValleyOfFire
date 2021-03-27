using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HealEffect", menuName = "GDS/AttackEffects/HealEffect", order = 2)]
public class HealEffect : AttackEffect
{
    [SerializeField] 
    private float healAmount;

    public override IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile)
    {
        targetUnit.ModifyHealth(new DamageValue(healAmount, 1, DamageType.Heal));

        yield return new WaitForEndOfFrame();
    }
}
