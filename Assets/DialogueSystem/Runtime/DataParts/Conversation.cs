using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public enum ConversationPriority {
        // The comments are just an example of how to organise the priority
        Lowest, // Random things like player noticing a non-important object
        Low, // Others
        Medium, // Normal 
        High, // NPC interactions
        Highest, // Cutscenes
    }

    public class Conversation : DialogueSystemDataPart
    {
        [System.Serializable]
        public class Dialogue
        {
            public DialogueSpeakerType speakerType;
            [TextArea]
            public string dialogue;
        }

        public List<Condition> conditions;

        [Tooltip(
            "[Very Low] priority will be ignored if the queue is not empty\n" +
            "[High - Low] priority will add it the the queue and sort it\n" +
            "[Very High] priority will clear the list\n")]
        public ConversationPriority priority;

        public Dialogue[] dialogues;

        public List<ModifyFactParam> modifyFacts;

        // Triggers these events after finishing all the dialogues
        public List<DialogueEvent> events;

        // 
        [Tooltip("The index at which the dialogue was stopped\n" +
        "When -1, it means the dialogue wasn't interrupted" +
        "(either haven't start conversation or finished conversation without interruption")]
        [ReadOnly]
        public int currDialogueIndex = -1;

        Conversation()
        {
            dialogues = new Dialogue[1];
            conditions = new List<Condition>();
            priority = ConversationPriority.Medium;
        }

        public Dialogue GetNextDialogue()
        {
            // If finish conversation
            if (++currDialogueIndex >= dialogues.Length)
            {
                // Reset to -1
                currDialogueIndex = -1;

                return null;
            }

            return dialogues[currDialogueIndex];
        }

        // Return to the previous dialogue, might use when dialogue was interrupted
        public void GoBackToPreviousDialogue()
        {
            --currDialogueIndex;
        }

        // Only return true if all conditions are met
        // Might want to implement OR conditions instead of only AND
        public bool CheckAllConditions()
        {
            foreach (Condition condition in conditions)
            {
                bool result = condition.GetResult();

                if (!result)
                    return false;
            }

            return true;
        }
    }
}