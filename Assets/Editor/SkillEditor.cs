using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Skill))]
public class SkillEditor : Editor
{
    private Skill pattern = null;

    void OnEnable()
    {
        pattern = (Skill)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();

        if (pattern.attackTrajectory == AttackTrajectory.Straight)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("attackRange"));
        } else if (pattern.attackTrajectory == AttackTrajectory.Curve)
        {
            GUILayout.BeginVertical("HelpBox");

            GUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("Pattern shape");
            GUILayout.EndVertical();
            EditorGUILayout.Space(10);

            Rect headerRect = EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("Size: {0}", pattern.patternSize));
            if (GUILayout.Button("Expand"))
            {
                pattern.ExpandArea();
                EditorUtility.SetDirty(target);
            };
            if (GUILayout.Button("Detract"))
            {
                pattern.DetractArea();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            int cellWidth = 20;
            int cellHeight = 20;
            int minCellX = 0;
            int minCellY = 0;
            int maxCellX = 0;
            int maxCellY = 0;

            bool changeMade = false;

            foreach (KeyValuePair<Vector2Int, AttackPatternField> item in pattern._pattern)
            {
                if (item.Key.x < minCellX) minCellX = item.Key.x;
                if (item.Key.y < minCellY) minCellY = item.Key.y;
                if (item.Key.x > maxCellX) maxCellX = item.Key.x;
                if (item.Key.y > maxCellY) maxCellY = item.Key.y;
            }

            Vector2Int patternBeginning = new Vector2Int(
                (int)headerRect.x - minCellX * cellWidth,
                (int)headerRect.y + (int)headerRect.height - minCellY * cellHeight + 10
            );

            int boardWidth = maxCellX - minCellX;
            int boardHeight = maxCellY - minCellY;

            GUILayoutUtility.GetRect(
                boardWidth * cellWidth,
                boardHeight * cellHeight + 30
            );

            foreach (KeyValuePair<Vector2Int, AttackPatternField> cell in pattern._pattern)
            {
                Vector2Int cellPos = cell.Key;

                Rect cellRect = new Rect(
                    patternBeginning.x + cellPos.x * cellWidth,
                    patternBeginning.y - cellPos.y * cellHeight,
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
                        pattern.ToggleField(cellPos);
                        EditorUtility.SetDirty(target);
                        changeMade = true;
                    };
                }

                if (changeMade) break;
            }
            GUI.color = Color.white;

            EditorGUILayout.Space(10);
            GUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
