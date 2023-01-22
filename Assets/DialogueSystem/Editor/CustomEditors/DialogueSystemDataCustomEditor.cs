using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.Callbacks;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    public class DialogueAssetHandler
    {
        [OnOpenAsset()]
        public static bool OpenEditor(int instanceId)
        {
            DialogueSystemData dialogueSystemData = EditorUtility.InstanceIDToObject(instanceId) as DialogueSystemData;
            if (dialogueSystemData != null)
            {
                DialogueSystemDataEditorWindow.OpenEditor(dialogueSystemData);
                return true;
            }

            dialogueSystemData = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(instanceId)) as DialogueSystemData;
            if (dialogueSystemData == null)
                return false;

            DialogueSystemDataPart part = EditorUtility.InstanceIDToObject(instanceId) as DialogueSystemDataPart;
            if (part != null)
            {
                var window = DialogueSystemDataEditorWindow.OpenEditor(dialogueSystemData);
                window.SelectPart(part);
                return true;
            }

            return false;
        }
    }

    [CustomEditor(typeof(DialogueSystemData))]
    public class DialogueSystemDataCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor"))
            {
                DialogueSystemDataEditorWindow.OpenEditor((DialogueSystemData)target);
            }
            if (GUILayout.Button("Save Assets"))
            {
                ((DialogueSystemData)target).VerifyData();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button("Verify Data"))
            {
                ((DialogueSystemData)target).VerifyData();
            }

            if (GUILayout.Button("Reset Play Data"))
            {
                ((DialogueSystemData)target).ResetPlayData();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }

            DrawDefaultInspector();
        }
    }

    [CustomPropertyDrawer(typeof(DialogueSystemData))]
    public class DialogueSystemDataCustomPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.boxedValue != null)
                position.size = new Vector2(position.size.x, position.size.y / 2f);
            EditorGUI.PropertyField(position, property);

            position.position = new Vector2(position.position.x, position.position.y + position.size.y);
            if (property.boxedValue != null && GUI.Button(position, "Open Editor"))
            {
                DialogueSystemDataEditorWindow.OpenEditor((DialogueSystemData)property.boxedValue);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) * (property.boxedValue == null ? 1f : 2f);
        }
    }
}