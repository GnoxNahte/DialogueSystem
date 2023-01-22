using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class DialogueEventTrigger_OnTrigger : DialogueEventTrigger
    {
        private void Start()
        {
            if (GetComponent<Collider2D>() == null)
                Debug.LogError($"GameObject [{name}] doesn't have a Collider2D component");
            else if (!GetComponent<Collider2D>().isTrigger)
                Debug.LogError($"GameObject [{name}]'s Collider2D.isTrigger == false");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            TriggerEvent();
        }
    }
}