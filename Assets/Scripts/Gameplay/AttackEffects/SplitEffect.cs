using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SplitEffect", menuName = "GDS/AttackEffects/SplitEffect")]
public class SplitEffect : AttackEffect
{
    [SerializeField]
    private AttackEffect allyAttackEffect;
    [SerializeField]
    private AttackEffect enemyAttackEffect;

    public override IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile)
    {
        bool isAttackingAlly = targetUnit != null && attackerUnit.Player.faction == targetUnit.Player.faction;
        AttackEffect executedEffect = isAttackingAlly ? allyAttackEffect : enemyAttackEffect;

        yield return executedEffect.Execute(attackerUnit, targetUnit, targetPos, targetTile);
    }
}
