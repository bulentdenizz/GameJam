using UnityEngine;

public class Door : MonoBehaviour, IInteract
{
    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void Interact()
    {
        animator.Play("Open");
    }
}
