using UnityEngine;

public class SitOnObject : MonoBehaviour
{
    [Header("Sitting Settings")]
    [Tooltip("The object the player will sit on")]
    public Transform sitTarget;
    
    [Tooltip("Distance to check for sitting")]
    public float sitDistance = 2f;
    
    [Tooltip("Key to press to sit/stand")]
    public KeyCode sitKey = KeyCode.E;
    
    private bool isSitting = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private CharacterController characterController;
    private Animator animator;
    private StarterAssets.ThirdPersonController thirdPersonController;
    private StarterAssets.FirstPersonController firstPersonController;
    
    // Animator parameter names
    private static readonly int IsSittingHash = Animator.StringToHash("IsSitting");
    
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        firstPersonController = GetComponent<StarterAssets.FirstPersonController>();
        
        if (sitTarget == null)
        {
            Debug.LogWarning("SitOnObject: Sit Target is not assigned!");
        }
    }
    
    private void Update()
    {
        if (sitTarget == null) return;
        
        float distanceToSitTarget = Vector3.Distance(transform.position, sitTarget.position);
        
        // Check if player is close enough and pressed sit key
        if (distanceToSitTarget <= sitDistance && Input.GetKeyDown(sitKey))
        {
            if (!isSitting)
            {
                SitDown();
            }
            else
            {
                StandUp();
            }
        }
    }
    
    private void SitDown()
    {
        if (isSitting) return;
        
        isSitting = true;
        
        // Save original position and rotation
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Disable character controller movement
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = false;
        }
        if (firstPersonController != null)
        {
            firstPersonController.enabled = false;
        }
        
        // Move character to sit position
        Vector3 sitPosition = sitTarget.position;
        sitPosition.y += 0.5f; // Adjust height offset
        
        // Rotate character to face forward from sit target
        transform.position = sitPosition;
        transform.rotation = sitTarget.rotation;
        
        // Disable character controller temporarily
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Trigger sitting animation
        if (animator != null)
        {
            animator.SetBool(IsSittingHash, true);
        }
    }
    
    private void StandUp()
    {
        if (!isSitting) return;
        
        isSitting = false;
        
        // Enable character controller
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Move character back to original position
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        
        // Enable third person controller
        if (thirdPersonController != null)
        {
            thirdPersonController.enabled = true;
        }
        if (firstPersonController != null)
        {
            firstPersonController.enabled = true;
        }
        
        // Stop sitting animation
        if (animator != null)
        {
            animator.SetBool(IsSittingHash, false);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (sitTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(sitTarget.position, sitDistance);
        }
    }
}

