using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GnoxNahte.DialogueSystem.Runtime
{
    public class Fact : DialogueSystemDataPart
    {

        [ReadOnly]
        public int value;
        public int startingValue;
    }

    [System.Serializable]
    public class ModifyFactParam
    {
        public enum ModifyFactType
        {
            Set,
            Add,
            Subtract,
            Multiply,
            Divide,
        }
        public Fact fact;
        public ModifyFactType type;

        // If modifyType == Set, fact.value = modifyValue
        // If modifyType == add, fact.value += modifyValue
        // etc...
        public int modifyValue;

        public void Modify()
        {
            switch (type)
            {
                case ModifyFactType.Set: fact.value = modifyValue; break;
                case ModifyFactType.Add: fact.value += modifyValue; break;
                case ModifyFactType.Subtract: fact.value -= modifyValue; break;
                case ModifyFactType.Multiply: fact.value *= modifyValue; break;
                case ModifyFactType.Divide: fact.value /= modifyValue; break;
                default:
                    Debug.LogError($"ModifyFactType: [{type}] is not implemented");
                    break;
            }
        }
    }
}