using System;
using UnityEngine;

[Serializable]
public enum AttackPatternField
{
    Player,
    On,
    Off
}


[Serializable]
public abstract class AreaPattern : MonoBehaviour
{
    [HideInInspector]
    public int surroundingAreaWidth = 0;

    [HideInInspector]
    public SerializableDictionary<Vector2Int, AttackPatternField> fields =
        new SerializableDictionary<Vector2Int, AttackPatternField>() {
            { Vector2Int.zero, AttackPatternField.Player } 
        };

    public void ToggleField(Vector2Int cellPos)
    {
        if (Mathf.Abs(cellPos.x) <= surroundingAreaWidth && Mathf.Abs(cellPos.y) <= surroundingAreaWidth)
        {
            if (!fields.ContainsKey(cellPos)) fields.Add(cellPos, AttackPatternField.On);
            else
            {
                fields[cellPos] = fields[cellPos] == AttackPatternField.Off ? AttackPatternField.On : AttackPatternField.Off;
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
                    fields.Add(cellPos, AttackPatternField.Off);
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
                        fields.Remove(cellPos);
                    }
                }
            }
            surroundingAreaWidth -= 1;
        }
    }
}
