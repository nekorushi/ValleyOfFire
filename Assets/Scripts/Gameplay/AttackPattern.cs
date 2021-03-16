using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum AttackPatternField
{
    Player,
    On,
    Off
}

[Serializable]
public enum AttackTrajectory
{
    Straight,
    Curve
}

[Serializable]
public class AttackPattern : MonoBehaviour
{
    private Unit unit;

    [SerializeField]
    private float _damage = 2f;
    public float Damage { get { return _damage; } private set { _damage = value; } }

    public AttackEffect effect;

    [HideInInspector]
    public int surroundingAreaWidth = 0;

    [HideInInspector]
    public SerializableDictionary<Vector2Int, AttackPatternField> _pattern =
        new SerializableDictionary<Vector2Int, AttackPatternField>() {
            { Vector2Int.zero, AttackPatternField.Player } 
        };

    public SerializableDictionary<Vector2Int, AttackPatternField> Pattern {
        get { return _pattern; }
    }

    private void Awake()
    {
        unit = GetComponent<Unit>();  
    }

    public IEnumerator ExecuteAttack(Vector3Int targetPos, Unit targetUnit)
    {
        if (Damage > 0) targetUnit.ApplyDamage(Damage);
        if (effect != null) yield return StartCoroutine(effect.Execute(unit.CellPosition, targetPos, targetUnit));
    }

    public bool Contains(Vector3Int cellPos)
    {
        Vector3Int relativeCellPos = cellPos - unit.CellPosition;
        return GetField(new Vector2Int(relativeCellPos.x, relativeCellPos.y)) != null;
    }

    private AttackPatternField? GetField(Vector2Int pos)
    {
        if (Pattern.ContainsKey(pos)) return Pattern[pos];
        return null;
    }

    public void ToggleField(Vector2Int cellPos)
    {
        if (Mathf.Abs(cellPos.x) <= surroundingAreaWidth && Mathf.Abs(cellPos.y) <= surroundingAreaWidth)
        {
            if (!_pattern.ContainsKey(cellPos)) _pattern.Add(cellPos, AttackPatternField.On);
            else
            {
                _pattern[cellPos] = _pattern[cellPos] == AttackPatternField.Off ? AttackPatternField.On : AttackPatternField.Off;
            }
        }
    }

    public void ExpandArea()
    {
        surroundingAreaWidth += 1;
        for (int row = -surroundingAreaWidth; row <= surroundingAreaWidth; row++)
        {
            for (int column = -surroundingAreaWidth; column <= surroundingAreaWidth; column++)
            {
                if (Mathf.Abs(row) == surroundingAreaWidth || Mathf.Abs(column) == surroundingAreaWidth)
                {
                    Vector2Int cellPos = new Vector2Int(row, column);
                    _pattern.Add(cellPos, AttackPatternField.Off);
                }
            }
        }
    }

    public void DetractArea()
    {
        if (surroundingAreaWidth > 0)
        {
            for (int row = -surroundingAreaWidth; row <= surroundingAreaWidth; row++)
            {
                for (int column = -surroundingAreaWidth; column <= surroundingAreaWidth; column++)
                {
                    if (Mathf.Abs(row) == surroundingAreaWidth || Mathf.Abs(column) == surroundingAreaWidth)
                    {
                        Vector2Int cellPos = new Vector2Int(row, column);
                        _pattern.Remove(cellPos);
                    }
                }
            }
            surroundingAreaWidth -= 1;
        }
    }
}
