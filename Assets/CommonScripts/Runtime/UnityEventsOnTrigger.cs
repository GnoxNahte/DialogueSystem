using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnityEventsOnTrigger : MonoBehaviour
{
    [SerializeField]
    UnityEngine.Events.UnityEvent unityEvent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        unityEvent?.Invoke();
    }
}
