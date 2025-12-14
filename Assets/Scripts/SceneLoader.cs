using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahne yükleme için basit bir script
/// New Game butonuna veya diğer UI elementlerine bağlanabilir
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Yüklenecek sahnenin adı (Build Settings'teki isimle aynı olmalı)")]
    [SerializeField] private string sceneName = "Start";
    
    [Tooltip("Sahne yükleme modu")]
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;
    
    [Header("Settings")]
    [Tooltip("Yükleme sırasında loading ekranı göster")]
    [SerializeField] private bool showLoadingScreen = false;
    
    [Tooltip("Yükleme öncesi bekleme süresi (saniye)")]
    [SerializeField] private float delayBeforeLoad = 0f;
    
    [Header("Fade Settings")]
    [Tooltip("Yükleme öncesi fade efekti göster")]
    [SerializeField] private bool useFadeEffect = false;
    
    [Tooltip("Fade süresi (saniye)")]
    [SerializeField] private float fadeDuration = 1f;
    
    private bool isLoading = false;
    
    /// <summary>
    /// Sahneyi yükle (public method, butonlardan çağrılabilir)
    /// </summary>
    public void LoadScene()
    {
        if (isLoading) return;
        
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader: Sahne adı belirtilmemiş!");
            return;
        }
        
        isLoading = true;
        
        if (delayBeforeLoad > 0f)
        {
            Invoke(nameof(ExecuteLoad), delayBeforeLoad);
        }
        else
        {
            ExecuteLoad();
        }
    }
    
    /// <summary>
    /// Belirtilen sahne adını yükle (dinamik kullanım için)
    /// </summary>
    public void LoadSceneByName(string scene)
    {
        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogError("SceneLoader: Geçersiz sahne adı!");
            return;
        }
        
        sceneName = scene;
        LoadScene();
    }
    
    /// <summary>
    /// Oyunu başlat (New Game için özel method)
    /// </summary>
    public void StartNewGame()
    {
        LoadScene();
    }
    
    /// <summary>
    /// Oyunu yeniden başlat (mevcut sahneyi yeniden yükle)
    /// </summary>
    public void RestartCurrentScene()
    {
        sceneName = SceneManager.GetActiveScene().name;
        LoadScene();
    }
    
    /// <summary>
    /// Oyunu kapat
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Oyun kapatılıyor...");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    void ExecuteLoad()
    {
        if (useFadeEffect)
        {
            StartCoroutine(LoadSceneWithFade());
        }
        else
        {
            SceneManager.LoadScene(sceneName, loadMode);
        }
    }
    
    System.Collections.IEnumerator LoadSceneWithFade()
    {
        // Basit fade efekti (Canvas Group kullanarak)
        CanvasGroup canvasGroup = FindObjectOfType<CanvasGroup>();
        if (canvasGroup == null)
        {
            // Canvas'ta CanvasGroup yoksa oluştur
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvasGroup = canvas.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Fade out
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        
        // Sahneyi yükle
        SceneManager.LoadScene(sceneName, loadMode);
    }
    
    void OnValidate()
    {
        // Inspector'da sahne adı boşsa uyarı ver
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning($"SceneLoader ({gameObject.name}): Sahne adı belirtilmemiş!");
        }
    }
}


