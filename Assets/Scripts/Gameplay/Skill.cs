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
public class Skill : MonoBehaviour
{
    private Unit attackerUnit;

    [SerializeField]
    private float _damage = 2f;
    public float Damage { get { return _damage; } private set { _damage = value; } }

    public AttackEffect effect;

    public AttackTrajectory attackTrajectory = AttackTrajectory.Straight;

    [HideInInspector]
    public int attackRange = 0;

    [HideInInspector]
    public int patternSize = 0;

    [HideInInspector]
    public SerializableDictionary<Vector2Int, AttackPatternField> _pattern =
        new SerializableDictionary<Vector2Int, AttackPatternField>() {
            { Vector2Int.zero, AttackPatternField.Player } 
        };

    public SerializableDictionary<Vector3Int, AttackPatternField> AttackArea {
        get { return attackTrajectory == AttackTrajectory.Straight ? CalculateStraightRange() :  MapPatternToLevel(_pattern); }
    }

    private void Awake()
    {
        attackerUnit = GetComponent<Unit>();  
    }

    public IEnumerator ExecuteAttack(Vector3Int targetPos, Unit targetUnit)
    {
        

        if (targetUnit != null && Damage > 0)
        {
            yield return StartCoroutine(ProjectileAnimator.Instance.Play(attackerUnit.CellPosition, targetPos, attackTrajectory));
            targetUnit.ApplyDamage(Damage, DamageConfig.Types.Normal);
        }
        if (effect != null)
        {
            yield return StartCoroutine(
                effect.Execute(
                    attackerUnit.CellPosition,
                    targetPos,
                    targetUnit,
                    TilemapNavigator.Instance.GetTile(targetPos)
                )
            );
        }
    }

    public bool Contains(Vector3Int cellPos)
    {
        return GetField(cellPos) != null;
    }

    private SerializableDictionary<Vector3Int, AttackPatternField> MapPatternToLevel(SerializableDictionary<Vector2Int, AttackPatternField> pattern)
    {
        SerializableDictionary<Vector3Int, AttackPatternField> result = new SerializableDictionary<Vector3Int, AttackPatternField>();
        foreach(KeyValuePair<Vector2Int, AttackPatternField> field in pattern)
        {
            Vector3Int cellPos = new Vector3Int(field.Key.x, field.Key.y, 0) + attackerUnit.CellPosition;
            result.Add(cellPos, field.Value);
        }

        return result;
    }

    private SerializableDictionary<Vector3Int, AttackPatternField> CalculateStraightRange()
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;

        List <Vector3Int> directions = new List<Vector3Int>()
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,
        };

        SerializableDictionary<Vector3Int, AttackPatternField> resultPattern =
            new SerializableDictionary<Vector3Int, AttackPatternField>() {
                { attackerUnit.CellPosition, AttackPatternField.Player }
            };

        foreach (Vector3Int direction in directions)
        {
            for (int step = 1; step <= attackRange; step++)
            {
                Vector3Int cellPos = attackerUnit.CellPosition + direction * step;

                if (!navigator.IsTileWalkable(cellPos)) break;
                if (navigator.IsTileTaken(cellPos))
                {
                    resultPattern.Add(cellPos, AttackPatternField.On);
                    break;
                }
                resultPattern.Add(cellPos, AttackPatternField.On);
            }
        }

        return resultPattern;
    }

    private AttackPatternField? GetField(Vector3Int pos)
    {
        if (AttackArea.ContainsKey(pos)) return AttackArea[pos];
        return null;
    }

    public void ToggleField(Vector2Int cellPos)
    {
        if (Mathf.Abs(cellPos.x) <= patternSize && Mathf.Abs(cellPos.y) <= patternSize)
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
        patternSize += 1;
        for (int row = -patternSize; row <= patternSize; row++)
        {
            for (int column = -patternSize; column <= patternSize; column++)
            {
                if (Mathf.Abs(row) == patternSize || Mathf.Abs(column) == patternSize)
                {
                    Vector2Int cellPos = new Vector2Int(row, column);
                    _pattern.Add(cellPos, AttackPatternField.Off);
                }
            }
        }
    }

    public void DetractArea()
    {
        if (patternSize > 0)
        {
            for (int row = -patternSize; row <= patternSize; row++)
            {
                for (int column = -patternSize; column <= patternSize; column++)
                {
                    if (Mathf.Abs(row) == patternSize || Mathf.Abs(column) == patternSize)
                    {
                        Vector2Int cellPos = new Vector2Int(row, column);
                        _pattern.Remove(cellPos);
                    }
                }
            }
            patternSize -= 1;
        }
    }
}
