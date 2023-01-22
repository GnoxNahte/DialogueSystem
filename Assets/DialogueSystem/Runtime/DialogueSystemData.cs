using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    // A data type to make it easier to manage the data
    public abstract class DialogueSystemDataPart : ScriptableObject 
    { 
        //virtual public void Init() { }
    }

    [Serializable]
    public class Condition
    {
        public enum ConditionType
        {
            Equal,
            NotEqual,
            More,
            Less,
            MoreOrEqual,
            LessOrEqual,
        }

        public ConditionType type;

        public Fact fact;

        public int valueToCheck;

        public bool GetResult()
        {
            switch (type)
            {
                case ConditionType.Equal: return fact.value == valueToCheck;
                case ConditionType.NotEqual: return fact.value != valueToCheck;
                case ConditionType.More: return fact.value > valueToCheck;
                case ConditionType.Less: return fact.value < valueToCheck;
                case ConditionType.MoreOrEqual: return fact.value >= valueToCheck;
                case ConditionType.LessOrEqual: return fact.value <= valueToCheck;
                default:
                    Debug.LogError($"ConditionType: [{type}] is not implemented");
                    return false;
            }
        }
    }


    [CreateAssetMenu(fileName = "DialogueSystemData", menuName = "DialogueSystemData", order = int.MaxValue)]
    public class DialogueSystemData : ScriptableObject
    {
        public List<Entry> entries;

        // Reset the data every playthrough
        public void ResetPlayData()
        {
            foreach (Entry entry in entries)
            {
                foreach (Fact fact in entry.facts)
                    fact.value = fact.startingValue;

                foreach (Conversation conversation in entry.conversations)
                    conversation.currDialogueIndex = -1;
            }
        }

        public Conversation OnTriggerEvent(DialogueEvent dialogueEvent)
        {
            return dialogueEvent.TriggerEvent();
        }

        public void VerifyData()
        {
            Debug.Log("Start Verifying Data...");

            foreach (Entry entry in entries)
            {
                foreach (Fact fact in entry.facts)
                {
                    if (string.IsNullOrEmpty(fact.name))
                        Debug.LogError($"Fact name is empty. Parent Entry Name: {entry.name}");
                }

                foreach (DialogueEvent dialogueEvent in entry.events)
                {
                    if (string.IsNullOrEmpty(dialogueEvent.name))
                        Debug.LogError($"DialogueEvent name is empty. Parent Entry Name: {entry.name}");

                    if (dialogueEvent.conversations.Count == 0)
                        Debug.LogError($"DialogueEvent has no conversations. Path: {entry.name}/{dialogueEvent.name}");

                    foreach (Conversation conversation in dialogueEvent.conversations)
                    {
                        if (conversation == null)
                            Debug.LogError($"Inside DialogueEvent.conversations, it has a null element. Path: {entry.name}/{dialogueEvent.name}");
                    }
                }

                foreach (Conversation conversation in entry.conversations)
                {
                    if (string.IsNullOrEmpty(conversation.name))
                        Debug.LogError($"Conversation name is empty. Parent Entry Name: {entry.name}");

                    foreach (Condition condition in conversation.conditions)
                    {
                        if (condition == null)
                            Debug.LogError($"Inside Conversation.conditions, it has a null element. Path: {entry.name}/{conversation.name}");
                    }

                    if (conversation.dialogues.Length == 0)
                        Debug.LogError($"Conversation has no dialogues. Path: {entry.name}/{conversation.name}");


                    foreach (ModifyFactParam modifyFactParam in conversation.modifyFacts)
                    {
                        if (modifyFactParam == null)
                            Debug.LogError($"Inside Conversation.modifyFact, it has a null element. Path: {entry.name}/{conversation.name}");
                        else if (modifyFactParam.fact == null)
                            Debug.LogError($"Inside Conversation.modifyFact.fact, it is null. Path: {entry.name}/{conversation.name}");
                    }
                }
            }

            Debug.Log("Finish verifying data");
        }

        public List<Tuple<string, object>> GetAllEvents()
        {
            List<Tuple<string, object>> result = new List<Tuple<string, object>>();

            foreach (Entry entry in entries)
                foreach (DialogueEvent dialogueEvent in entry.events)
                    result.Add(new Tuple<string, object>($"{entry.name}/{dialogueEvent.name}", dialogueEvent));

            return result;
        }

        public List<Tuple<string, object>> GetAllConversations()
        {
            List<Tuple<string, object>> result = new List<Tuple<string, object>>();

            foreach (Entry entry in entries)
                foreach (Conversation conversation in entry.conversations)
                    result.Add(new Tuple<string, object>($"{entry.name}/{conversation.name}", conversation));

            return result;
        }

        public List<Tuple<string, object>> GetAllFacts()
        {
            List<Tuple<string, object>> result = new List<Tuple<string, object>>();

            foreach (Entry entry in entries)
                foreach (Fact fact in entry.facts)
                    result.Add(new Tuple<string, object>($"{entry.name}/{fact.name}", fact));

            return result;
        }

        public string GetFactPath(Fact fact)
        {
            if (fact == null)
                return string.Empty;

            foreach (Entry entry in entries)
            {
                foreach (Fact f in entry.facts)
                {
                    if (f == fact)
                        return $"{entry.name}/{f.name}";
                }
            }

            return string.Empty;
        }

        public string GetDialogueEventPath(DialogueEvent dialogueEvent)
        {
            if (dialogueEvent == null)
                return string.Empty;

            foreach (Entry entry in entries)
            {
                foreach (DialogueEvent de in entry.events)
                {
                    if (de == dialogueEvent)
                        return $"{entry.name}/{de.name}";
                }
            }

            return string.Empty;
        }
        public string GetConversationPath(Conversation conversation)
        {
            if (conversation == null)
                return string.Empty;

            foreach (Entry entry in entries)
            {
                foreach (Conversation el in entry.conversations)
                {
                    if (el == conversation)
                        return $"{entry.name}/{el.name}";
                }
            }

            return string.Empty;
        }
    }
}
