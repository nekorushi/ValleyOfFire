using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DamageResistance))]
[CanEditMultipleObjects]
public class DamageResistanceEditor : Editor
{
    SerializedProperty isPermanent;
    SerializedProperty duration;

    SerializedProperty fx;
    SerializedProperty tickSound;
    SerializedProperty anyDamage;
    SerializedProperty shouldResistTypes;
    SerializedProperty damageType;
    SerializedProperty shouldResistTrajectories;
    SerializedProperty damageTrajectory;
    SerializedProperty resistanceEffect;
    SerializedProperty effectAmount;

    void OnEnable()
    {
        isPermanent = serializedObject.FindProperty("isPermanent");
        duration = serializedObject.FindProperty("duration");

        fx = serializedObject.FindProperty("fx");
        tickSound = serializedObject.FindProperty("tickSound");
        anyDamage = serializedObject.FindProperty("anyDamage");
        shouldResistTypes = serializedObject.FindProperty("shouldResistTypes");
        damageType = serializedObject.FindProperty("damageType");
        shouldResistTrajectories = serializedObject.FindProperty("shouldResistTrajectories");
        damageTrajectory = serializedObject.FindProperty("damageTrajectory");
        resistanceEffect = serializedObject.FindProperty("resistanceEffect");
        effectAmount = serializedObject.FindProperty("effectAmount");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("What FX should be played when resistance is active?");
        EditorGUILayout.PropertyField(fx);

        EditorGUILayout.LabelField("What sound should be played when resistance takes effect?");
        EditorGUILayout.PropertyField(tickSound);

        EditorGUILayout.LabelField("Resistance duration?");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(isPermanent);
        if (!isPermanent.boolValue)
        {
            EditorGUILayout.PropertyField(duration, new GUIContent("Uses"));
        }
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("What kind of damage should be resisted?");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(anyDamage);
        if (!anyDamage.boolValue)
        {
            EditorGUILayout.PropertyField(shouldResistTypes);
            if (shouldResistTypes.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(damageType);
                EditorGUI.indentLevel--;
            }
    
            EditorGUILayout.PropertyField(shouldResistTrajectories);
            if (shouldResistTrajectories.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(damageTrajectory);
                EditorGUI.indentLevel--;
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("What is the outcome of the resistance?");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(resistanceEffect);
        if (resistanceEffect.enumValueIndex != (int)DamageResistanceEffect.CancelDamage)
        {
            Dictionary<DamageResistanceEffect, string> labels = new Dictionary<DamageResistanceEffect, string>() {
                {DamageResistanceEffect.ReduceDamage, "Reduced damage"},
                {DamageResistanceEffect.SetDamageToFixedAmount, "Fixed amount"}
            };

            EditorGUILayout.PropertyField(
                effectAmount,
                new GUIContent(labels[(DamageResistanceEffect)resistanceEffect.enumValueIndex])
            );
        }
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }
}
