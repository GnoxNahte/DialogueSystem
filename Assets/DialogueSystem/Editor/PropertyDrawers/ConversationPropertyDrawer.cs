using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor.Experimental.GraphView;
using UnityEditor;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    [CustomPropertyDrawer(typeof(Conversation))]
    public class ConversationPropertyDrawer : PropertyDrawer
    {
        float spacing = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the dialogueSystemData that is parented to this
            DialogueSystemData dialogueSystemData = (DialogueSystemData)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));

            // Try getting the data another way. Might happen if the serializedObject is a monobehaviour
            if (dialogueSystemData == null)
                dialogueSystemData = (DialogueSystemData)property.serializedObject.FindProperty("data")?.objectReferenceValue;

            if (dialogueSystemData == null)
            {
                //Debug.LogError("Drawing Conversation Property Drawer, Cannot get dialogueSystemData using MainAsset \nDrawing default property");
                EditorGUI.PropertyField(position, property, label);
                return;
            }


            Conversation conversation = (Conversation)property.objectReferenceValue;

            // ========== Ping And Select Asset Button ==========

            float pingButtonSize = position.size.y - 3f;
            Rect pingButtonRect = position;
            pingButtonRect.position = new Vector2(pingButtonRect.position.x, pingButtonRect.position.y + 3f);
            pingButtonRect.size = new Vector2(pingButtonSize + 7f, pingButtonSize);

            Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(Vector2.one * pingButtonSize);

            if (GUI.Button(pingButtonRect, (Texture)EditorGUIUtility.Load("d_SearchJump Icon")) && conversation != null)
            {
                EditorGUIUtility.PingObject(conversation);

                DialogueSystemDataEditorWindow.SelectPart(dialogueSystemData, conversation);
            }

            EditorGUIUtility.SetIconSize(originalIconSize);

            // ========== Search dropdown ==========

            Rect searchPosition = position;
            searchPosition.position = new Vector2(pingButtonRect.xMax + spacing, position.y + 3f);
            searchPosition.size = new Vector2(position.size.x - pingButtonSize - spacing * 3f, position.size.y);

            string buttonLabel;
            if (conversation == null)
                buttonLabel = "null";
            else
                buttonLabel = $"{dialogueSystemData.GetConversationPath(conversation)}";

            if (buttonLabel == string.Empty && dialogueSystemData.GetConversationPath(conversation) == string.Empty)
                buttonLabel = "Cannot find Conversation. Check if the Dialogue System Data is the same";

            if (GUI.Button(searchPosition, buttonLabel, EditorStyles.popup))
            {
                var conversations = dialogueSystemData.GetAllConversations();
                SimpleSearchProvider searchProvider = ScriptableObject.CreateInstance<SimpleSearchProvider>();
                searchProvider.Init(conversations,
                    (path, item) =>
                    {
                        property.objectReferenceValue = (Object)item;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    );

                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position.position)), searchProvider);
            }
        }

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight;
        //}
    }
}