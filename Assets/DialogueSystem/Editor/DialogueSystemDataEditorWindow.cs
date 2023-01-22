using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using GnoxNahte.DialogueSystem.Runtime;

namespace GnoxNahte.DialogueSystem.EditorMode
{
    public class DialogueSystemDataEditorWindow : EditorWindow
    {
        SerializedObject serializedObject;

        Entry selectedEntry;
        SimpleReorderableList<Entry> entryList;

        DialogueSystemDataPart selectedPart;
        SerializedObject selectedPartSerializedObj;

        SimpleReorderableList<Fact> factList;
        SimpleReorderableList<DialogueEvent> eventList;
        SimpleReorderableList<Conversation> conversationList;

        DialogueSystemData dialogueSystemData;

        private Vector2 sidebarScrollPos;
        private Vector2 entryPropViewScrollPos;

        bool entryFoldout = true;
        bool factFoldout = true;
        bool eventFoldout = true;
        bool conversationFoldout = true;

        SimpleReorderableList<Conversation> conversationReorderableList;

        void OnSelectPart<T>(T part) where T : DialogueSystemDataPart
        {
            SelectPart(dialogueSystemData, part);
        }

        T OnAddPart<T>() where T : DialogueSystemDataPart
        {
            ScriptableObject newData = ScriptableObject.CreateInstance<T>();

            int arraySize = 0;
            System.Type type = typeof(T);

            switch (type.Name)
            {
                case nameof(Entry): arraySize = dialogueSystemData.entries.Count; break;
                case nameof(Fact): arraySize = selectedEntry.facts.Count; break;
                case nameof(DialogueEvent): arraySize = selectedEntry.events.Count; break;
                case nameof(Conversation): arraySize = selectedEntry.conversations.Count; break;
                default: Debug.LogError($"{nameof(T)} not handled"); break;
            }

            if (newData != null)
            {
                newData.name = $"{type.Name} {arraySize + 1}";

                AssetDatabase.AddObjectToAsset(newData, dialogueSystemData);

                AssetDatabase.SaveAssets();
                serializedObject.ApplyModifiedProperties();
            }

            return (T)newData;
        }

        void OnRemovePart<T>(T part) where T : DialogueSystemDataPart
        {
            // Delete all parts that is parented to this
            Entry entry = part as Entry;
            if (entry != null)
            {
                foreach (Fact fact in entry.facts)
                    Object.DestroyImmediate(fact, true);

                foreach (DialogueEvent dialogueEvent in entry.events)
                    Object.DestroyImmediate(dialogueEvent, true);

                foreach (Conversation conversation in entry.conversations)
                    Object.DestroyImmediate(conversation, true);
            }

            Object.DestroyImmediate(part, true);

            AssetDatabase.SaveAssets();
        }

