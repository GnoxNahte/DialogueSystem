using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class DialogueAreaCheck : MonoBehaviour
    {
        [SerializeField] public DialogueSystemData data;

        // Area check for this conversation
        // Might want to change to list
        [SerializeField] public Conversation conversation;
        //[SerializeField] public List<Conversation> conversations;

        private void Start()
        {
            if (GetComponent<Collider2D>() == null)
                Debug.LogError("DialogueAreaCheck, can't find Collider2D component");
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (conversation != DialogueSystem.CurrConversation)
                return;

            DialogueSpeaker speaker = collision.GetComponent<DialogueSpeaker>();
            if (speaker != null && speaker.inConversation)
            {
                DialogueSystem.InterruptConversation(null);
            }
        }
    }
}