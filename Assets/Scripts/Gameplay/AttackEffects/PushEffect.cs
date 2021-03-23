using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "PushEffect", menuName = "GDS/AttackEffects/PushEffect", order = 1)]
public class PushEffect : AttackEffect
{
    [SerializeField]
    private int distance;

    public override IEnumerator Execute(Vector3Int attackerPos, Vector3Int targetPos, Unit targetUnit, LevelTile targetTile)
    {
        yield return targetUnit.Push(WorldUtils.DirectionToTarget(attackerPos, targetPos), distance);
    }
}
