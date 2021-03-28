using System;
using System.Collections;
using UnityEngine;

[Serializable]
public abstract class AttackEffect : ScriptableObject
{
    public abstract IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile);
}
