using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class DialogueEvent : DialogueSystemDataPart
    {
        public List<Conversation> conversations;

        DialogueEvent()
        {
            conversations = new List<Conversation>();
        }

        public Conversation TriggerEvent()
        {
            if (conversations.Count == 0)
            {
                Debug.LogWarning($"Triggering event [{name}]when conversations.Count == 0");
                return null;
            }

            foreach (Conversation conversation in conversations)
            {
                if (conversation == null)
                {
                    Debug.LogError($"Triggering event [{name}], Conversation == null.\nCheck DialogueEvent.conversations in data");
                    continue;
                }

                if (conversation.CheckAllConditions())
                    return conversation;
            }

            return null;
        }
    }
}