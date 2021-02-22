using System.Collections.Generic;
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

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Size: {0}", pattern.surroundingAreaWidth));
        if (GUILayout.Button("Expand")) pattern.expandArea();
        if (GUILayout.Button("Detract")) pattern.detractArea();
        EditorGUILayout.EndHorizontal();

        GUIStyle gridStyle = new GUIStyle();
        EditorGUILayout.BeginHorizontal();
        for (int row = -pattern.surroundingAreaWidth; row <= pattern.surroundingAreaWidth; row++)
        {

            EditorGUILayout.BeginVertical();
            for (int column = -pattern.surroundingAreaWidth; column <= pattern.surroundingAreaWidth; column++)
            {
                GUI.color = Color.grey;
                AttackPatternField field = pattern.fields[row][column];
                if (field == AttackPatternField.Player) GUILayout.Label("Player");
                else
                {
                    if (field == AttackPatternField.On) GUI.color = Color.green;
                    if (GUILayout.Button("")) {
                        pattern.ToggleField(row, column);
                    };
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }
}
