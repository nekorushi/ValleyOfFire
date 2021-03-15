using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AttackEffect))]
public class PushEffectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

        GUILayout.BeginVertical("HelpBox");

        GUILayout.BeginVertical("HelpBox");
        EditorGUILayout.LabelField("Attack effect");
        GUILayout.EndVertical();
        EditorGUILayout.Space(10);

        SerializedProperty effectType = property.FindPropertyRelative("type");
        EditorGUILayout.PropertyField(effectType);

        if (effectType.intValue == (int)AttackEffectType.Push)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("distance"), new GUIContent("Distance"));
        }

        if (effectType.intValue == (int)AttackEffectType.Ignite)
        {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("duration"), new GUIContent("Duration (in turns)"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("damage"), new GUIContent("Damage (per tick)"));
        }

        EditorGUILayout.Space(10);
        GUILayout.EndVertical();
    }
}
