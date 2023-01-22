using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace GnoxNahte.DialogueSystem.Runtime
{
    // Similar to ConversationPriority
    public enum InteractionPriority
    {
        // The comments are just an example of how to organise the priority
        Lowest, // Random things like player noticing a non-important object
        Low, // Others
        Medium, // Normal 
        High, // NPC interactions
        Highest, // Cutscenes
    }

    public class SimpleInteractionOption : MonoBehaviour
    {
        [field: SerializeField] public InteractionPriority Priority { get; protected set; } = InteractionPriority.Medium;

        // Assigns the prompt to be a child of this
        [field: SerializeField] public Transform PromptParent { get; protected set; }

        [Tooltip("Triggers this event when the option is triggered")]
        public UnityEngine.Events.UnityEvent OnTriggerOptionEvent;

        public int PriorityValue => (int)Priority;

        // NOTE: It doesn't check if overritten keybind is pressed, only shows the prompt for it
        // Might want to create a function that subscribes to the event when the keybind is pressed when this option is active
        [SerializeField] protected InputActionReference overrideInteractionKeybind;
        public string OverrideInteractionKeybindName { get; protected set; }

        private void Start()
        {
            if (GetComponent<Collider2D>() == null)
                Debug.LogError($"GameObject [{name}] doesn't have a Collider2D component");
            else if (!GetComponent<Collider2D>().isTrigger)
                Debug.LogError($"GameObject [{name}]'s Collider2D.isTrigger != true");

            if (PromptParent == null)
                PromptParent = transform;

            if (overrideInteractionKeybind == null)
                OverrideInteractionKeybindName = string.Empty;
            else
                OverrideInteractionKeybindName = overrideInteractionKeybind.action.GetBindingDisplayString();
        }


    }
}
