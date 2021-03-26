using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SplitEffect", menuName = "GDS/AttackEffects/SplitEffect", order = 2)]
public class SplitEffect : AttackEffect
{
    [SerializeField]
    private AttackEffect allyAttackEffect;
    [SerializeField]
    private AttackEffect enemyAttackEffect;

    public override IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile)
    {
        bool isAttackingAlly = attackerUnit.Player.faction == targetUnit.Player.faction;
        AttackEffect executedEffect = isAttackingAlly ? allyAttackEffect : enemyAttackEffect;

        yield return executedEffect.Execute(attackerUnit, targetUnit, targetPos, targetTile);
    }
}
