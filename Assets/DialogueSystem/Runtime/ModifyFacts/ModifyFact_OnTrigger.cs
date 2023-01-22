using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class ModifyFact_OnTrigger : ModifyFact
    {
        private void Start()
        {
            if (GetComponent<Collider2D>() == null)
            {
                Debug.LogError($"GameObject [{name}] doesn't have a Collider2D component");
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Modify();
        }
    }
}