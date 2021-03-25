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
public enum AttackTrajectory
{
    Straight,
    Curve
}

[Serializable]
public class SkillConfig
{
    [SerializeField]
    public float damage = 2f;

    public AttackEffect effect;
    public AttackTrajectory trajectory = AttackTrajectory.Straight;

    [HideInInspector]
    public int straightRange = 0;

    [HideInInspector]
    public int curveRange = 0;

    [HideInInspector]
    public SerializableDictionary<Vector2Int, AttackPatternField> pattern =
        new SerializableDictionary<Vector2Int, AttackPatternField>() {
            { Vector2Int.zero, AttackPatternField.Player }
        };

    public BoundsInt patternBounds
    {
        get {
            BoundsInt result = new BoundsInt(0, 0, 0, 0, 0, 0);

            foreach (KeyValuePair<Vector2Int, AttackPatternField> field in pattern)
            {
                Vector2Int pos = field.Key;
                if (pos.x < result.xMin) result.xMin = pos.x;
                if (pos.x > result.xMax) result.xMax = pos.x;

                if (pos.y < result.yMin) result.yMin = pos.y;
                if (pos.y > result.yMax) result.yMax = pos.y;
            }

            return result;

        }
    }


    //public void ToggleField(Vector2Int cellPos)
    //{
    //    if (Mathf.Abs(cellPos.x) <= curveRange && Mathf.Abs(cellPos.y) <= curveRange)
    //    {
    //        if (!pattern.ContainsKey(cellPos)) pattern.Add(cellPos, AttackPatternField.On);
    //        else
    //        {
    //            pattern[cellPos] = pattern[cellPos] == AttackPatternField.Off ? AttackPatternField.On : AttackPatternField.Off;
    //        }
    //    }
    //}

    //public void ExpandArea()
    //{
    //    curveRange += 1;
    //    for (int row = -curveRange; row <= curveRange; row++)
    //    {
    //        for (int column = -curveRange; column <= curveRange; column++)
    //        {
    //            if (Mathf.Abs(row) == curveRange || Mathf.Abs(column) == curveRange)
    //            {
    //                Vector2Int cellPos = new Vector2Int(row, column);
    //                pattern.Add(cellPos, AttackPatternField.Off);
    //            }
    //        }
    //    }
    //}

    //public void DetractArea()
    //{
    //    if (curveRange > 0)
    //    {
    //        for (int row = -curveRange; row <= curveRange; row++)
    //        {
    //            for (int column = -curveRange; column <= curveRange; column++)
    //            {
    //                if (Mathf.Abs(row) == curveRange || Mathf.Abs(column) == curveRange)
    //                {
    //                    Vector2Int cellPos = new Vector2Int(row, column);
    //                    pattern.Remove(cellPos);
    //                }
    //            }
    //        }
    //        curveRange -= 1;
    //    }
    //}
}
