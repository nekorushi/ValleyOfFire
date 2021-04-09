using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "IgniteEffect", menuName = "GDS/AttackEffects/IgniteEffect")]
public class IgniteEffect : AttackEffect
{
    public AudioClip tileIgniteSound;

    public override IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile)
    {
        if (targetTile && targetTile.GetType() == typeof(BushTile))
        {
            attackerUnit.PlaySound(tileIgniteSound);
            targetTile.Activate(targetPos);
        }
        yield return new WaitForEndOfFrame();
    }
}