using TMPro;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class Interaction : MonoBehaviour
{
    public float distance;
    public LayerMask mask;
    RaycastHit hitInfo;
    Transform cam;

    private void Awake()
    {
        cam = Camera.main.transform;
    }
    public void InteractStarted(CallbackContext callbackContext)
    {
        if (callbackContext.started)
        {
            if (Physics.Raycast(cam.position, cam.forward, out hitInfo, distance, mask))
            {
                (hitInfo.collider.GetComponent<MonoBehaviour>() as IInteract)?.Interact();
            }
        }
    }


#if UNITY_EDITOR
    [SerializeField] TextMeshProUGUI textMeshPro;
    private void Update()
    {
        if (Physics.Raycast(cam.position, cam.forward, out hitInfo, distance, mask))
        {
            if (hitInfo.collider.GetComponent<MonoBehaviour>() as IInteract is not null)
            {
                textMeshPro.gameObject.SetActive(true);
                textMeshPro.SetText(hitInfo.collider.gameObject.name);
            }
            else if (textMeshPro.gameObject.activeSelf)
            {
                textMeshPro.gameObject.SetActive(false);
            }
        }
        else if (textMeshPro.gameObject.activeSelf)
        {
            textMeshPro.gameObject.SetActive(false);
        }
    }
#endif
    private void OnDrawGizmos()
    {
        if (cam != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(cam.position, cam.forward * distance);
        }
    }
}