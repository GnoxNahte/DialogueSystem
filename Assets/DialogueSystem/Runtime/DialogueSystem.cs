using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class DialogueSystem : MonoBehaviour
    {
        [field: SerializeField] public DialogueSystemData Data { get; private set; }

        [ReadOnly]
        [SerializeField] List<DialogueSpeaker> speakers;

        [ReadOnly]
        [SerializeField] DialogueSpeaker player;

        [Header("Settings")]

        [Tooltip("Text speed in words per minute\nHuman reading speed = 250-300 words per min, ")]
        [SerializeField] float talkingSpeed = 250f;

        [ReadOnly]
        [SerializeField]
        float timeBetweenCharacters;

        [Tooltip("Any extra time given to the player to read the dialogue.\n Extra time = timeToSayLine * value")]
        [SerializeField] float extraReadingTimePercentage = 0.5f;

        [SerializeField] float minimumExtraReadingTime = 3f;

        [SerializeField] float timeBetweenConversations = 2f;

        [Tooltip("Controls the scale of the speech bubble when it is appearing")]
        [SerializeField] AnimationCurve appearScaleAnimation;

        [Tooltip("Duration for the appearing animation")]
        [SerializeField] float appearDuration = 1f;

        // Not using System.Queue because need to sort based on conversation priority
        // Element 0 = Start of queue, next conversation is element 0
        // Element n = End of queue, add any new conversations here
        //[SerializeField] List<Conversation> conversationQueue;

        [SerializeField] [ReadOnly]
        Conversation currentConversation = null;

        [SerializeField] InputActionReference interactionKeybind;

        // Param 1 (Conversation) - Conversation that is going to start
        public static event System.Action<Conversation> OnStartConversation;
        
        // Param 1 (Conversation) - Conversation that is ended
        // Param 2 (bool) - If the conversation was interrupted
        public static event System.Action<Conversation, bool> OnEndConversation;

        public string interactionKeybindName { get; private set; }

        public static bool InConversation { get; private set; }
        public static float TalkingSpeed
        {
            get
            {
                if (instance == null)
                    return 0.3f;
                else
                    return instance.talkingSpeed;
            }
        }
        public static AnimationCurve AppearScaleAnimation => instance.appearScaleAnimation;
        public static float AppearDuration => instance.appearDuration;
        public static string InteractionKeybindName => instance.interactionKeybindName;
        public static Conversation CurrConversation => instance.currentConversation;

        // Singleton
        private static DialogueSystem instance;

        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
            {
                Destroy(gameObject);
                Debug.LogWarning("More than 1 DialogueSystem. Destroying this. Name: " + name);
                return;
            }

            Data.ResetPlayData();

            timeBetweenCharacters = 1 / (DialogueSystem.TalkingSpeed * 4.7f / 60f);

            currentConversation = null;

            // TODO (GnoxNahte): Do a proper search on how to get the display string
            string rawString = interactionKeybind.action.GetBindingDisplayString();
            rawString = rawString.Split('|')[0];
            interactionKeybindName = rawString;
        }

        // TODO (GnoxNathe): was not here before
        public static void AddSpeaker(DialogueSpeaker speaker)
        {
            instance?.speakers.Add(speaker);
        }
        // TODO (GnoxNathe): was not here before
        public static void RemoveSpeaker(DialogueSpeaker speaker)
        {
            instance?.speakers.Remove(speaker);
        }

        public static void ModifyFacts(List<ModifyFactParam> parameters)
        {
            foreach (ModifyFactParam p in parameters)
                p.Modify();
        }

        public static Conversation TriggerEvent(DialogueEvent dialogueEvent)
        {
            if (dialogueEvent == null)
            {
                Debug.LogError("Triggering null event");
                return null;
            }

            Conversation result = instance.Data.OnTriggerEvent(dialogueEvent);

            // If the new conversation has higher priority, stop and interrupt the current conversation
            if (instance.currentConversation != null && result.priority <= instance.currentConversation.priority)
            {
                return null;
            }

            if (instance.StartConversation(result))
                return result;
            else
                return null;
        }

        public static void InterruptConversation(Conversation nextConversation)
        {
            instance.StopAllCoroutines();
            instance.StartCoroutine(instance.InterruptConversation_Routine(nextConversation));
        }

        // Returns if can started the conversation
        bool StartConversation(Conversation conversation)
        {
            if (currentConversation != null)
            {
                // If the new conversation has higher priority, stop and interrupt the current conversation
                if (conversation.priority > currentConversation.priority)
                {
                    InterruptConversation(conversation);
                    return true;
                }
                else
                {
                   return false;
                }
            }

            currentConversation = conversation;
            StartCoroutine(StartConversation_Routine(conversation));
            ModifyFacts(conversation.modifyFacts);

            return true;
        }

        IEnumerator StartConversation_Routine(Conversation conversation)
        {
            InConversation = true;
            OnStartConversation?.Invoke(conversation);

            DialogueSpeakerType? previousSpeakerType = null;

            // Do this to filter out all the speakers and reduce the number of times that needs to search the whole list
            // Also set speakers inConversation to true
            List<DialogueSpeaker> speakersInConversation = new List<DialogueSpeaker>();
            foreach (Conversation.Dialogue d in conversation.dialogues)
            {
                DialogueSpeaker speaker = FindSpeaker(d.speakerType);

                if (speaker == null)
                {
                    Debug.LogError($"Can't find speaker [{d.speakerType}], skipping this line: \n{d.dialogue}");
                    continue;
                }

                speakersInConversation.Add(speaker);
                speaker.inConversation = true;
            }

            player.inConversation = true;

            // If this conversation was interrupted before
            if (conversation.currDialogueIndex != -1)
            {
                DialogueSpeaker speaker = FindSpeaker(conversation.dialogues[conversation.currDialogueIndex].speakerType);

                if (speaker == null)
                {
                    Debug.LogError($"Can't find speaker [{speaker.type}]. Cannot continue conversation");
                }
                // Similar to the code in the while loop below
                else
                {
                    conversation.GoBackToPreviousDialogue();

                    if (speaker.IsAnimatingSpeechBubble)
                        yield return speaker.WaitForSpeechBubbleAnimation();

                    previousSpeakerType = speaker.type;

                    yield return speaker.SayLine(speaker.CoutinueDialogue);

                    float timeToSayLine = speaker.CoutinueDialogue.Length * timeBetweenCharacters;
                    //Debug.Log("Waiting for : " + timeToSayLine * extraReadingTimePercentage);
                    yield return new WaitForSeconds(Mathf.Max(minimumExtraReadingTime, timeToSayLine * extraReadingTimePercentage));
                }
            }

            Conversation.Dialogue dialogue = conversation.GetNextDialogue();

            while (dialogue != null)
            {
                DialogueSpeaker speaker = FindSpeaker(dialogue.speakerType, speakersInConversation);

                if (speaker.IsAnimatingSpeechBubble)
                    yield return speaker.WaitForSpeechBubbleAnimation();

                if (dialogue.speakerType == previousSpeakerType)
                    yield return speaker.CloseSpeechBubble();

                previousSpeakerType = dialogue.speakerType;

                yield return speaker.SayLine(dialogue.dialogue);

                float timeToSayLine = dialogue.dialogue.Length * timeBetweenCharacters;
                //Debug.Log("Waiting for : " + timeToSayLine * extraReadingTimePercentage);
                yield return new WaitForSeconds(Mathf.Max(minimumExtraReadingTime, timeToSayLine * extraReadingTimePercentage));

                dialogue = conversation.GetNextDialogue();
            }

            // ===== Conversation end, clean up =====

            foreach (DialogueSpeaker dialogueSpeaker in speakersInConversation)
                dialogueSpeaker.inConversation = false;

            player.inConversation = false;

            yield return StopConversation_Routine();

            // Trigger the next events
            foreach (DialogueEvent dialogueEvent in conversation.events)
                TriggerEvent(dialogueEvent);
        }

        // Stops the current conversation and start the next conversation (if any)
        IEnumerator StopConversation_Routine()
        {
            if (currentConversation == null)
                yield break;

            List<Coroutine> coroutines = new List<Coroutine>();

            player.inConversation = false;

            // Close all Speech bubbles
            foreach (DialogueSpeaker speaker in speakers)
            {
                speaker.inConversation = false;

                speaker.StopSpeechBubbleAnimations();
                IEnumerator enumerator = speaker.CloseSpeechBubble();
                if (enumerator != null)
                    coroutines.Add(StartCoroutine(enumerator));
            }

            // Wait for all coroutines to finish
            foreach (Coroutine coroutine in coroutines)
                yield return coroutine;

            bool wasConversationInterrupted = false;

            // If the conversation was interrupted, say the interrupted dialogue and close the speech bubble
            // NOTE: Not waiting for the saying line and closing of the speech bubble
            if (currentConversation.currDialogueIndex != -1)
            {
                wasConversationInterrupted = true;

                DialogueSpeaker speaker = FindSpeaker(currentConversation.dialogues[currentConversation.currDialogueIndex].speakerType);
                speaker.SayLine(speaker.InterruptedDialogue);
                speaker.CloseSpeechBubble();
            }
            else
            {
                // wait for some time between conversations
                yield return new WaitForSeconds(timeBetweenConversations);
            }

            OnEndConversation?.Invoke(currentConversation, wasConversationInterrupted);

            currentConversation = null;

            InConversation = false;
        }

        private IEnumerator InterruptConversation_Routine(Conversation nextConversation)
        {
            Conversation interruptedConversation = currentConversation;

            // If it is at the last dialogue, count it as the dialogue not being interrupted and 
            // set the dialogue index to -1
            if (interruptedConversation.currDialogueIndex == interruptedConversation.dialogues.Length - 1)
                interruptedConversation.currDialogueIndex = -1;

            yield return StopConversation_Routine();

            if (nextConversation != null)
                StartConversation(nextConversation);
        }

        private DialogueSpeaker FindSpeaker(DialogueSpeakerType speakerType, List<DialogueSpeaker> speakersList = null)
        {
            if (speakersList == null)
                speakersList = speakers;

            foreach (DialogueSpeaker speaker in speakersList)
            {
                if (speaker.type == speakerType)
                    return speaker;
            }

            return null;
        }

        private void OnValidate()
        {
            timeBetweenCharacters = 1 / (talkingSpeed * 4.7f / 60f);
        }

        [ContextMenu("AssignReferences")]
        public void AssignReferences()
        {
            speakers = new List<DialogueSpeaker>(FindObjectsOfType<DialogueSpeaker>());

            if (player == null)
            {
                foreach (DialogueSpeaker speaker in speakers)
                {
                    if (speaker.type == DialogueSpeakerType.Player)
                    {
                        player = speaker;
                        return;
                    }
                }
            }
        }

        [ContextMenu("ManualValidate")]
        public void ManualValidate()
        {
            Debug.Log("Validate Dialogue System start");
            DialogueEventTrigger[] eventTriggers = FindObjectsOfType<DialogueEventTrigger>();
            foreach (DialogueEventTrigger trigger in eventTriggers)
            {
                if (trigger.DialogueSystemData != Data)
                    Debug.LogError($"DialogueEventTrigger [{name}] has a different DialogueSystemData than DialogueSystem. DialogueEventTrigger has [{trigger.DialogueSystemData.name}] as data and DialogueSystem has [{Data.name}] as data");
            }

            DialogueInteractionOption[] interactionOptions = FindObjectsOfType<DialogueInteractionOption>();
            foreach (DialogueInteractionOption option in interactionOptions)
            {
                if (option.DialogueSystemData != Data)
                    Debug.LogError($"DialogueInteractionOption [{name}] has a different DialogueSystemData than DialogueSystem. DialogueInteractionOption has [{option.DialogueSystemData.name}] as data and DialogueSystem has [{Data.name}] as data");
            }

            DialogueAreaCheck[] areaChecks = FindObjectsOfType<DialogueAreaCheck>();
            foreach (DialogueAreaCheck check in areaChecks)
            {
                if (check.data != Data)
                    Debug.LogError($"DialogueAreaCheck [{name}] has a different DialogueSystemData than DialogueSystem. DialogueAreaCheck has [{check.data.name}] as data and DialogueSystem has [{Data.name}] as data");
            }

            Debug.Log("Validate Dialogue System end");
        }
    }
}