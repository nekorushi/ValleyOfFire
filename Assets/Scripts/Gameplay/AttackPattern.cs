using System.Collections.Generic;
using UnityEngine;

public enum AttackPatternField
{
    Player,
    On,
    Off
}


public class AttackPattern : MonoBehaviour {
    public Dictionary<int, Dictionary<int, AttackPatternField>> fields
        = new Dictionary<int, Dictionary<int, AttackPatternField>>() { { 0, new Dictionary<int, AttackPatternField>() { { 0, AttackPatternField.Player } } } };
    public int surroundingAreaWidth = 0;

    public void ToggleField(int row, int column)
    {
        if (Mathf.Abs(row) <= surroundingAreaWidth && Mathf.Abs(column) <= surroundingAreaWidth)
        {
            if (!fields.ContainsKey(row)) fields.Add(row, new Dictionary<int, AttackPatternField>());
            if (!fields[row].ContainsKey(column)) fields[row].Add(column, AttackPatternField.On);
            else
            {
                fields[row][column] = fields[row][column] == AttackPatternField.Off ? AttackPatternField.On : AttackPatternField.Off;
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
                    if (!fields.ContainsKey(row)) fields.Add(row, new Dictionary<int, AttackPatternField>());
                    fields[row].Add(column, AttackPatternField.Off);
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
                        fields[row].Remove(column);
                    }
                }
                if (fields[row].Count == 0) fields.Remove(row);
            }
            surroundingAreaWidth -= 1;
        }
    }
}
