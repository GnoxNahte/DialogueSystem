using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    [CustomEditor(typeof(DialogueAreaCheck), true)]

    public class DialogueAreaCheckCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SerializedProperty dataProperty = serializedObject.FindProperty("data");
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

                DrawPropertiesExcluding(serializedObject, "data", "m_Script");

                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }
        }
    }
}