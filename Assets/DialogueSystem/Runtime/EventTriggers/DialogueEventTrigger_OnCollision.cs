using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    [RequireComponent(typeof(Collider2D))]
    public class DialogueEventTrigger_OnCollision : DialogueEventTrigger
    {
        private void Start()
        {
            if (GetComponent<Collider2D>() == null)
                Debug.LogError($"GameObject [{name}] doesn't have a Collider2D component");
            else if (GetComponent<Collider2D>().isTrigger)
                Debug.LogError($"GameObject [{name}]'s Collider2D.isTrigger == true");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TriggerEvent();
        }
    }

}