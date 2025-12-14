using UnityEngine;

/// <summary>
/// Karakter belirli bir noktaya geldiğinde component'leri açıp kapatan script
/// </summary>
public class ComponentToggleZone : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("Player tag'i (varsayılan: Player)")]
    [SerializeField] private string playerTag = "Player";
    
    [Tooltip("Player GameObject'i (otomatik bulunur)")]
    [SerializeField] private GameObject playerObject;
    
    [Header("Components to Disable")]
    [Tooltip("Kapatılacak component'ler (karakter zone'a girdiğinde)")]
    [SerializeField] private MonoBehaviour[] componentsToDisable;
    
    [Header("GameObjects to Disable")]
    [Tooltip("Kapatılacak GameObject'ler (karakter zone'a girdiğinde)")]
    [SerializeField] private GameObject[] gameObjectsToDisable;
    
    [Tooltip("Tag ile otomatik olarak GameObject'leri bul ve kapat")]
    [SerializeField] private string disableGameObjectTag = "";
    
    [Tooltip("Belirli isimlerle GameObject'leri otomatik bul")]
    [SerializeField] private string[] disableGameObjectNames;
    
    [Tooltip("Bu GameObject'i de kapat (zone GameObject'i)")]
    [SerializeField] private bool disableThisGameObject = false;
    
    [Header("Components to Enable")]
    [Tooltip("Açılacak component'ler (karakter zone'a girdiğinde)")]
    [SerializeField] private MonoBehaviour[] componentsToEnable;
    
    [Tooltip("Açılacak GameObject'ler (karakter zone'a girdiğinde)")]
    [SerializeField] private GameObject[] gameObjectsToEnable;
    
    [Header("Zone Settings")]
    [Tooltip("Zone'dan çıkınca eski haline dönsün mü?")]
    [SerializeField] private bool resetOnExit = false;
    
    [Tooltip("Sadece bir kez tetiklensin mi?")]
    [SerializeField] private bool triggerOnce = false;
    
    [Header("Debug")]
    [Tooltip("Gizmos gösterilsin mi?")]
    [SerializeField] private bool showGizmos = true;
    
    [Tooltip("Gizmo rengi")]
    [SerializeField] private Color gizmoColor = new Color(0, 1, 0, 0.3f);
    
    private bool hasTriggered = false;
    private bool playerInZone = false;
    
    // Zone'dan çıkınca eski haline döndürmek için saklanan durumlar
    private bool[] componentDisabledStates;
    private bool[] gameObjectDisabledStates;
    
    void Start()
    {
        // Player'ı bul
        if (playerObject == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                playerObject = player;
            }
            else
            {
                player = GameObject.Find("Player");
                if (player != null)
                {
                    playerObject = player;
                }
            }
        }
        
        // Collider kontrolü
        SetupCollider();
        
        // Tag ve isim ile GameObject'leri bul
        FindGameObjectsByTagAndName();
        
        // Başlangıç durumlarını kaydet
        SaveInitialStates();
    }
    
    void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = GetComponentInChildren<Collider>();
        }
        
        if (col == null)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            Debug.LogWarning($"{gameObject.name}: Collider bulunamadı. BoxCollider eklendi. Boyutunu Inspector'da ayarlayın.");
        }
        else
        {
            col.isTrigger = true;
        }
    }
    
    void FindGameObjectsByTagAndName()
    {
        System.Collections.Generic.List<GameObject> foundObjects = new System.Collections.Generic.List<GameObject>();
        
        // Mevcut listedeki GameObject'leri ekle
        if (gameObjectsToDisable != null && gameObjectsToDisable.Length > 0)
        {
            foreach (GameObject obj in gameObjectsToDisable)
            {
                if (obj != null && !foundObjects.Contains(obj))
                {
                    foundObjects.Add(obj);
                }
            }
        }
        
        // Tag ile bulunan GameObject'leri ekle
        if (!string.IsNullOrEmpty(disableGameObjectTag))
        {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(disableGameObjectTag);
            foreach (GameObject obj in taggedObjects)
            {
                if (obj != null && !foundObjects.Contains(obj))
                {
                    foundObjects.Add(obj);
                }
            }
        }
        
        // İsim ile bulunan GameObject'leri ekle
        if (disableGameObjectNames != null && disableGameObjectNames.Length > 0)
        {
            foreach (string objName in disableGameObjectNames)
            {
                if (!string.IsNullOrEmpty(objName))
                {
                    GameObject obj = GameObject.Find(objName);
                    if (obj != null && !foundObjects.Contains(obj))
                    {
                        foundObjects.Add(obj);
                    }
                }
            }
        }
        
        // Eğer bu GameObject'i de kapatacaksak ekle
        if (disableThisGameObject && !foundObjects.Contains(gameObject))
        {
            foundObjects.Add(gameObject);
        }
        
        // Listeyi güncelle
        if (foundObjects.Count > 0)
        {
            gameObjectsToDisable = foundObjects.ToArray();
            Debug.Log($"{gameObject.name}: {foundObjects.Count} GameObject disable listesine eklendi.");
        }
    }
    
    void SaveInitialStates()
    {
        // Component durumlarını kaydet
        if (componentsToDisable != null && componentsToDisable.Length > 0)
        {
            componentDisabledStates = new bool[componentsToDisable.Length];
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                if (componentsToDisable[i] != null)
                {
                    componentDisabledStates[i] = componentsToDisable[i].enabled;
                }
            }
        }
        
        // GameObject durumlarını kaydet
        if (gameObjectsToDisable != null && gameObjectsToDisable.Length > 0)
        {
            gameObjectDisabledStates = new bool[gameObjectsToDisable.Length];
            for (int i = 0; i < gameObjectsToDisable.Length; i++)
            {
                if (gameObjectsToDisable[i] != null)
                {
                    gameObjectDisabledStates[i] = gameObjectsToDisable[i].activeSelf;
                }
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (IsPlayer(other.gameObject))
        {
            OnPlayerEnter();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (IsPlayer(other.gameObject))
        {
            OnPlayerExit();
        }
    }
    
    bool IsPlayer(GameObject obj)
    {
        if (obj.CompareTag(playerTag))
        {
            return true;
        }
        
        if (playerObject != null && obj == playerObject)
        {
            return true;
        }
        
        if (obj.name == "Player" || obj.name.Contains("Player"))
        {
            return true;
        }
        
        return false;
    }
    
    void OnPlayerEnter()
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        playerInZone = true;
        hasTriggered = true;
        
        // Component'leri kapat
        DisableComponents();
        
        // GameObject'leri kapat
        DisableGameObjects();
        
        // Component'leri aç
        EnableComponents();
        
        // GameObject'leri aç
        EnableGameObjects();
        
        Debug.Log($"{gameObject.name}: Player zone'a girdi. Component'ler güncellendi.");
    }
    
    void OnPlayerExit()
    {
        if (!resetOnExit)
        {
            return;
        }
        
        playerInZone = false;
        
        // Component'leri eski haline döndür
        RestoreComponents();
        
        // GameObject'leri eski haline döndür
        RestoreGameObjects();
        
        Debug.Log($"{gameObject.name}: Player zone'dan çıktı. Component'ler eski haline döndürüldü.");
    }
    
    void DisableComponents()
    {
        if (componentsToDisable == null) return;
        
        foreach (MonoBehaviour component in componentsToDisable)
        {
            if (component != null)
            {
                component.enabled = false;
            }
        }
    }
    
    void DisableGameObjects()
    {
        if (gameObjectsToDisable == null) return;
        
        foreach (GameObject obj in gameObjectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
    
    void EnableComponents()
    {
        if (componentsToEnable == null) return;
        
        foreach (MonoBehaviour component in componentsToEnable)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }
    }
    
    void EnableGameObjects()
    {
        if (gameObjectsToEnable == null) return;
        
        foreach (GameObject obj in gameObjectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }
    
    void RestoreComponents()
    {
        if (componentsToDisable == null || componentDisabledStates == null) return;
        
        for (int i = 0; i < componentsToDisable.Length && i < componentDisabledStates.Length; i++)
        {
            if (componentsToDisable[i] != null)
            {
                componentsToDisable[i].enabled = componentDisabledStates[i];
            }
        }
    }
    
    void RestoreGameObjects()
    {
        if (gameObjectsToDisable == null || gameObjectDisabledStates == null) return;
        
        for (int i = 0; i < gameObjectsToDisable.Length && i < gameObjectDisabledStates.Length; i++)
        {
            if (gameObjectsToDisable[i] != null)
            {
                gameObjectsToDisable[i].SetActive(gameObjectDisabledStates[i]);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Gizmos.color = gizmoColor;
        
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = GetComponentInChildren<Collider>();
        }
        
        if (col != null)
        {
            if (col is BoxCollider boxCol)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(boxCol.center, boxCol.size);
            }
            else if (col is SphereCollider sphereCol)
            {
                Gizmos.DrawSphere(transform.position + sphereCol.center, sphereCol.radius);
            }
        }
        else
        {
            // Varsayılan box gizmo
            Gizmos.DrawCube(transform.position, Vector3.one);
        }
    }
}