        public void SelectPart(DialogueSystemDataPart part)
        {
            if (part == null)
                return;
            for (int i = 0; i < dialogueSystemData.entries.Count; i++)
            {
                Entry entry = dialogueSystemData.entries[i];
                int partIndex = -1;

                if (entry == part)
                {
                    selectedEntry = entry;
                    selectedPart = null;
                    selectedPartSerializedObj = null;

                    SerializedObject entrySerializedObject = new SerializedObject(entry);
                    factList = new SimpleReorderableList<Fact>(
                        "Facts",
                        dialogueSystemData.entries[i].facts,
                        entrySerializedObject.FindProperty("facts"),
                        onAddAction: OnAddPart<Fact>,
                        onRemoveAction: OnRemovePart,
                        onSelectAction: OnSelectPart);

                    eventList = new SimpleReorderableList<DialogueEvent>(
                        "Events",
                        dialogueSystemData.entries[i].events,
                        entrySerializedObject.FindProperty("events"),
                        onAddAction: OnAddPart<DialogueEvent>,
                        onRemoveAction: OnRemovePart,
                        onSelectAction: OnSelectPart);

                    conversationList = new SimpleReorderableList<Conversation>(
                        "Conversations",
                        dialogueSystemData.entries[i].conversations,
                        entrySerializedObject.FindProperty("conversations"),
                        onAddAction: OnAddPart<Conversation>,
                        onRemoveAction: OnRemovePart,
                        onSelectAction: OnSelectPart);

                    return;
                }

                // Find the fact index
                for (int j = 0; j < entry.facts.Count; j++)
                    if (entry.facts[j] == part)
                        partIndex = j;

                // Find the event index
                for (int j = 0; j < entry.events.Count; j++)
                    if (entry.events[j] == part)
                        partIndex = j;

                // Find the fact index
                for (int j = 0; j < entry.conversations.Count; j++)
                    if (entry.conversations[j] == part)
                        partIndex = j;

                if (partIndex >= 0)
                {
                    // If any lists are null, select the entry to rebuild it
                    if (factList == null || eventList == null || conversationList == null)
                        SelectPart(entry);

                    selectedEntry = entry;

                    selectedPart = part;
                    selectedPartSerializedObj = new SerializedObject(part);

                    if (part.GetType() == typeof(DialogueEvent))
                    {
                        DialogueEvent dialogueEvent = (DialogueEvent)selectedPart;

                        SerializedProperty conversationProp = selectedPartSerializedObj.FindProperty("conversations");
                        conversationReorderableList = new SimpleReorderableList<Conversation>
                            ("Conversations", dialogueEvent.conversations, conversationProp, ifDrawUsingLabel: false);
                    }

                    Repaint();

                    return;
                }
            }

            Debug.LogError($"Can't find part: {part.name}");
        }

        public static void SelectPart(DialogueSystemData data, DialogueSystemDataPart part)
        {
            DialogueSystemDataEditorWindow window = null;

            // Checks if any opened window already has this dialogue system 
            DialogueSystemDataEditorWindow[] dialogueWindows = Resources.FindObjectsOfTypeAll<DialogueSystemDataEditorWindow>();
            
            foreach (var w in dialogueWindows)
                if (w.dialogueSystemData == data)
                    window = w;

            // If can't find the window, just return
            if (window == null)
                return;
            else
                window.Focus();

            window.Show();

            window.SelectPart(part);
        }

        private void OnEnable()
        { 
            if (dialogueSystemData != null)
            {
                SerializedObject dialogueSystemDataSerializedObj = new SerializedObject(dialogueSystemData);
                entryList = new SimpleReorderableList<Entry>(
                    "Entries",
                    dialogueSystemData.entries,
                    dialogueSystemDataSerializedObj.FindProperty("entries"),
                    onAddAction: OnAddPart<Entry>,
                    onRemoveAction: OnRemovePart,
                    onSelectAction: OnSelectPart);
                 
                serializedObject = new SerializedObject(dialogueSystemData);
                SelectPart(dialogueSystemData, selectedEntry); 
                SelectPart(dialogueSystemData, selectedPart); 
            }
        }


        public static DialogueSystemDataEditorWindow OpenEditor(DialogueSystemData dialogueSystemData)
        {
            DialogueSystemDataEditorWindow window = null;

            // Checks if any opened window already has this dialogue system 
            DialogueSystemDataEditorWindow[] dialogueWindows = Resources.FindObjectsOfTypeAll<DialogueSystemDataEditorWindow>();
            foreach (var w in dialogueWindows)
            {
                if (w.dialogueSystemData == dialogueSystemData)
                {
                    window = w;
                }
            }

            if (window == null)
                window = CreateInstance<DialogueSystemDataEditorWindow>();
            else
                window.Focus();

            window.titleContent = new GUIContent(dialogueSystemData?.name);

            window.dialogueSystemData = dialogueSystemData;

            window.OnEnable();

            window.Show();

            return window;
        }

        private void OnGUI()
        {
            if (serializedObject == null)
            {
                EditorGUILayout.LabelField("Select a DialogueSystemData scriptable object");
                dialogueSystemData = (DialogueSystemData)EditorGUILayout.ObjectField(dialogueSystemData, typeof(DialogueSystemData), false);
                return;
            }

            serializedObject.Update();

            SerializedProperty entriesProp = serializedObject.FindProperty("entries");

            EditorGUILayout.BeginHorizontal();

            #region Entry Sidebar 

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(200f), GUILayout.ExpandHeight(true));

