using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class ModifyFact : MonoBehaviour
    {
        [SerializeField] protected DialogueSystemData dialogueSystemData = null;
        [SerializeField] protected List<ModifyFactParam> modifyFactParams = null;

        public void Modify()
        {
            DialogueSystem.ModifyFacts(modifyFactParams);
        }

        private void Reset()
        {
            dialogueSystemData = FindObjectOfType<DialogueSystem>().Data;
        }
    }
}