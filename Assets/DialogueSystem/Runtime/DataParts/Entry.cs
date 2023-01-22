using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GnoxNahte.DialogueSystem.Runtime
{
    public class Entry : DialogueSystemDataPart
    {
        public List<Fact> facts;
        public List<DialogueEvent> events;
        public List<Conversation> conversations;

        Entry()
        {
            facts = new List<Fact>();
            events = new List<DialogueEvent>();
            conversations = new List<Conversation>();
        }
    }
}