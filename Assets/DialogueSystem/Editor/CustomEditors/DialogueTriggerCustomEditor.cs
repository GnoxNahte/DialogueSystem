using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    [CustomEditor(typeof(DialogueEventTrigger), true)]
    public class DialogueTriggerCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty dataProperty = serializedObject.FindProperty("dialogueSystemData");
            DialogueSystemData data = (DialogueSystemData)dataProperty.objectReferenceValue;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(dataProperty);

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            if (data == null)
            {
                EditorGUILayout.HelpBox("Select a DialogueSystemData object to set the dialogue event", MessageType.Warning);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                
                DrawPropertiesExcluding(serializedObject, "dialogueSystemData", "m_Script");

                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}