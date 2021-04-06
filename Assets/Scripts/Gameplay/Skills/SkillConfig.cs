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
public enum AttackTargets
{
    Allies,
    Enemies,
    Both,
    SameClassAlly,
    EnemiesOrSameClassAlly,
    Self
}

[Serializable]
public class SkillConfig
{
    public bool isActive = true;
    public float baseDamage = 2f;
    public DamageTrajectory trajectory = DamageTrajectory.Straight;

    public AttackTargets targets = AttackTargets.Enemies;
    public AttackEffect effect;

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

    public bool CanPerformAttack(Unit attackerUnit, Unit targetUnit, LevelTile targetTile)
    {
        if (!isActive) return false;
        AttackTargets allowedTargets = targets;

        bool canAttackSelf = allowedTargets == AttackTargets.Self;
        bool canAttackAllies = allowedTargets == AttackTargets.Allies;
        bool canAttackEnemies = allowedTargets == AttackTargets.Enemies;
        bool canAttackSameClassAlly = allowedTargets == AttackTargets.SameClassAlly;
        bool canAttackEnemiesAndSameClassAlly = allowedTargets == AttackTargets.EnemiesOrSameClassAlly;

        List<Unit> attackerAllies = attackerUnit.Player.Units;
        bool clickedSelf = targetUnit == attackerUnit;
        bool clickedEnemy = !attackerAllies.Contains(targetUnit);
        bool clickedAlly = !clickedSelf && !clickedEnemy;
        bool clickedSameClassAlly = clickedAlly && targetUnit.unitClass.Type == attackerUnit.unitClass.Type;
        bool clickedEnemyOrSameClassAlly = clickedEnemy || clickedSameClassAlly;

        bool canAttackEnvironment = targetTile != null && targetTile.CanBeAttacked;
        bool canAttackUnit = targetUnit != null
            && (
                allowedTargets == AttackTargets.Both
                || canAttackSelf && clickedSelf
                || canAttackAllies && clickedAlly
                || canAttackEnemies && clickedEnemy
                || canAttackSameClassAlly && clickedSameClassAlly
                || canAttackEnemiesAndSameClassAlly && clickedEnemyOrSameClassAlly
            );

        return canAttackUnit || canAttackEnvironment;
    }
}
