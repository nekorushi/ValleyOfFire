using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UnitsConfig))]
public class UnitsConfigEditor : Editor
{
    UnitsConfig unitsConfig;

    readonly string filePath = "Assets/Scripts/Gameplay/";
    readonly string fileName = "UnitTypes";

    readonly int minMatrixWidth = 100;
    readonly int minMatrixHeight = 100;

    readonly int targetCellWidth = 30;
    readonly int targetCellHeight = 30;

    readonly int axisLabelHeight = 20;
    readonly int unitLabelHeight = 60;

    private void OnEnable()
    {
        unitsConfig = (UnitsConfig)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Save unit types"))
        {
            EdiorMethods.WriteToEnum(filePath, fileName, unitsConfig.unitTypes);
        }

        DrawDamageMatrix();
    }

    private void DrawDamageMatrix()
    {
        string[] typeList = System.Enum.GetNames(typeof(UnitTypes));

        int cellWidth = typeList.Length * targetCellWidth >= minMatrixWidth ? targetCellWidth : minMatrixWidth / typeList.Length;
        int cellHeight = typeList.Length * targetCellHeight >= minMatrixHeight ? targetCellHeight : minMatrixHeight / typeList.Length;

        int matrixWidth = cellWidth * typeList.Length;
        int matrixHeight = cellHeight * typeList.Length;

        GUILayout.Space(10);
        Rect matrixAreaRect = GUILayoutUtility.GetRect(
            matrixWidth + axisLabelHeight + unitLabelHeight,
            matrixHeight + axisLabelHeight + unitLabelHeight
        );

        Rect defenderAxisLabelRect = new Rect(
            matrixAreaRect.x + axisLabelHeight + unitLabelHeight,
            matrixAreaRect.y,
            matrixWidth,
            axisLabelHeight
        );

        Rect attackerAxisLabelRect = new Rect(
            matrixAreaRect.x,
            matrixAreaRect.y + matrixHeight + axisLabelHeight + unitLabelHeight,
            matrixHeight,
            axisLabelHeight
        );
        Vector2 attackerAxisLabelPivot = new Vector2(
            attackerAxisLabelRect.x,
            attackerAxisLabelRect.y
        );

        GUIStyle axisLabelStyle = new GUIStyle();
        axisLabelStyle.alignment = TextAnchor.MiddleCenter;
        axisLabelStyle.normal.textColor = Color.grey;

        GUIStyle attackerLabelStyle = new GUIStyle();
        attackerLabelStyle.alignment = TextAnchor.MiddleRight;
        attackerLabelStyle.normal.textColor = Color.white;
        attackerLabelStyle.padding = new RectOffset(5,5,5,5);

        GUIStyle defenderLabelStyle = new GUIStyle();
        defenderLabelStyle.alignment = TextAnchor.MiddleLeft;
        defenderLabelStyle.normal.textColor = Color.white;
        defenderLabelStyle.padding = new RectOffset(5, 5, 5, 5);

        GUI.Button(defenderAxisLabelRect, "Defender units", axisLabelStyle);
        GUIUtility.RotateAroundPivot(-90, attackerAxisLabelPivot);
        GUI.Button(attackerAxisLabelRect, "Attacker units", axisLabelStyle);
        GUIUtility.RotateAroundPivot(90, attackerAxisLabelPivot);

        Vector2 matrixAnchor = new Vector2(
            matrixAreaRect.x + axisLabelHeight + unitLabelHeight,
            matrixAreaRect.y + axisLabelHeight + unitLabelHeight
        );

        for (int row = 0; row < typeList.Length; row++)
        {
            UnitTypes unitType = (UnitTypes)row;

            Rect attackerLabelRect = new Rect(
                matrixAreaRect.x + axisLabelHeight,
                matrixAreaRect.y + axisLabelHeight + unitLabelHeight + row * cellHeight,
                unitLabelHeight,
                cellHeight
            );

            GUI.Label(attackerLabelRect, unitType.ToString(), attackerLabelStyle);

            Rect defenderLabelRect = new Rect(
                matrixAreaRect.x + axisLabelHeight + unitLabelHeight + row * cellWidth,
                matrixAreaRect.y + axisLabelHeight + unitLabelHeight,
                unitLabelHeight,
                cellHeight
            );
            Vector2 defenderLabelPivot = new Vector2(
                defenderLabelRect.x,
                defenderLabelRect.y
            );

            GUIUtility.RotateAroundPivot(-90, defenderLabelPivot);
            GUI.Label(defenderLabelRect, unitType.ToString(), defenderLabelStyle);
            GUIUtility.RotateAroundPivot(90, defenderLabelPivot);

            for (int column = 0; column < typeList.Length; column++)
            {
                Rect cellRect = new Rect(
                    matrixAnchor.x + column * cellWidth,
                    matrixAnchor.y + row * cellHeight,
                    cellWidth,
                    cellHeight
                );

                UnitTypes attackerType = (UnitTypes)row;
                UnitTypes defenderType = (UnitTypes)column;

                GUI.color = row == column ? new Color(.8f, .8f, .8f) : new Color(1f, 1f, 1f);
                EditorGUI.BeginChangeCheck();
                float cellValue = EditorGUI.FloatField(cellRect, unitsConfig.GetExtraDamage(attackerType, defenderType));
                if (EditorGUI.EndChangeCheck())
                {
                    unitsConfig.SetExtraDamage(attackerType, defenderType, cellValue);
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}