using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor.Experimental.GraphView;
using UnityEditor;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    [CustomPropertyDrawer(typeof(DialogueEvent))]
    public class DialogueEventPropertyDrawer : PropertyDrawer
    {
        float spacing = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the dialogueSystemData that is parented to this
            DialogueSystemData dialogueSystemData = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(property.serializedObject.targetObject)) as DialogueSystemData;

            // Try getting the data another way. Might happen if the serializedObject is a monobehaviour
            if (dialogueSystemData == null)
                dialogueSystemData = (DialogueSystemData)property.serializedObject.FindProperty("dialogueSystemData").objectReferenceValue;

            if (dialogueSystemData == null)
            {
                Debug.LogError("Drawing DialogueEvent Property Drawer, Cannot get dialogueSystemData using MainAsset \nDrawing default property");
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            DialogueEvent dialogueEvent = (DialogueEvent)property.objectReferenceValue;

            // ========== Ping And Select Asset Button ==========

            float pingButtonSize = position.size.y;
            Rect pingButtonRect = position;
            pingButtonRect.size = new Vector2(pingButtonSize + 7f, pingButtonSize);

            Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(Vector2.one * position.size.y);

            if (GUI.Button(pingButtonRect, (Texture)EditorGUIUtility.Load("d_SearchJump Icon")) && dialogueEvent != null)
            {
                EditorGUIUtility.PingObject(dialogueEvent);

                DialogueSystemDataEditorWindow.SelectPart(dialogueSystemData, dialogueEvent);
            }

            EditorGUIUtility.SetIconSize(originalIconSize);

            // ========== Search dropdown ==========

            Rect searchPosition = position;
            searchPosition.position = new Vector2(pingButtonRect.xMax + spacing, position.y);
            searchPosition.size = new Vector2(position.size.x - pingButtonSize - spacing * 3f, position.size.y);

            string buttonLabel;
            if (dialogueEvent == null)
                buttonLabel = "null";
            else
                buttonLabel = $"{dialogueSystemData.GetDialogueEventPath(dialogueEvent)}";

            if (buttonLabel == string.Empty && dialogueSystemData.GetDialogueEventPath(dialogueEvent) == string.Empty)
                buttonLabel = "Cannot find Dialogue Event. Check if the dialogue System Data is the same";

            if (GUI.Button(searchPosition, buttonLabel, EditorStyles.popup))
            {
                var events = dialogueSystemData.GetAllEvents();
                SimpleSearchProvider searchProvider = ScriptableObject.CreateInstance<SimpleSearchProvider>();
                searchProvider.Init(events,
                    (path, item) =>
                    {
                        property.objectReferenceValue = (Object)item;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    );

                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position.position)), searchProvider);
            }
        }
    }
}