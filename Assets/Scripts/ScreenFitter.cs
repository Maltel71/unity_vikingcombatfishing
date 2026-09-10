using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class ScreenFitter : MonoBehaviour
{
    [Header("Backdrop Size")]
    [Tooltip("Width of the background artwork in world units.")]
    public float worldWidth = 19.2f;
    [Tooltip("Height of the background artwork in world units.")]
    public float worldHeight = 10.8f;

    [Header("Margin")]
    [Tooltip("Fraction of the backdrop kept as a safety margin so the edges never show.")]
    [Range(0f, 0.1f)]
    public float inset = 0.005f;

    private Camera cam;
    private float baseSize;
    private int lastWidth;
    private int lastHeight;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera target = Camera.main;
        if (target == null) target = FindFirstObjectByType<Camera>();
        if (target == null || !target.orthographic) return;
        if (target.GetComponent<ScreenFitter>() != null) return;

        target.gameObject.AddComponent<ScreenFitter>();
    }

    void Awake()
    {
        cam = GetComponent<Camera>();
        baseSize = cam.orthographicSize;
    }

    void OnEnable()
    {
        lastWidth = 0;
        lastHeight = 0;
    }

    void LateUpdate()
    {
        if (Screen.width == lastWidth && Screen.height == lastHeight) return;

        lastWidth = Screen.width;
        lastHeight = Screen.height;
        Apply();
    }

    void Apply()
    {
        if (cam == null) return;

        float aspect = cam.aspect;
        if (aspect <= 0f) return;

        float halfWidth = worldWidth * 0.5f * (1f - inset);
        float halfHeight = worldHeight * 0.5f * (1f - inset);

        float size = Mathf.Min(baseSize, halfHeight);
        size = Mathf.Min(size, halfWidth / aspect);

        cam.orthographicSize = size;
    }
}
