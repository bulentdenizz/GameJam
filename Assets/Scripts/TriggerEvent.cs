using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent OnTriggerEnterEvent;
    public UnityEvent OnTriggerExitEvent;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterEvent?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitEvent?.Invoke();
    }
}
