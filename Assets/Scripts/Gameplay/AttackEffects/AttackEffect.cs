using System;
using System.Collections;
using UnityEngine;

[Serializable]
public abstract class AttackEffect : ScriptableObject
{
    public Sprite icon;
    public string label;
    public string description;

    [SerializeField] private AudioClip sound;
    public virtual AudioClip GetSound(Unit attackerUnit, Unit targetUnit)
    {
        return sound;
    }

    public abstract IEnumerator Execute(Unit attackerUnit, Unit targetUnit, Vector3Int targetPos, LevelTile targetTile);
}
