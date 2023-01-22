using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    [CustomPropertyDrawer(typeof(Condition))]
    public class DialogueConditionPropertyDrawer : PropertyDrawer
    {
        float typeSize = 40f;
        float valueToCheckSize = 50f;

        float spacing = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the dialogueSystemData that is parented to this
            DialogueSystemData dialogueSystemData = (DialogueSystemData)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));
            if (dialogueSystemData == null)
            {
                Debug.LogError("Drawing Condition Property Drawer, Cannot get dialogueSystemData using MainAsset \nDrawing default property");
                EditorGUI.PropertyField(position, property, label);
                return;
            }
            
            property.serializedObject.Update();
            
            Condition condition = (Condition)property.boxedValue;

            // ========== Ping And Select Asset Button ==========

            float pingButtonSize = position.size.y;
            Rect pingButtonRect = position;
            pingButtonRect.size = new Vector2(pingButtonSize + 7f, pingButtonSize);

            Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(Vector2.one * position.size.y);

            if (GUI.Button(pingButtonRect, (Texture)EditorGUIUtility.Load("d_SearchJump Icon")) && condition.fact != null)
            {
                EditorGUIUtility.PingObject(condition.fact);

                DialogueSystemDataEditorWindow.SelectPart(dialogueSystemData, condition.fact);
            }

            EditorGUIUtility.SetIconSize(originalIconSize);

            // ========== Fact Search dropdown ==========

            Rect factRect = position;
            // Fact property takes up all the space that type and valueToCheck aren't using
            factRect.position = new Vector2(pingButtonRect.xMax + spacing, position.y);
            factRect.size = new Vector2(position.size.x - pingButtonSize - typeSize - valueToCheckSize - spacing * 4f, position.size.y);

            string buttonLabel;
            if (condition.fact == null)
                buttonLabel = "null";
            else
                buttonLabel = $"{dialogueSystemData.GetFactPath(condition.fact)} [{condition.fact.value}]";

            if (buttonLabel == string.Empty && dialogueSystemData.GetFactPath(condition.fact) == string.Empty)
                buttonLabel = "Cannot find Fact. Check if the Dialogue System Data is the same";

            if (GUI.Button(factRect, buttonLabel, EditorStyles.popup))
            {
                var facts = dialogueSystemData.GetAllFacts();

                SimpleSearchProvider searchProvider = ScriptableObject.CreateInstance<SimpleSearchProvider>();
                searchProvider.Init(facts,
                    (path, item) =>
                    {
                        Condition condition = (Condition)property.boxedValue;
                        condition.fact = (Fact)item;
                        property.boxedValue = condition;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    );

                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position.position)), searchProvider);
            }

            // ========== Condition Type dropdown ==========

            Rect conditionTypeRect = position;
            conditionTypeRect.position = new Vector2(factRect.xMax + spacing, position.y);
            conditionTypeRect.size = new Vector2(typeSize, position.size.y);

            SerializedProperty conditionTypeProp = property.FindPropertyRelative("type");
            var type = (Condition.ConditionType)conditionTypeProp.enumValueIndex;

            if (EditorGUI.DropdownButton(conditionTypeRect, new GUIContent(GetConditionTypeEnumSymbol(type)), FocusType.Passive))
            {
                void handleItemClicked(object parameter)
                {
                    conditionTypeProp.enumValueIndex = (int)(Condition.ConditionType)parameter;
                    conditionTypeProp.serializedObject.ApplyModifiedProperties();
                }
                GenericMenu menu = new GenericMenu();

                System.Array conditionTypeEnums = System.Enum.GetValues(typeof(Condition.ConditionType));
                foreach (Condition.ConditionType conditionType in conditionTypeEnums)
                {
                    menu.AddItem(new GUIContent(GetConditionTypeEnumSymbol(conditionType)), false, handleItemClicked, conditionType);
                }

                menu.DropDown(conditionTypeRect);
            }

            // ========== Value to check Int Field ==========

            Rect valueToCheckRect = position;
            valueToCheckRect.position = new Vector2(conditionTypeRect.xMax + spacing, position.y);
            valueToCheckRect.size = new Vector2(valueToCheckSize, position.size.y - 2f);

            SerializedProperty valueToCheckProp = property.FindPropertyRelative("valueToCheck");
            EditorGUI.BeginChangeCheck();
            valueToCheckProp.intValue = EditorGUI.IntField(valueToCheckRect, valueToCheckProp.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        string GetConditionTypeEnumSymbol(Condition.ConditionType type)
        {
            switch (type)
            {
                case Condition.ConditionType.Equal: return "==";
                case Condition.ConditionType.NotEqual: return "!=";
                case Condition.ConditionType.More: return ">";
                case Condition.ConditionType.Less: return "<";
                case Condition.ConditionType.MoreOrEqual: return ">=";
                case Condition.ConditionType.LessOrEqual: return "<=";
                default:
                    Debug.Log($"Condition type [{type}] not handled");
                    return type.ToString();
            }
        }
    }
}