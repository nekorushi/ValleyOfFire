﻿using UnityEngine;
using UnityEditor;
using System;
using UnityObject = UnityEngine.Object;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(SkillConfig))]
public class SkillConfigDrawer : PropertyDrawer
{
    SkillConfig _SkillConfig;

    private readonly float fieldHeight = 17f;
    private float patternHeight = 0;
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        CheckAndInitialize(property);
        if (_SkillConfig.isActive)
            return fieldHeight * 9 + patternHeight;
        return 34f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CheckAndInitialize(property);
        position.height = fieldHeight;

        EditorGUI.LabelField(position, label);
        position.y += fieldHeight;

        EditorGUI.indentLevel++;

        // IsActive field
        bool isActiveVal = _SkillConfig.isActive;
        EditorGUI.BeginChangeCheck();
        bool newIsActiveVal = EditorGUI.Toggle(position, "Is active?", isActiveVal);
        position.y += fieldHeight;
        if (EditorGUI.EndChangeCheck())
        {
            try
            {
                _SkillConfig.isActive = newIsActiveVal;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        if (_SkillConfig.isActive)
        {
            // Damage field
            float damageVal = _SkillConfig.baseDamage;
            EditorGUI.BeginChangeCheck();
            float newDamageVal = EditorGUI.FloatField(position, "Base damage", damageVal);
            position.y += fieldHeight;
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _SkillConfig.baseDamage = newDamageVal;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            // Targets field
            AttackTargets targetsVal = _SkillConfig.targets;
            EditorGUI.BeginChangeCheck();
            AttackTargets newTargetsVal = (AttackTargets)EditorGUI.EnumPopup(position, "Targets", (Enum)(object)targetsVal);
            position.y += fieldHeight;
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _SkillConfig.targets = newTargetsVal;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            // Effect field
            AttackEffect effectVal = _SkillConfig.effect;
            EditorGUI.BeginChangeCheck();
            AttackEffect newEffectVal =
                (AttackEffect)EditorGUI.ObjectField(position, "Effect", (UnityObject)(object)effectVal, typeof(AttackEffect), true);
            position.y += fieldHeight;
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _SkillConfig.effect = newEffectVal;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            // Trajectory field
            DamageTrajectory trajectoryVal = _SkillConfig.trajectory;
            EditorGUI.BeginChangeCheck();
            DamageTrajectory newTrajectoryVal = (DamageTrajectory)EditorGUI.EnumPopup(
                position,
                "Trajectory",
                (Enum)(object)trajectoryVal
            ); ;
            position.y += fieldHeight;
            if (EditorGUI.EndChangeCheck())
            {
                try
                {
                    _SkillConfig.trajectory = newTrajectoryVal;
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }

            if (_SkillConfig.trajectory == DamageTrajectory.Straight)
            {
                // Straight range field
                int straightRangeVal = _SkillConfig.straightRange;
                EditorGUI.BeginChangeCheck();
                int newStraightRangeVal = EditorGUI.IntField(position, "Range", straightRangeVal);
                position.y += fieldHeight;
                if (EditorGUI.EndChangeCheck())
                {
                    try
                    {
                        _SkillConfig.straightRange = newStraightRangeVal;
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                }
            }

            if (_SkillConfig.trajectory == DamageTrajectory.Curve)
            {
                position.y += fieldHeight;
                // Curve range expand
                Rect expandButtonRect = position;
                expandButtonRect.width = position.width / 2;
                if (GUI.Button(expandButtonRect, "Expand"))
                {
                    ExpandArea();
                }

                // Curve range shrink
                Rect shrinkButtonRect = position;
                shrinkButtonRect.width = position.width / 2;
                shrinkButtonRect.x = position.x + expandButtonRect.width;
                if (GUI.Button(shrinkButtonRect, "Shrink"))
                {
                    ShrinkArea();
                }
                position.y += fieldHeight;

                int cellWidth = 20;
                int cellHeight = 20;
                BoundsInt bounds = _SkillConfig.patternBounds;

                Rect patternAreaRect = position;
                patternAreaRect.width = bounds.size.x * cellWidth;
                patternAreaRect.height = bounds.size.y * cellHeight;
                patternHeight = patternAreaRect.height;

                foreach (KeyValuePair<Vector2Int, AttackPatternField> cell in _SkillConfig.pattern)
                {
                    Vector2Int cellPos = cell.Key;

                    Rect cellRect = new Rect(
                        patternAreaRect.x + position.width / 2 + (cellPos.x - .5f) * cellWidth,
                        patternAreaRect.y + 5 - (cellPos.y - bounds.yMax) * cellHeight,
                        cellWidth,
                        cellHeight
                    );
                    GUI.color = Color.gray;

                    if (cell.Value == AttackPatternField.Player)
                    {
                        GUI.color = Color.magenta;
                        GUI.Button(cellRect, "");
                    }
                    else
                    {
                        if (cell.Value == AttackPatternField.On) GUI.color = Color.green;
                        if (GUI.Button(cellRect, ""))
                        {
                            ToggleField(cellPos);
                            break;
                        };
                    }
                }
                GUI.color = Color.white;
            }
        }

        if (GUI.changed)
        {
            var target = property.serializedObject.targetObject;
            EditorUtility.SetDirty(target);
        }
    }

    private void CheckAndInitialize(SerializedProperty property)
    {
        if (_SkillConfig == null)
        {
            var target = property.serializedObject.targetObject;
            _SkillConfig = fieldInfo.GetValue(target) as SkillConfig;

            if (_SkillConfig == null)
            {
                _SkillConfig = new SkillConfig();
                fieldInfo.SetValue(target, _SkillConfig);
            }
        }
    }

    public void ExpandArea()
    {
        BoundsInt bounds = _SkillConfig.patternBounds;

        int xMin = bounds.xMin - 1;
        int yMin = bounds.yMin - 1;

        int xMax = bounds.xMax + 1;
        int yMax = bounds.yMax + 1;

        for (int row = yMin; row <= yMax; row++)
        {
            for (int column = xMin; column <= xMax; column++)
            {
                if (row == yMin || row == yMax || column == xMin || column == xMax)
                {
                    Vector2Int cellPos = new Vector2Int(row, column);
                    _SkillConfig.pattern.Add(cellPos, AttackPatternField.Off);
                }
            }
        }
    }

    public void ShrinkArea()
    {
        BoundsInt bounds = _SkillConfig.patternBounds;

        SerializableDictionary<Vector2Int, AttackPatternField> newPattern
            = new SerializableDictionary<Vector2Int, AttackPatternField>();

        int xMin = bounds.xMin + 1;
        int yMin = bounds.yMin + 1;

        int xMax = bounds.xMax - 1;
        int yMax = bounds.yMax - 1;

        for (int row = yMin; row <= yMax; row++)
        {
            for (int column = xMin; column <= xMax; column++)
            {
                Vector2Int cellPos = new Vector2Int(row, column);
                if (_SkillConfig.pattern.ContainsKey(cellPos)) newPattern.Add(cellPos, _SkillConfig.pattern[cellPos]);
            }
        }

        bool shouldReset = bounds.size.x <= 0 || bounds.size.y <= 0;
        _SkillConfig.pattern = shouldReset
            ? new SerializableDictionary<Vector2Int, AttackPatternField>() { { Vector2Int.zero, AttackPatternField.Player } }
            : newPattern;
    }

    public void ToggleField(Vector2Int cellPos)
    {
        BoundsInt bounds = _SkillConfig.patternBounds;
        bool isInBounds =
            cellPos.x >= bounds.xMin
            && cellPos.x <= bounds.xMax
            && cellPos.y >= bounds.yMin
            && cellPos.y <= bounds.yMax;

        if (isInBounds)
        {
            if (!_SkillConfig.pattern.ContainsKey(cellPos)) _SkillConfig.pattern.Add(cellPos, AttackPatternField.On);
            else
            {
                _SkillConfig.pattern[cellPos] = _SkillConfig.pattern[cellPos] == AttackPatternField.Off
                    ? AttackPatternField.On
                    : AttackPatternField.Off;
            }
        }
    }
}
