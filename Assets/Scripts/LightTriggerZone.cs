using UnityEngine;

/// <summary>
/// Karakter belirli pozisyona geldiğinde ışıkları açıp kapatan script
/// Trigger zone veya pozisyon kontrolü ile çalışır
/// </summary>
public class LightTriggerZone : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Takip edilecek karakter. Boş bırakılırsa otomatik bulunur")]
    [SerializeField] private Transform playerTarget;
    
    [Tooltip("Karakter tag'i")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Zone Settings")]
    [Tooltip("Trigger zone kullan (Collider ile) veya manuel pozisyon kontrolü")]
    [SerializeField] private bool useTriggerZone = true;
    
    [Tooltip("Zone merkezi (trigger kullanmıyorsanız)")]
    [SerializeField] private Vector3 zoneCenter = Vector3.zero;
    
    [Tooltip("Zone boyutu (trigger kullanmıyorsanız)")]
    [SerializeField] private Vector3 zoneSize = new Vector3(5f, 5f, 5f);
    
    [Header("Light Control")]
    [Tooltip("Kontrol edilecek ışıklar listesi")]
    [SerializeField] private Light[] lightsToControl;
    
    [Tooltip("Otomatik olarak sahnedeki tüm ışıkları bul")]
    [SerializeField] private bool autoFindAllLights = false;
    
    [Tooltip("Sadece belirli tag'e sahip ışıkları bul")]
    [SerializeField] private string lightTag = "";
    
    [Header("States")]
    [Tooltip("Bu zone'da iken ışıklar açık mı?")]
    [SerializeField] private bool lightsOnInZone = true;
    
    [Tooltip("Zone dışında iken ışıklar kapalı mı?")]
    [SerializeField] private bool lightsOffOutside = false;
    
    [Header("Debug")]
    [Tooltip("Zone'u görsel olarak göster")]
    [SerializeField] private bool showGizmos = true;
    
    [Tooltip("Gizmo rengi")]
    [SerializeField] private Color gizmoColor = new Color(1f, 1f, 0f, 0.3f);
    
    private bool playerInZone = false;
    private Collider triggerCollider;
    
    void Start()
    {
        // Player'ı bul
        if (playerTarget == null)
        {
            FindPlayer();
        }
        
        // Işıkları bul
        if (lightsToControl == null || lightsToControl.Length == 0 || autoFindAllLights)
        {
            FindLights();
        }
        
        // Trigger collider'ı ayarla
        if (useTriggerZone)
        {
            SetupTriggerZone();
        }
        
        // Başlangıç durumunu ayarla (player zone dışında olduğu varsayılır)
        if (!playerInZone && lightsOffOutside)
        {
            UpdateLights();
        }
    }
    
    void SetupTriggerZone()
    {
        triggerCollider = GetComponent<Collider>();
        
        // Eğer collider yoksa BoxCollider ekle
        if (triggerCollider == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = zoneSize;
            boxCollider.center = zoneCenter;
            triggerCollider = boxCollider;
        }
        else
        {
            // Mevcut collider'ı trigger yap
            triggerCollider.isTrigger = true;
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
    
    void FindLights()
    {
        if (!string.IsNullOrEmpty(lightTag))
        {
            // Tag ile ışıkları bul
            GameObject[] lightObjects = GameObject.FindGameObjectsWithTag(lightTag);
            lightsToControl = new Light[lightObjects.Length];
            for (int i = 0; i < lightObjects.Length; i++)
            {
                lightsToControl[i] = lightObjects[i].GetComponent<Light>();
            }
        }
        else
        {
            // Tüm ışıkları bul
            lightsToControl = FindObjectsOfType<Light>();
        }
        
        Debug.Log($"LightTriggerZone: {lightsToControl.Length} ışık bulundu.");
    }
    
    void Update()
    {
        // Trigger zone kullanmıyorsa manuel pozisyon kontrolü yap
        if (!useTriggerZone && playerTarget != null)
        {
            CheckPosition();
        }
    }
    
    void CheckPosition()
    {
        Vector3 playerPos = playerTarget.position;
        Vector3 worldZoneCenter = transform.position + zoneCenter;
        
        // AABB (Axis-Aligned Bounding Box) kontrolü
        bool wasInZone = playerInZone;
        playerInZone = 
            playerPos.x >= worldZoneCenter.x - zoneSize.x * 0.5f &&
            playerPos.x <= worldZoneCenter.x + zoneSize.x * 0.5f &&
            playerPos.y >= worldZoneCenter.y - zoneSize.y * 0.5f &&
            playerPos.y <= worldZoneCenter.y + zoneSize.y * 0.5f &&
            playerPos.z >= worldZoneCenter.z - zoneSize.z * 0.5f &&
            playerPos.z <= worldZoneCenter.z + zoneSize.z * 0.5f;
        
        // Durum değiştiğinde ışıkları güncelle
        if (wasInZone != playerInZone)
        {
            UpdateLights();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!useTriggerZone) return;
        
        if (IsPlayer(other.gameObject))
        {
            playerInZone = true;
            UpdateLights();
            Debug.Log("LightTriggerZone: Player zone'a girdi.");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!useTriggerZone) return;
        
        if (IsPlayer(other.gameObject))
        {
            playerInZone = false;
            UpdateLights();
            Debug.Log("LightTriggerZone: Player zone'dan çıktı.");
        }
    }
    
    bool IsPlayer(GameObject obj)
    {
        if (playerTarget != null && obj.transform == playerTarget)
        {
            return true;
        }
        
        if (obj.CompareTag(playerTag))
        {
            return true;
        }
        
        if (obj.name == "Player" || obj.name.Contains("Player"))
        {
            return true;
        }
        
        return false;
    }
    
    void UpdateLights()
    {
        if (lightsToControl == null) return;
        
        bool shouldBeOn;
        
        if (playerInZone)
        {
            // Zone içindeyken
            shouldBeOn = lightsOnInZone;
        }
        else
        {
            // Zone dışındayken
            shouldBeOn = !lightsOffOutside; // lightsOffOutside true ise false, false ise true
        }
        
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                light.enabled = shouldBeOn;
            }
        }
        
        Debug.Log($"LightTriggerZone: Player {(playerInZone ? "zone içinde" : "zone dışında")}, Işıklar {(shouldBeOn ? "açık" : "kapalı")}.");
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        Vector3 center = useTriggerZone && triggerCollider != null 
            ? transform.position + triggerCollider.bounds.center 
            : transform.position + zoneCenter;
        
        Vector3 size = useTriggerZone && triggerCollider != null 
            ? triggerCollider.bounds.size 
            : zoneSize;
        
        Gizmos.DrawWireCube(center, size);
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a * 0.2f);
        Gizmos.DrawCube(center, size);
    }
    
    /// <summary>
    /// Işıkları manuel olarak aç
    /// </summary>
    [ContextMenu("Işıkları Aç")]
    public void TurnLightsOn()
    {
        if (lightsToControl == null) return;
        
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                light.enabled = true;
            }
        }
    }
    
    /// <summary>
    /// Işıkları manuel olarak kapat
    /// </summary>
    [ContextMenu("Işıkları Kapat")]
    public void TurnLightsOff()
    {
        if (lightsToControl == null) return;
        
        foreach (Light light in lightsToControl)
        {
            if (light != null)
            {
                light.enabled = false;
            }
        }
    }
    
    /// <summary>
    /// Işıkları manuel olarak ekle
    /// </summary>
    public void AddLight(Light light)
    {
        if (light == null) return;
        
        // Mevcut listeye ekle
        System.Collections.Generic.List<Light> lightList = new System.Collections.Generic.List<Light>();
        if (lightsToControl != null)
        {
            lightList.AddRange(lightsToControl);
        }
        
        if (!lightList.Contains(light))
        {
            lightList.Add(light);
            lightsToControl = lightList.ToArray();
        }
    }
}

