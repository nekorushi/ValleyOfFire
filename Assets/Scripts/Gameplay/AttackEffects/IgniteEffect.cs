using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "IgniteEffect", menuName = "GDS/AttackEffects/IgniteEffect", order = 1)]
public class IgniteEffect : AttackEffect
{
    [SerializeField]
    private IgnitedStatus status;

    public override IEnumerator Execute(Vector3Int attackerPos, Vector3Int targetPos, Unit targetUnit)
    {
        IgnitedStatus statusInstance = Instantiate(status);
        targetUnit.AddStatus(statusInstance);
        yield return new WaitForEndOfFrame();
    }
}
