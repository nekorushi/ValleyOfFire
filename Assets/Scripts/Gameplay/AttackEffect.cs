using System;
using System.Collections;
using UnityEngine;

[Serializable]
public abstract class AttackEffect : ScriptableObject
{
    public abstract IEnumerator Execute(Vector3Int attackerPos, Vector3Int targetPos, Unit targetUnit, LevelTile targetTile);
}
