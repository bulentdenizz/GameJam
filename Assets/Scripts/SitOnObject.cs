using UnityEngine;
using System.Collections;

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
    private bool wasSitPressed = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private CharacterController characterController;
    private Animator animator;
    private StarterAssets.ThirdPersonController thirdPersonController;
    private StarterAssets.FirstPersonController firstPersonController;
    private StarterAssets.StarterAssetsInputs input;
    private bool originalApplyRootMotion;
    private Coroutine positionMaintainCoroutine;
    private Vector3 targetSitPosition;
    private Quaternion targetSitRotation;
    
    // Animator parameter names
    private static readonly int IsSittingHash = Animator.StringToHash("IsSitting");
    
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();
        firstPersonController = GetComponent<StarterAssets.FirstPersonController>();
        input = GetComponent<StarterAssets.StarterAssetsInputs>();
        
        if (sitTarget == null)
        {
            Debug.LogWarning("SitOnObject: Sit Target is not assigned!");
        }
    }
    
    private void Update()
    {
        if (sitTarget == null)
        {
            Debug.LogWarning("SitOnObject: Sit Target is null!");
            return;
        }
        
        float distanceToSitTarget = Vector3.Distance(transform.position, sitTarget.position);
        
        // Check for input (either Input System or legacy Input)
        bool sitPressed = false;
        bool sitPressedThisFrame = false;
        
        // Try Input System first
        if (input != null)
        {
            sitPressed = input.sit;
            sitPressedThisFrame = sitPressed && !wasSitPressed;
            
            // Debug input state
            if (sitPressed)
            {
                Debug.Log($"Input System: sit = {input.sit}, wasSitPressed = {wasSitPressed}, sitPressedThisFrame = {sitPressedThisFrame}");
            }
        }
        
        // Fallback to legacy Input if Input System not available
        if (!sitPressedThisFrame && Input.GetKeyDown(sitKey))
        {
            sitPressed = true;
            sitPressedThisFrame = true;
            Debug.Log("Legacy Input: E key pressed");
        }
        
        // Debug distance when E is pressed
        if (sitPressedThisFrame)
        {
            Debug.Log($"E pressed! Distance to sitTarget: {distanceToSitTarget}, sitDistance: {sitDistance}, isSitting: {isSitting}, input != null: {input != null}");
        }
        
        // Check if player is close enough and pressed sit key
        if (distanceToSitTarget <= sitDistance && sitPressedThisFrame)
        {
            Debug.Log($"Sitting triggered! Distance: {distanceToSitTarget}");
            if (!isSitting)
            {
                SitDown();
            }
            else
            {
                StandUp();
            }
        }
        else if (sitPressedThisFrame && distanceToSitTarget > sitDistance)
        {
            Debug.Log($"E pressed but too far! Distance: {distanceToSitTarget}, Required: {sitDistance}");
        }
        
        wasSitPressed = sitPressed;
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
        
        // Disable character controller temporarily
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Disable root motion to prevent animation from moving character
        if (animator != null)
        {
            originalApplyRootMotion = animator.applyRootMotion;
            animator.applyRootMotion = false;
        }
        
        // Get world position of chair (handles parent transforms)
        Vector3 chairWorldPosition = sitTarget.position;
        Vector3 chairForward = sitTarget.forward;
        
        // Calculate sit position - on top of chair seat
        Vector3 sitPosition = chairWorldPosition;
        sitPosition += chairForward * 0.1f; // Move slightly in front of chair center
        sitPosition.y = chairWorldPosition.y + 0.6f; // Adjust height to sit on chair (chair seat is around y+0.5)
        
        // Calculate rotation - face opposite of chair forward (sit facing forward)
        Quaternion sitRotation = Quaternion.LookRotation(-chairForward);
        
        // Store target position and rotation
        targetSitPosition = sitPosition;
        targetSitRotation = sitRotation;
        
        // Set position and rotation immediately
        transform.position = sitPosition;
        transform.rotation = sitRotation;
        
        // Start coroutine to maintain position during animation
        if (positionMaintainCoroutine != null)
        {
            StopCoroutine(positionMaintainCoroutine);
        }
        positionMaintainCoroutine = StartCoroutine(MaintainSitPosition(sitPosition, sitRotation));
        
        Debug.Log($"SitDown: Moving to position {sitPosition}, chair position: {chairWorldPosition}, chair forward: {chairForward}");
        
        // Trigger sitting animation AFTER position is set
        if (animator != null && animator.enabled)
        {
            // Check if parameter exists
            bool hasParameter = false;
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "IsSitting")
                {
                    hasParameter = true;
                    break;
                }
            }
            
            if (hasParameter)
            {
                // Set the parameter
                animator.SetBool(IsSittingHash, true);
                
                // Force update animator
                animator.Update(0);
                
                Debug.Log($"SitDown: IsSitting set to true. Animator enabled: {animator.enabled}, Current State: {GetCurrentAnimatorStateName()}");
            }
            else
            {
                Debug.LogError("SitOnObject: Animator does not have 'IsSitting' parameter!");
            }
        }
        else
        {
            Debug.LogWarning($"SitOnObject: Animator is null or disabled! animator={animator}, enabled={(animator != null ? animator.enabled.ToString() : "N/A")}");
        }
    }
    
    private void StandUp()
    {
        if (!isSitting) return;
        
        isSitting = false;
        
        // Stop position maintenance coroutine
        if (positionMaintainCoroutine != null)
        {
            StopCoroutine(positionMaintainCoroutine);
            positionMaintainCoroutine = null;
        }
        
        // Restore root motion setting
        if (animator != null)
        {
            animator.applyRootMotion = originalApplyRootMotion;
        }
        
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
            Debug.Log("StandUp: IsSitting set to false");
        }
        else
        {
            Debug.LogWarning("SitOnObject: Animator is null!");
        }
    }
    
    private IEnumerator MaintainSitPosition(Vector3 targetPosition, Quaternion targetRotation)
    {
        while (isSitting)
        {
            // Continuously maintain position and rotation
            // Use LateUpdate timing to override any animation root motion
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            
            // Also disable root motion every frame to be sure
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
            
            yield return null;
        }
    }
    
    private void LateUpdate()
    {
        // In LateUpdate, override any root motion that might have been applied
        if (isSitting)
        {
            // Force position and rotation
            transform.position = targetSitPosition;
            transform.rotation = targetSitRotation;
            
            // Ensure root motion is disabled
            if (animator != null)
            {
                animator.applyRootMotion = false;
            }
        }
    }
    
    private string GetCurrentAnimatorStateName()
    {
        if (animator != null && animator.enabled)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.IsName("Stand To Sit") ? "Stand To Sit" : 
                   stateInfo.IsName("Sit To Stand") ? "Sit To Stand" : 
                   stateInfo.IsName("Sitting Idle") ? "Sitting Idle" : 
                   stateInfo.IsName("Idle Walk Run Blend") ? "Idle Walk Run Blend" : "Unknown";
        }
        return "Animator null or disabled";
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

