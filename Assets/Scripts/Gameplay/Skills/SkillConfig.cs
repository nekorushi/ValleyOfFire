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
public enum AttackTargets
{
    Allies,
    Enemies,
    Both,
    SameClassAlly,
    EnemiesOrSameClassAlly
}

[Serializable]
public class SkillConfig
{
    public bool isActive = true;
    public float damage = 2f;
    public AttackTargets targets = AttackTargets.Enemies;
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
}
