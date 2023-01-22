using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class DialogueInteractionReceiver : MonoBehaviour
    {
        [SerializeField] [ReadOnly]
        SimpleInteractionOption currOption;

        // Array that stores a List of options
        // option[priority]     == List of options in that priority
        // option[priority][i]  == Get the option in the list with the priority
        // Do this to sepearte the different options by priority,
        // making it faster to iterate the list to find the closest option with the priority
        [SerializeField] [ReadOnly]
        public List<SimpleInteractionOption>[] options;

        public List<SimpleInteractionOption> option1;
        public List<SimpleInteractionOption> option2;
        public List<SimpleInteractionOption> option3;
        public List<SimpleInteractionOption> option4;
        public List<SimpleInteractionOption> option5;

        [SerializeField] SpeechBubble promptSpeechBubble;

        // The option that triggered the event
        // null if never trigger any events / finish conversations
        SimpleInteractionOption triggeredOption;

        readonly int priorityCount = System.Enum.GetNames(typeof(InteractionPriority)).Length;

        private void Awake()
        {
            // Init options
            options = new List<SimpleInteractionOption>[priorityCount];
            for (int i = 0; i < options.Length; i++)
                options[i] = new List<SimpleInteractionOption>(3);


            option1 = options[0];
            option2 = options[1];
            option3 = options[2];
            option4 = options[3];
            option5 = options[4];
        }

        public void OnInteractPressed(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (currOption == null)
                    return;

                DialogueInteractionOption dialogueInteractionOption = currOption as DialogueInteractionOption;
                if (dialogueInteractionOption != null)
                {
                    Conversation conversation = DialogueSystem.TriggerEvent(dialogueInteractionOption.DialogueEvent);

                    // If there is a conversation that was triggered AND 
                    // There is at least 1 OnEndConversationEvent,
                    if (conversation != null && dialogueInteractionOption.OnEndConversationEvent.GetPersistentEventCount() > 0)
                    {
                        dialogueInteractionOption.OnTriggerOptionEvent?.Invoke();
                        triggeredOption = currOption;

                        // No need to subscribe to DialogueSystem.OnEndConversation because already subscribe in OnEnable()
                    }
                }
                else
                {
                    currOption.OnTriggerOptionEvent?.Invoke();
                    options[currOption.PriorityValue].Remove(currOption);

                    SetCurrentOption(null);
                }
            }
        }

        public void ReturnSpeechBubble()
        {
            // TODO (GnoxNahte): Reset speech bubble (Animations, queue, etc)
            promptSpeechBubble.StopAllAnimations();
            promptSpeechBubble.transform.SetParent(transform, false);
        }

        private void OnStartConversation(Conversation conversation)
        {
            promptSpeechBubble.Close();
        }

        private void OnEndConversation(Conversation conversation, bool wasConversationInterrupted)
        {
            DialogueInteractionOption dialogueTriggeredOption = triggeredOption as DialogueInteractionOption;

            if (!wasConversationInterrupted && dialogueTriggeredOption != null)
                dialogueTriggeredOption.OnEndConversationEvent?.Invoke(conversation);

            triggeredOption = null;

            if (currOption != null)
            {
                var newOption = GetClosestOption(options[currOption.PriorityValue]);
                SetCurrentOption(newOption);
            }
        }

        private void OnEnable()
        {
            DialogueSystem.OnStartConversation += OnStartConversation;
            DialogueSystem.OnEndConversation += OnEndConversation;
        }

        private void OnDisable()
        {
            DialogueSystem.OnStartConversation -= OnStartConversation;
            DialogueSystem.OnEndConversation -= OnEndConversation;
        }

        private void Update()
        {
            if (currOption != null)
            {
                var newOption = GetClosestOption(options[currOption.PriorityValue]);
                if (newOption != currOption)
                    SetCurrentOption(newOption);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            SimpleInteractionOption option = collision.GetComponent<SimpleInteractionOption>();
            if (option != null)
            {
                print("A");
                options[option.PriorityValue].Add(option);

                if (currOption == null || option.Priority > currOption.Priority)
                {
                    SetCurrentOption(option);
                }
            }

            else if (collision.GetComponent<DialogueInteractionOption>() != null)
            {
                print("B");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            SimpleInteractionOption option = collision.GetComponent<SimpleInteractionOption>();
            if (option != null)
            {
                options[option.PriorityValue].Remove(option);

                if (option == currOption)
                {
                    bool ifFound = false;

                    // Search the other priorities to see if there are any other options
                    // Search from the highest priority first
                    for (int i = priorityCount - 1; i >= 0; --i)
                    {
                        if (options[i].Count > 0)
                        {
                            SetCurrentOption(GetClosestOption(options[i]));
                            ifFound = true;
                            break;
                        }
                    }

                    if (!ifFound)
                        SetCurrentOption(null);
                }
            }
        }

        private void SetCurrentOption(SimpleInteractionOption option)
        {
            if (option == null)
            {
                promptSpeechBubble.Close();
                currOption = null;

                return;
            }

#if UNITY_EDITOR
            // Check if there is any higher priority options
            // Shouldn't happen and might be expensive so check in editor only
            for (int i = option.PriorityValue + 1; i < priorityCount; ++i)
            {
                if (options[i].Count > 0)
                    Debug.LogError("Setting new interaction option when there is a higher option");
            }
#endif
            if (currOption != null)
                promptSpeechBubble.Close();

            currOption = option;

            // Only show if not in conversation
            if (!DialogueSystem.InConversation)
            {
                if (string.IsNullOrEmpty(option.OverrideInteractionKeybindName))
                    promptSpeechBubble.Show(DialogueSystem.InteractionKeybindName, option.PromptParent);
                else
                    promptSpeechBubble.Show(option.OverrideInteractionKeybindName, option.PromptParent);

            }
        }

        private SimpleInteractionOption GetClosestOption(List<SimpleInteractionOption> searchOptions)
        {
            SimpleInteractionOption closestOption = null;
            float closestSqrDist = float.MaxValue;
            foreach (var option in searchOptions)
            {
                float sqrDist = (option.transform.position - transform.position).sqrMagnitude;

                if (sqrDist < closestSqrDist)
                {
                    closestOption = option;
                    closestSqrDist = sqrDist;
                }
            }

            return closestOption;
        }
    }
}