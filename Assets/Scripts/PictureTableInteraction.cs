using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PictureTableInteraction : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator pictureAnimator;
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private bool allowMultiplePlays = true;
    
    [Header("Animation Settings")]
    [SerializeField] private float pictureAnimationDuration = 0.25f;
    [SerializeField] private string doorAnimationName = "Door_open";
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    
    private bool isPlayerNearby = false;
    private bool hasPlayedAnimation = false;
    private float originalAnimatorSpeed = 0.1f;
    
#if ENABLE_INPUT_SYSTEM
    private InputAction interactAction;
    private InputActionAsset inputActionAsset;
    private PlayerInput playerInput;
#endif

    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                // Try to find by name
                player = GameObject.Find("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
        }
        
        // Get animator if not assigned
        if (pictureAnimator == null)
        {
            pictureAnimator = GetComponent<Animator>();
            if (pictureAnimator == null)
            {
                // Try to find picture in children
                Transform pictureTransform = transform.Find("Picture");
                if (pictureTransform != null)
                {
                    pictureAnimator = pictureTransform.GetComponent<Animator>();
                }
            }
        }
        
        // Store original animator speed
        if (pictureAnimator != null)
        {
            originalAnimatorSpeed = pictureAnimator.speed;
        }
        
        // Find door animator if not assigned
        if (doorAnimator == null)
        {
            GameObject door = GameObject.Find("Door");
            if (door != null)
            {
                doorAnimator = door.GetComponent<Animator>();
                if (doorAnimator == null)
                {
                    // Try to find in children
                    doorAnimator = door.GetComponentInChildren<Animator>();
                }
            }
        }
        
#if ENABLE_INPUT_SYSTEM
        // Try to get InputActionAsset from PlayerInput component
        if (playerTransform != null)
        {
            playerInput = playerTransform.GetComponent<PlayerInput>();
            if (playerInput != null && playerInput.actions != null)
            {
                inputActionAsset = playerInput.actions;
                interactAction = inputActionAsset.FindAction("Player/Interact");
                if (interactAction != null)
                {
                    interactAction.Enable();
                }
            }
        }
        
        // Fallback: Try to load the InputActionAsset directly
        if (interactAction == null)
        {
            inputActionAsset = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActionAsset == null)
            {
                // Try loading from Assets
                inputActionAsset = UnityEngine.Resources.Load<InputActionAsset>("Assets/InputSystem_Actions");
            }
            
            if (inputActionAsset != null)
            {
                interactAction = inputActionAsset.FindAction("Player/Interact");
                if (interactAction != null)
                {
                    interactAction.Enable();
                }
            }
        }
#endif
    }

    void Update()
    {
        if (playerTransform == null || pictureAnimator == null)
        {
            return;
        }
        
        // Calculate distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        isPlayerNearby = distance <= interactionDistance;
        
        // Check for interaction input
        bool interactPressed = false;
        
#if ENABLE_INPUT_SYSTEM
        // Use new Input System
        if (interactAction != null)
        {
            interactPressed = interactAction.WasPressedThisFrame();
        }
        else
        {
            // Fallback to keyboard
            interactPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        }
#else
        // Use old Input System
        interactPressed = Input.GetKeyDown(KeyCode.E);
#endif
        
        // If player is nearby and presses E, play animation
        if (isPlayerNearby && interactPressed)
        {
            if (allowMultiplePlays || !hasPlayedAnimation)
            {
                PlayPictureAnimation();
            }
        }
    }
    
    void PlayPictureAnimation()
    {
        if (pictureAnimator != null)
        {
            // Reset and play the animation at normal speed
            pictureAnimator.speed = 1f;
            pictureAnimator.Play("Picture", 0, 0f);
            
            hasPlayedAnimation = true;
            Debug.Log("Picture animation played!");
            
            // Start coroutine to wait for picture animation to finish, then play door animation
            StartCoroutine(WaitForPictureAnimationThenPlayDoor());
        }
    }
    
    IEnumerator WaitForPictureAnimationThenPlayDoor()
    {
        // Wait for picture animation to finish
        yield return new WaitForSeconds(pictureAnimationDuration);
        
        // Play door animation
        if (doorAnimator != null)
        {
            doorAnimator.Play(doorAnimationName, 0, 0f);
            Debug.Log("Door animation played!");
        }
        else
        {
            Debug.LogWarning("Door animator not found! Please assign it in the inspector.");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            Gizmos.color = isPlayerNearby ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionDistance);
        }
    }
    
    void OnDestroy()
    {
#if ENABLE_INPUT_SYSTEM
        if (interactAction != null)
        {
            interactAction.Disable();
        }
#endif
    }
}

