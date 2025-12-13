using UnityEngine;
using UnityEngine.InputSystem;

public class RayInteractor : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    PlayerInput playerInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!playerInput.enabled)
            {
                playerInput.enabled = true;
            }
            else
            {
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
                {
                    if (hit.collider.CompareTag("Saat"))
                    {
                        playerInput.enabled = false;
                    }
                }
            }
        }
    }
}
