using System;
using System.Collections;
using UnityEngine;

public enum AttackEffectType
{
    None,
    Push,
    Ignite
}

[Serializable]
public class AttackEffect
{
    public AttackEffectType type;

    //Push properties
    public int distance = 1;

    //Ignite properties
    public int duration = 1;
    public int damage = 1;

    public IEnumerator Execute(Vector3Int attackerPos, Vector3Int targetPos, Unit targetUnit) {
        switch (type)
        {
            case AttackEffectType.Push:
                yield return targetUnit.Push(WorldUtils.DirectionToTarget(attackerPos, targetPos), distance);
                break;

            case AttackEffectType.Ignite:
                break;

            case AttackEffectType.None:
                break;
        }
    }
}
