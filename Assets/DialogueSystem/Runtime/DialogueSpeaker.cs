using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public enum DialogueSpeakerType
    {
        Player,
        Narrator,
        Merchant,
        NPC,
        Enemy,
    }

    public class DialogueSpeaker : MonoBehaviour
    {
        public DialogueSpeakerType type;

        [SerializeField] SpeechBubble speechBubble;

        [Tooltip("Says this when the speaker is interrupted")]
        [field: SerializeField]
        public string InterruptedDialogue { get; private set; } = "Wait, I haven't finished";

        [Tooltip("Says this when the speaker continues after being interrupted")]
        [field: SerializeField]
        public string CoutinueDialogue { get; private set; } = "As I was saying,";

        [ReadOnly]
        public bool inConversation;

        public bool IsAnimatingSpeechBubble => speechBubble.IsAnimating;

        private void OnEnable()
        {
            DialogueSystem.AddSpeaker(this);
        }
        private void OnDisable()
        {
            DialogueSystem.RemoveSpeaker(this);
        }

        public IEnumerator SayLine(string dialogue)
        {
            return speechBubble.SayLine(dialogue);
        }

        public IEnumerator CloseSpeechBubble()
        {
            return speechBubble.Close();
        }

        public IEnumerator WaitForSpeechBubbleAnimation()
        {
            return speechBubble.AllAnimationDoneCheck();
        }

        public void StopSpeechBubbleAnimations()
        {
            // TODO (GnoxNahte): Wasn't null check before
            speechBubble.StopAllAnimations();
        }

        private void OnValidate()
        {
            name = type.ToString();
        }
    }
}