            entryList.DrawList();
            
            EditorGUILayout.EndVertical();

            #endregion

            #region Fact, DialogueEvent, Conversation Sidebar

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(200f), GUILayout.MinWidth(100f), GUILayout.ExpandHeight(true));

            if (selectedEntry == null)
            {
                EditorGUILayout.LabelField("Select an entry");
            }
            else
            {
                sidebarScrollPos = EditorGUILayout.BeginScrollView(sidebarScrollPos);

                factList.DrawList();
                eventList.DrawList();
                conversationList.DrawList();

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();

            #endregion

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));

            DrawPartProperties();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            return;
        }

        void DrawPartProperties()
        {
            if (selectedEntry == null)
            {
                EditorGUILayout.LabelField("Select a Entry");
                return;
            }
            entryPropViewScrollPos = EditorGUILayout.BeginScrollView(entryPropViewScrollPos);
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.BeginHorizontal("box");

            EditorGUILayout.LabelField("Entry Name: ");

            selectedEntry.name = EditorGUILayout.TextField(selectedEntry.name);

            EditorGUILayout.EndHorizontal();

            if (selectedPart == null)
            {
                EditorGUILayout.LabelField("Select a Fact / Event / Conversation");
            }
            else
            {
                EditorGUILayout.BeginVertical("box");

                switch (selectedPart.GetType().Name)
                {
                    case nameof(Fact): DrawFactProps(); break;
                    case nameof(DialogueEvent): DrawDialogueEventProps(); break;
                    case nameof(Conversation): DrawConversationProps(); break;
                    default: Debug.LogError($"{selectedPart.GetType().Name} not handled"); break;
                }

                EditorGUILayout.EndHorizontal();
            }

            //if (Event.current.keyCode == KeyCode.Return)
            //    AssetDatabase.SaveAssets();

            EditorGUILayout.EndScrollView();
        }

        void DrawFactProps()
        {
            Fact fact = selectedPart as Fact;
            if (fact == null)
            {
                Debug.LogError("selectedPart.type != Fact");
                return;
            }    

            selectedPart.name = TextFieldWithLabel("Fact Name: ", selectedPart.name);

            EditorGUI.BeginChangeCheck();
            fact.startingValue = IntFieldWithLabel("Starting Value: ", fact.startingValue);
            if (EditorGUI.EndChangeCheck() && !Application.isPlaying)
                fact.value = fact.startingValue;

            GUI.enabled = false;
            IntFieldWithLabel("Current Value: ", fact.value);
            GUI.enabled = true;
        }

        void DrawDialogueEventProps()
        {
            selectedPart.name = TextFieldWithLabel("Dialogue Event Name: ", selectedPart.name);
            conversationReorderableList.DrawList();
        }

        void DrawConversationProps()
        {
            selectedPart.name = TextFieldWithLabel("Conversation Name: ", selectedPart.name);
            // Cache obj since it might change when selecting another part in one of the property. See references to SelectPart()
            SerializedObject obj = selectedPartSerializedObj;
            
            EditorGUILayout.PropertyField(obj.FindProperty("conditions"));
            obj.ApplyModifiedProperties();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(obj.FindProperty("priority"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(obj.FindProperty("dialogues"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(obj.FindProperty("modifyFacts"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(obj.FindProperty("events"));
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(obj.FindProperty("currDialogueIndex"));

            obj.ApplyModifiedProperties();
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }

        #region Helper functions

        private void CenteredLabel(string label)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        private string TextFieldWithLabel(string labelTitle, string initialValue)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(labelTitle, GUILayout.ExpandWidth(false));

            string newValue = EditorGUILayout.TextField(initialValue, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            return newValue;
        }

        private int IntFieldWithLabel(string labelTitle, int initialValue)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(labelTitle, GUILayout.ExpandWidth(false));

            int newValue = EditorGUILayout.IntField(initialValue, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            return newValue;
        }

        private int FindIndex(SerializedProperty arrayProp, Object obj)
        {
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue == obj)
                    return i;
            }

            return -1;
        }

        #endregion
    }
}