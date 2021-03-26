using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "IgniteEffect", menuName = "GDS/AttackEffects/IgniteEffect", order = 1)]
public class IgniteEffect : AttackEffect
{
    public override IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile)
    {
        if (targetTile && targetTile.GetType() == typeof(BushTile))
        {
            targetTile.Activate(targetPos);
        }
        yield return new WaitForEndOfFrame();
    }
}