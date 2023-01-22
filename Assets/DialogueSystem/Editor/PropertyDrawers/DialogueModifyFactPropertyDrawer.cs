using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    [CustomPropertyDrawer(typeof(ModifyFactParam))]
    public class DialogueModifyFactPropertyDrawer : PropertyDrawer
    {
        float typeSize = 40f;
        float valueToCheckSize = 50f;

        float spacing = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the dialogueSystemData that is parented to this
            DialogueSystemData dialogueSystemData = (DialogueSystemData)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(property.serializedObject.targetObject));


            // Try getting the data another way. Might happen if the serializedObject is a monobehaviour
            if (dialogueSystemData == null)
                dialogueSystemData = (DialogueSystemData)property.serializedObject.FindProperty("dialogueSystemData").objectReferenceValue;


            if (dialogueSystemData == null)
            {
                Debug.LogError("Drawing ModifyFactParam Property Drawer, Cannot get dialogueSystemData using MainAsset \nDrawing default property");
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            property.serializedObject.Update();

            ModifyFactParam modifyFact = (ModifyFactParam)property.boxedValue;

            // ========== Ping And Select Asset Button ==========

            float pingButtonSize = position.size.y;
            Rect pingButtonRect = position;
            pingButtonRect.size = new Vector2(pingButtonSize + 7f, pingButtonSize);


            Vector2 originalIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(Vector2.one * position.size.y);

            if (GUI.Button(pingButtonRect, (Texture)EditorGUIUtility.Load("d_SearchJump Icon")) && modifyFact.fact != null)
            {
                EditorGUIUtility.PingObject(modifyFact.fact);

                DialogueSystemDataEditorWindow.SelectPart(dialogueSystemData, modifyFact.fact);
            }

            EditorGUIUtility.SetIconSize(originalIconSize);

            // ========== Fact Search dropdown ==========

            Rect factRect = position;
            // Fact property takes up all the space that type and valueToCheck aren't using
            factRect.position = new Vector2(pingButtonRect.xMax + spacing, position.y);
            factRect.size = new Vector2(position.size.x - pingButtonSize - typeSize - valueToCheckSize - spacing * 4f, position.size.y);

            string buttonLabel;
            if (modifyFact.fact == null)
                buttonLabel = "null";
            else
                buttonLabel = $"{dialogueSystemData.GetFactPath(modifyFact.fact)} [{modifyFact.fact.value}]";

            if (buttonLabel == string.Empty && dialogueSystemData.GetFactPath(modifyFact.fact) == string.Empty)
                buttonLabel = "Cannot find Fact. Check if the Dialogue System Data is the same";

            if (GUI.Button(factRect, buttonLabel, EditorStyles.popup))
            {
                var facts = dialogueSystemData.GetAllFacts();

                SimpleSearchProvider searchProvider = ScriptableObject.CreateInstance<SimpleSearchProvider>();
                searchProvider.Init(facts,
                    (path, item) =>
                    {
                        ModifyFactParam modifyFact = (ModifyFactParam)property.boxedValue;
                        modifyFact.fact = (Fact)item;
                        property.boxedValue = modifyFact;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                    );

                SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(position.position)), searchProvider);
            }

            // ========== ModifyFactParam Type dropdown ==========

            Rect typeRect = position;
            typeRect.position = new Vector2(factRect.xMax + spacing, position.y);
            typeRect.size = new Vector2(typeSize, position.size.y);

            SerializedProperty typeProp = property.FindPropertyRelative("type");
            var type = (ModifyFactParam.ModifyFactType)typeProp.enumValueIndex;

            if (EditorGUI.DropdownButton(typeRect, new GUIContent(GetModifyFactTypeEnumSymbol(type)), FocusType.Passive))
            {
                void handleItemClicked(object parameter)
                {
                    typeProp.enumValueIndex = (int)(ModifyFactParam.ModifyFactType)parameter;
                    typeProp.serializedObject.ApplyModifiedProperties();
                }
                GenericMenu menu = new GenericMenu();

                System.Array typeEnums = System.Enum.GetValues(typeof(ModifyFactParam.ModifyFactType));
                foreach (ModifyFactParam.ModifyFactType t in typeEnums)
                {
                    menu.AddItem(new GUIContent(GetModifyFactTypeEnumSymbol(t)), false, handleItemClicked, t);
                }

                menu.DropDown(typeRect);
            }

            // ========== Value to check Int Field ==========

            Rect modifyValueRect = position;
            modifyValueRect.position = new Vector2(typeRect.xMax + spacing, position.y);
            modifyValueRect.size = new Vector2(valueToCheckSize, position.size.y - 2f);

            SerializedProperty modifyValueProp = property.FindPropertyRelative("modifyValue");
            EditorGUI.BeginChangeCheck();
            modifyValueProp.intValue = EditorGUI.IntField(modifyValueRect, modifyValueProp.intValue);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        string GetModifyFactTypeEnumSymbol(ModifyFactParam.ModifyFactType type)
        {
            switch (type)
            {
                case ModifyFactParam.ModifyFactType.Set: return "=";
                case ModifyFactParam.ModifyFactType.Add: return "+";
                case ModifyFactParam.ModifyFactType.Subtract: return "-";
                case ModifyFactParam.ModifyFactType.Multiply: return "*";
                case ModifyFactParam.ModifyFactType.Divide: return "\\";
                default:
                    Debug.Log($"ModifyFactType type [{type}] not handled");
                    return type.ToString();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }
}