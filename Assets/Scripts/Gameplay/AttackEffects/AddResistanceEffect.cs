using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AddResistanceEffect", menuName = "GDS/AttackEffects/AddResistanceEffect")]
public class AddResistanceEffect : AttackEffect
{
    [SerializeField]
    private Resistance resistance;

    public override IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile)
    {
        targetUnit.resistancesManager.AddResistance(resistance);
        yield return new WaitForEndOfFrame();
    }
}
