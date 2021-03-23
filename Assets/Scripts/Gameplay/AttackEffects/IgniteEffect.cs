using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "IgniteEffect", menuName = "GDS/AttackEffects/IgniteEffect", order = 1)]
public class IgniteEffect : AttackEffect
{
    public override IEnumerator Execute(Vector3Int attackerPos, Vector3Int targetPos, Unit targetUnit, LevelTile targetTile)
    {
        if (targetTile && targetTile.GetType() == typeof(BushTile))
        {
            targetTile.Activate(targetPos);
        }
        yield return new WaitForEndOfFrame();
    }
}
