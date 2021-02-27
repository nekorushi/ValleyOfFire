using System;
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
public enum AttackType
{
    Targeted,
    Area
}

public enum AttackDirection
{
    Up,
    Down,
    Left,
    Right
}

[Serializable]
public class AttackPattern : MonoBehaviour
{
    [HideInInspector]
    public int surroundingAreaWidth = 0;

    [HideInInspector]
    public AttackType attackType = AttackType.Targeted;

    [HideInInspector]
    public AttackDirection direction = AttackDirection.Up;

    [HideInInspector]
    public SerializableDictionary<Vector2Int, AttackPatternField> sourcePattern =
        new SerializableDictionary<Vector2Int, AttackPatternField>() {
            { Vector2Int.zero, AttackPatternField.Player } 
        };

    public SerializableDictionary<Vector2Int, AttackPatternField> Pattern {
        get {
            switch (direction)
            {
                case AttackDirection.Down:
                    return Rotate180(sourcePattern);
                case AttackDirection.Left:
                    return Rotate90L(sourcePattern);
                case AttackDirection.Right:
                    return Rotate90R(sourcePattern);
            }

            return sourcePattern;
        }
    }

    public void ToggleField(Vector2Int cellPos)
    {
        if (Mathf.Abs(cellPos.x) <= surroundingAreaWidth && Mathf.Abs(cellPos.y) <= surroundingAreaWidth)
        {
            if (!sourcePattern.ContainsKey(cellPos)) sourcePattern.Add(cellPos, AttackPatternField.On);
            else
            {
                sourcePattern[cellPos] = sourcePattern[cellPos] == AttackPatternField.Off ? AttackPatternField.On : AttackPatternField.Off;
            }
        }
    }

    public void expandArea()
    {
        surroundingAreaWidth += 1;
        for (int row = -surroundingAreaWidth; row <= surroundingAreaWidth; row++)
        {
            for (int column = -surroundingAreaWidth; column <= surroundingAreaWidth; column++)
            {
                if (Mathf.Abs(row) == surroundingAreaWidth || Mathf.Abs(column) == surroundingAreaWidth)
                {
                    Vector2Int cellPos = new Vector2Int(row, column);
                    sourcePattern.Add(cellPos, AttackPatternField.Off);
                }
            }
        }
    }

    public void detractArea()
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
                        sourcePattern.Remove(cellPos);
                    }
                }
            }
            surroundingAreaWidth -= 1;
        }
    }

    private SerializableDictionary<Vector2Int, AttackPatternField> Transform(SerializableDictionary<Vector2Int, AttackPatternField> pattern, Func<Vector2Int, Vector2Int> newPosFormula)
    {
        SerializableDictionary<Vector2Int, AttackPatternField> result = new SerializableDictionary<Vector2Int, AttackPatternField>();
        foreach (KeyValuePair<Vector2Int, AttackPatternField> field in pattern)
        {
            Vector2Int newPosition = newPosFormula(field.Key);
            result.Add(newPosition, field.Value);
        }

        return result;
    }

    private SerializableDictionary<Vector2Int, AttackPatternField> Transpose(SerializableDictionary<Vector2Int, AttackPatternField> pattern)
    {
        return Transform(pattern, (Vector2Int sourcePos) => {
            return new Vector2Int(sourcePos.y, sourcePos.x);
        });
    }

    private SerializableDictionary<Vector2Int, AttackPatternField> FlipVertically(SerializableDictionary<Vector2Int, AttackPatternField> pattern)
    {
        return Transform(pattern, (Vector2Int sourcePos) => {
            return new Vector2Int(sourcePos.x, -sourcePos.y);
        });
    }

    private SerializableDictionary<Vector2Int, AttackPatternField> FlipHorizontally(SerializableDictionary<Vector2Int, AttackPatternField> pattern)
    {
        return Transform(pattern, (Vector2Int sourcePos) => {
            return new Vector2Int(-sourcePos.x, sourcePos.y);
        });
    }

    private SerializableDictionary<Vector2Int, AttackPatternField> Rotate180(SerializableDictionary<Vector2Int, AttackPatternField> pattern)
    {
        return Transform(pattern, (Vector2Int sourcePos) => {
            return new Vector2Int(-sourcePos.x, -sourcePos.y);
        });
    }

    private SerializableDictionary<Vector2Int, AttackPatternField> Rotate90R(SerializableDictionary<Vector2Int, AttackPatternField> pattern)
    {
        return FlipHorizontally(Transpose(pattern));
    }

    private SerializableDictionary<Vector2Int, AttackPatternField> Rotate90L(SerializableDictionary<Vector2Int, AttackPatternField> pattern)
    {
        return FlipVertically(Transpose(pattern));
    }
}
