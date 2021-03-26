using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PushEffect", menuName = "GDS/AttackEffects/PushEffect", order = 1)]
public class PushEffect : AttackEffect
{
    [SerializeField]
    private int distance;

    public override IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile)
    {
        yield return targetUnit.Push(WorldUtils.DirectionToTarget(attackerUnit.CellPosition, targetUnit.CellPosition), distance);
    }
}
