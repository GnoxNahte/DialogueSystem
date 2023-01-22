using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class DialogueEventTrigger : MonoBehaviour
    {
        [SerializeField] protected DialogueSystemData dialogueSystemData = null;
        [SerializeField] protected DialogueEvent dialogueEvent = null;

        [Tooltip("Triggers this event when the conversation is completed")]
        public UnityEngine.Events.UnityEvent<Conversation> OnEndConversationEvent;

        public DialogueSystemData DialogueSystemData => dialogueSystemData;

        public void TriggerEvent()
        {
            Conversation conversation = DialogueSystem.TriggerEvent(dialogueEvent);

            // If there is a conversation that was triggered AND 
            // There is at least 1 OnEndConversationEvent,
            if (conversation != null && OnEndConversationEvent.GetPersistentEventCount() > 0)
            {
                // Subscribe
                DialogueSystem.OnEndConversation += OnEndConversation;
            }
        }

        void OnEndConversation(Conversation conversation, bool wasConversationInterrupted)
        {
            if (!wasConversationInterrupted)
                OnEndConversationEvent?.Invoke(conversation);

            // Unsubscribe to prevent it from triggering
            DialogueSystem.OnEndConversation -= OnEndConversation;
        }

        private void Reset()
        {
            dialogueSystemData = FindObjectOfType<DialogueSystem>().Data;
        }
    }
}