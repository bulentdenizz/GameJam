using UnityEngine;

/// <summary>
/// Basit ışık takip scripti - LateUpdate ile player'ı takip eder
/// Multi-Aim Constraint yerine basit ve güvenilir bir çözüm
/// </summary>
public class SimpleLightFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Takip edilecek karakter. Boş bırakılırsa otomatik bulunur")]
    [SerializeField] private Transform playerTarget;
    
    [Tooltip("Karakter tag'i (otomatik bulma için)")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Follow Settings")]
    [Tooltip("Işığın player'a bakması")]
    [SerializeField] private bool lookAtPlayer = true;
    
    [Tooltip("Işığın player'ı takip etmesi (pozisyon)")]
    [SerializeField] private bool followPosition = false;
    
    [Tooltip("Pozisyon offset (eğer followPosition true ise)")]
    [SerializeField] private Vector3 positionOffset = new Vector3(0, 2f, 0);
    
    [Tooltip("Rotasyon smoothing (0 = anlık, yüksek = yumuşak)")]
    [Range(0f, 10f)]
    [SerializeField] private float rotationSmoothing = 0f;
    
    [Tooltip("Pozisyon smoothing (0 = anlık, yüksek = yumuşak)")]
    [Range(0f, 10f)]
    [SerializeField] private float positionSmoothing = 0f;
    
    [Header("Rotation Offset")]
    [Tooltip("Rotasyon offset (derece)")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;
    
    private Light lightComponent;
    private Vector3 velocity;
    private Quaternion targetRotation;
    
    void Start()
    {
        lightComponent = GetComponent<Light>();
        
        // Player'ı bul
        if (playerTarget == null)
        {
            FindPlayer();
        }
    }
    
    void LateUpdate()
    {
        if (playerTarget == null)
        {
            FindPlayer();
            return;
        }
        
        // Rotasyon takibi
        if (lookAtPlayer)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
            
            if (rotationSmoothing > 0f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / rotationSmoothing);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
        
        // Pozisyon takibi
        if (followPosition)
        {
            Vector3 targetPosition = playerTarget.position + positionOffset;
            
            if (positionSmoothing > 0f)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothing);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }
    
    void FindPlayer()
    {
        // Tag ile bul
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            return;
        }
        
        // İsim ile bul
        playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
            return;
        }
        
        // StarterAssets ThirdPersonController ile bul
        StarterAssets.ThirdPersonController controller = FindObjectOfType<StarterAssets.ThirdPersonController>();
        if (controller != null)
        {
            playerTarget = controller.transform;
        }
    }
    
    /// <summary>
    /// Player target'ını manuel olarak ayarla
    /// </summary>
    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
    }
    
    /// <summary>
    /// Işığı aç/kapa
    /// </summary>
    public void SetLightEnabled(bool enabled)
    {
        if (lightComponent != null)
        {
            lightComponent.enabled = enabled;
        }
    }
}



