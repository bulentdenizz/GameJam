using UnityEngine;

/// <summary>
/// Controller script to manage fog in a specific area.
/// Fog will be active when player/object is inside the trigger area.
/// </summary>
[RequireComponent(typeof(Collider))]
public class FogController : MonoBehaviour
{
    [Header("Fog Settings")]
    [Tooltip("Fog color")]
    public Color fogColor = new Color(0.5f, 0.5f, 0.6f, 1.0f);
    
    [Tooltip("Fog mode: Linear or Exponential")]
    public FogMode fogMode = FogMode.Linear;
    
    [Header("Linear Fog Settings")]
    [Tooltip("Distance where fog starts (Linear mode only)")]
    public float fogStartDistance = 0.0f;
    
    [Tooltip("Distance where fog ends (Linear mode only)")]
    public float fogEndDistance = 30.0f;
    
    [Header("Exponential Fog Settings")]
    [Tooltip("Fog density (Exponential mode only)")]
    [Range(0.0f, 0.1f)]
    public float fogDensity = 0.01f;
    
    [Header("Area Settings")]
    [Tooltip("Tag of objects that trigger fog (leave empty for any object)")]
    public string triggerTag = "Player";
    
    [Tooltip("Show fog area gizmo in scene view")]
    public bool showGizmo = true;
    
    [Tooltip("Gizmo color")]
    public Color gizmoColor = new Color(0.5f, 0.5f, 0.6f, 0.3f);
    
    private bool isFogActive = false;
    private Collider fogCollider;
    private Color originalFogColor;
    private FogMode originalFogMode;
    private float originalFogStartDistance;
    private float originalFogEndDistance;
    private float originalFogDensity;
    private bool originalFogEnabled;
    
    private void Start()
    {
        // Get collider component
        fogCollider = GetComponent<Collider>();
        
        // Make sure collider is a trigger
        fogCollider.isTrigger = true;
        
        // Save original fog settings
        SaveOriginalFogSettings();
        
        // Disable fog initially
        RenderSettings.fog = false;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if trigger tag matches or if no tag is specified
        if (string.IsNullOrEmpty(triggerTag) || other.CompareTag(triggerTag))
        {
            ActivateFog();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if trigger tag matches or if no tag is specified
        if (string.IsNullOrEmpty(triggerTag) || other.CompareTag(triggerTag))
        {
            DeactivateFog();
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        // Keep fog active while inside trigger
        if (!isFogActive && (string.IsNullOrEmpty(triggerTag) || other.CompareTag(triggerTag)))
        {
            ActivateFog();
        }
    }
    
    /// <summary>
    /// Saves the original fog settings before applying area fog
    /// </summary>
    private void SaveOriginalFogSettings()
    {
        originalFogEnabled = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogMode = RenderSettings.fogMode;
        originalFogStartDistance = RenderSettings.fogStartDistance;
        originalFogEndDistance = RenderSettings.fogEndDistance;
        originalFogDensity = RenderSettings.fogDensity;
    }
    
    /// <summary>
    /// Activates fog with the specified settings
    /// </summary>
    private void ActivateFog()
    {
        if (!isFogActive)
        {
            isFogActive = true;
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            
            if (fogMode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = fogStartDistance;
                RenderSettings.fogEndDistance = fogEndDistance;
            }
            else if (fogMode == FogMode.Exponential || fogMode == FogMode.ExponentialSquared)
            {
                RenderSettings.fogDensity = fogDensity;
            }
        }
    }
    
    /// <summary>
    /// Deactivates fog and restores original settings
    /// </summary>
    private void DeactivateFog()
    {
        if (isFogActive)
        {
            isFogActive = false;
            RenderSettings.fog = originalFogEnabled;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogMode = originalFogMode;
            RenderSettings.fogStartDistance = originalFogStartDistance;
            RenderSettings.fogEndDistance = originalFogEndDistance;
            RenderSettings.fogDensity = originalFogDensity;
        }
    }
    
    private void OnValidate()
    {
        // Update fog settings if already active
        if (Application.isPlaying && isFogActive)
        {
            ActivateFog();
        }
    }
    
    // Draw gizmo to visualize fog area in scene view
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        Gizmos.color = gizmoColor;
        
        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider)
        {
            SphereCollider sphere = col as SphereCollider;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider)
        {
            CapsuleCollider capsule = col as CapsuleCollider;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(capsule.center, new Vector3(capsule.radius * 2, capsule.height, capsule.radius * 2));
        }
        else
        {
            // For other collider types, draw a wireframe
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }
    }
    
    // Method to set fog color programmatically
    public void SetFogColor(Color color)
    {
        fogColor = color;
        if (isFogActive)
        {
            ActivateFog();
        }
    }
    
    // Method to set linear fog distances
    public void SetLinearFog(float startDistance, float endDistance)
    {
        fogMode = FogMode.Linear;
        fogStartDistance = startDistance;
        fogEndDistance = endDistance;
        if (isFogActive)
        {
            ActivateFog();
        }
    }
    
    // Method to set exponential fog density
    public void SetExponentialFog(float density)
    {
        fogMode = FogMode.Exponential;
        fogDensity = density;
        if (isFogActive)
        {
            ActivateFog();
        }
    }
}
