using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SkillHandler : MonoBehaviour
{
    private Unit attackerUnit;
    [HideInInspector]
    public SkillConfig config;

    public SerializableDictionary<Vector3Int, AttackPatternField> AttackArea {
        get {
            return config.trajectory == AttackTrajectory.Straight
                ? CalculateStraightRange()
                : MapPatternToLevel();
        }
    }

    private void Awake()
    {
        attackerUnit = GetComponent<Unit>();  
    }

    public IEnumerator ExecuteAttack(Vector3Int targetPos, Unit targetUnit)
    {
        bool shouldAttackTarget = targetUnit != null && targetUnit.Player.faction != attackerUnit.Player.faction && config.damage > 0;
        if (shouldAttackTarget)
        {
            yield return StartCoroutine(ProjectileAnimator.Instance.Play(attackerUnit.CellPosition, targetPos, config.trajectory));
            targetUnit.ModifyHealth(config.damage, DamageConfig.Types.Normal);
        }

        if (config.effect != null)
        {
            yield return StartCoroutine(
                config.effect.Execute(
                    attackerUnit,
                    targetUnit,
                    targetPos,
                    TilemapNavigator.Instance.GetTile(targetPos)
                )
            );
        }
    }

    public bool Contains(Vector3Int cellPos)
    {
        return GetField(cellPos) != null;
    }

    private AttackPatternField? GetField(Vector3Int pos)
    {
        if (AttackArea.ContainsKey(pos)) return AttackArea[pos];
        return null;
    }

    private SerializableDictionary<Vector3Int, AttackPatternField> MapPatternToLevel()
    {
        SerializableDictionary<Vector3Int, AttackPatternField> result = new SerializableDictionary<Vector3Int, AttackPatternField>();
        foreach(KeyValuePair<Vector2Int, AttackPatternField> field in config.pattern)
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
            for (int step = 1; step <= config.straightRange; step++)
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
}