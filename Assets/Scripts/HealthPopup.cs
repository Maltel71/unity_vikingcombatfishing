using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class HealthPopup : MonoBehaviour
{
    public static HealthPopup Instance { get; private set; }

    [Header("Placement")]
    [Tooltip("Where above Ragnar the text appears, in world units.")]
    public Vector3 offset = new Vector3(0f, 1.4f, 0f);
    [Tooltip("Random sideways spread so two pickups in a row do not overlap.")]
    public float spread = 0.25f;
    [Tooltip("How far the text drifts upward before it is gone.")]
    public float riseDistance = 0.8f;

    [Header("Timing")]
    public float duration = 1.2f;

    [Header("Look")]
    public float fontSize = 3.5f;
    public Color healColor = new Color(0.475f, 0.788f, 0.427f, 1f);
    public Color hurtColor = new Color(0.827f, 0.322f, 0.271f, 1f);
    [Tooltip("Leave empty to borrow the font the rest of the UI uses.")]
    public TMP_FontAsset font;
    public int sortingOrder = 200;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (FindFirstObjectByType<HealthPopup>(FindObjectsInactive.Include) != null) return;

        PlayerScript player = FindFirstObjectByType<PlayerScript>();
        if (player == null) return;

        player.gameObject.AddComponent<HealthPopup>();
    }

    void Awake()
    {
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Show(int amount)
    {
        if (amount == 0) return;

        HealthPopup popup = Instance;
        if (popup == null)
        {
            popup = FindFirstObjectByType<HealthPopup>(FindObjectsInactive.Include);
        }

        if (popup != null) popup.Spawn(amount);
    }

    void Spawn(int amount)
    {
        if (font == null) font = UiKit.FindGameFont();

        GameObject go = new GameObject("HealthPopupText");
        TextMeshPro label = go.AddComponent<TextMeshPro>();

        if (font != null) label.font = font;
        label.text = (amount > 0 ? "+" : "") + amount + " HP";
        label.fontSize = fontSize;
        label.color = amount > 0 ? healColor : hurtColor;
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.sizeDelta = new Vector2(8f, 2f);

        MeshRenderer mesh = go.GetComponent<MeshRenderer>();
        if (mesh != null) mesh.sortingOrder = sortingOrder;

        Vector3 start = transform.position + offset;
        start.x += Random.Range(-spread, spread);
        go.transform.position = start;

        Destroy(go, duration + 0.5f);
        StartCoroutine(Rise(go.transform, label, start));
    }

    IEnumerator Rise(Transform target, TextMeshPro label, Vector3 start)
    {
        float elapsed = 0f;
        Color tint = label.color;

        while (elapsed < duration && target != null)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;

            target.position = start + Vector3.up * (riseDistance * t);
            tint.a = 1f - Mathf.Pow(t, 3f);
            label.color = tint;

            yield return null;
        }

        if (target != null) Destroy(target.gameObject);
    }
}
