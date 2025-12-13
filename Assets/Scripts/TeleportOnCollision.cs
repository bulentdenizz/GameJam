using UnityEngine;

public class TeleportOnCollision : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private Vector3 teleportPosition;
    [SerializeField] private bool useTransform = true;
    [SerializeField] private bool useCurrentPosition = false; // Teleport to player's starting position
    [SerializeField] private bool preserveRotation = false;
    
    [Header("Player Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform playerTransform;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.cyan;
    
    private Vector3 savedPlayerPosition;
    private bool hasTeleported = false;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                player = GameObject.Find("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
        }
        
        // Save initial player position if using "current position"
        if (playerTransform != null)
        {
            savedPlayerPosition = playerTransform.position;
        }
        
        // Setup collider if not present
        SetupCollider();
    }
    
    void SetupCollider()
    {
        // Check if this object has a collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // Try to get from children
            col = GetComponentInChildren<Collider>();
        }
        
        if (col == null)
        {
            // Add a box collider as trigger
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            Debug.LogWarning($"No collider found on {gameObject.name}. Added a BoxCollider as trigger. Please adjust size in inspector.");
        }
        else
        {
            // Make sure it's a trigger
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (IsPlayer(other.gameObject))
        {
            TeleportPlayer();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Also check for non-trigger collisions
        if (IsPlayer(collision.gameObject))
        {
            TeleportPlayer();
        }
    }
    
    bool IsPlayer(GameObject obj)
    {
        if (obj.CompareTag(playerTag))
        {
            return true;
        }
        
        if (playerTransform != null && obj.transform == playerTransform)
        {
            return true;
        }
        
        // Check by name
        if (obj.name == "Player" || obj.name.Contains("Player"))
        {
            return true;
        }
        
        return false;
    }
    
    void TeleportPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform not found! Cannot teleport.");
            return;
        }
        
        Vector3 targetPosition;
        
        if (useCurrentPosition)
        {
            // Teleport to player's current/starting position
            targetPosition = savedPlayerPosition;
        }
        else if (useTransform && teleportTarget != null)
        {
            targetPosition = teleportTarget.position;
        }
        else
        {
            targetPosition = teleportPosition;
        }
        
        // Teleport player
        CharacterController charController = playerTransform.GetComponent<CharacterController>();
        if (charController != null)
        {
            // Disable character controller temporarily for teleport
            charController.enabled = false;
            playerTransform.position = targetPosition;
            charController.enabled = true;
        }
        else
        {
            playerTransform.position = targetPosition;
        }
        
        // Preserve rotation if needed
        if (!preserveRotation && teleportTarget != null)
        {
            playerTransform.rotation = teleportTarget.rotation;
        }
        
        Debug.Log($"Player teleported to: {targetPosition}");
    }
    
    void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            Gizmos.color = gizmoColor;
            
            // Draw teleport target position
            Vector3 targetPos;

            if (useCurrentPosition)
            {
                targetPos = savedPlayerPosition;
            }
            else if (useTransform && teleportTarget != null)
            {
                targetPos = teleportTarget.position;
            }
            else
            {
                targetPos = teleportPosition;

            }
            
            Gizmos.DrawWireSphere(targetPos, 0.5f);
            Gizmos.DrawLine(transform.position, targetPos);
        }
    }
}

