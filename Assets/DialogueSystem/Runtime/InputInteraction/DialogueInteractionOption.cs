using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{

    public class DialogueInteractionOption : SimpleInteractionOption
    {
        [SerializeField] protected DialogueSystemData dialogueSystemData = null;
        [field: SerializeField] public DialogueEvent DialogueEvent { get; private set; }

        [Tooltip("Triggers this event when the conversation is completed")]
        public UnityEngine.Events.UnityEvent<Conversation> OnEndConversationEvent;

        public DialogueSystemData DialogueSystemData => dialogueSystemData;

        private void Reset()
        {
            PromptParent = transform;
            dialogueSystemData = FindObjectOfType<DialogueSystem>().Data;
        }
    }
}