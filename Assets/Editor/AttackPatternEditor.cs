using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackPattern))]
public class AttackPatternEditor : Editor
{
    private AttackPattern pattern = null;

    void OnEnable()
    {
        pattern = (AttackPattern)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Rect headerRect = EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Size: {0}", pattern.surroundingAreaWidth));
        if (GUILayout.Button("Expand")) {
            pattern.expandArea();
            EditorUtility.SetDirty(target);
        };
        if (GUILayout.Button("Detract"))
        {
            pattern.detractArea();
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();


        //EditorGUILayout.EnumFlagsField(pattern.test[0]);

        //EditorGUILayout.BeginHorizontal();
        //for (int row = -pattern.surroundingAreaWidth; row <= pattern.surroundingAreaWidth; row++)
        //{

        //    EditorGUILayout.BeginVertical();
        //    for (int column = -pattern.surroundingAreaWidth; column <= pattern.surroundingAreaWidth; column++)
        //    {
        //        Vector2Int cellPos = new Vector2Int(row, column);

        //        GUI.color = Color.grey;
        //        AttackPatternField field = pattern.fields[cellPos];
        //        if (field == AttackPatternField.Player) GUILayout.Label("Player");
        //        else
        //        {
        //            EditorGUILayout.EnumFlagsField(pattern.fields[cellPos]);
        //            if (field == AttackPatternField.On) GUI.color = Color.green;
        //            if (GUILayout.Button(""))
        //            {
        //                pattern.ToggleField(row, column);
        //                EditorUtility.SetDirty(target);
        //            };
        //        }
        //    }
        //    EditorGUILayout.EndVertical();
        //}
        //EditorGUILayout.EndHorizontal();

        int cellWidth = 20;
        int cellHeight = 20;
        int minCellX = 0;
        int minCellY = 0;
        int maxCellX = 0;
        int maxCellY = 0;

        bool changeMade = false;

        foreach (KeyValuePair<Vector2Int, AttackPatternField> item in pattern.fields)
        {
            if (item.Key.x < minCellX) minCellX = item.Key.x;
            if (item.Key.y < minCellY) minCellY = item.Key.y;
            if (item.Key.x > maxCellX) maxCellX = item.Key.x;
            if (item.Key.y > maxCellY) maxCellY = item.Key.y;
        }

        Vector2Int patternBeginning = new Vector2Int((int)headerRect.x - minCellX * cellWidth, (int)headerRect.y + (int)headerRect.height - minCellY * cellHeight + 10);
        GUILayoutUtility.GetRect((maxCellX - minCellX) * cellWidth, (maxCellY - minCellY) * cellHeight + 30);

        foreach (KeyValuePair<Vector2Int, AttackPatternField> cell in pattern.fields)
        {
            Vector2Int cellPos = cell.Key;
            Rect cellRect = new Rect(patternBeginning.x + cellPos.x * cellWidth, patternBeginning.y + cellPos.y * cellHeight, cellWidth, cellHeight);
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
                    pattern.ToggleField(cellPos);
                    EditorUtility.SetDirty(target);
                    changeMade = true;
                };
            }

            if (changeMade) break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
