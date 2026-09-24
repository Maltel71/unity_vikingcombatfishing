using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BossHealthBar : MonoBehaviour
{
    [Header("UI (leave empty to build in code)")]
    [Tooltip("Object toggled on and off. Leave empty to build the whole bar in code.")]
    public GameObject barRoot;
    public TextMeshProUGUI nameLabel;
    [Tooltip("Image with Image Type = Filled, Horizontal.")]
    public Image fillImage;
    [Tooltip("Optional label showing 340 / 700.")]
    public TextMeshProUGUI healthLabel;

    [Header("Appearance")]
    [Tooltip("Off: the fill keeps the colour you set on the Image, so your own artwork shows as drawn.")]
    public bool tintFill = true;
    public Color healthyColor = new Color(0.780f, 0.294f, 0.243f, 1f);
    [Tooltip("Colour the bar fades to when the boss is close to death.")]
    public Color criticalColor = new Color(0.949f, 0.729f, 0.263f, 1f);
    [Range(0f, 1f)]
    public float criticalThreshold = 0.3f;
    [Tooltip("How fast the bar drains. Higher is faster.")]
    public float drainSpeed = 6f;
    public float fadeSpeed = 5f;
    [Tooltip("Show numbers next to the bar.")]
    public bool showNumbers = true;

    private EndlessWaveManager waves;
    private CanvasGroup group;
    private float shownFill = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainScene") return;
        if (FindFirstObjectByType<BossHealthBar>(FindObjectsInactive.Include) != null) return;

        GameObject go = new GameObject("BossHealthBar");
        go.AddComponent<BossHealthBar>();
    }

    void Start()
    {
        waves = FindFirstObjectByType<EndlessWaveManager>();

        if (barRoot == null)
        {
            BuildUI();
        }

        group = barRoot != null ? barRoot.GetComponent<CanvasGroup>() : null;
        if (barRoot != null && group == null)
        {
            group = barRoot.AddComponent<CanvasGroup>();
        }

        if (group != null) group.alpha = 0f;
    }

    void Update()
    {
        if (barRoot == null) return;

        if (waves == null)
        {
            waves = FindFirstObjectByType<EndlessWaveManager>();
            if (waves == null) return;
        }

        EnemyScript boss = waves.ActiveBoss;
        bool visible = !PlayerScript.IsGameOver && boss != null && boss.health > 0;

        if (group != null)
        {
            float target = visible ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, fadeSpeed * Time.deltaTime);
            if (group.alpha <= 0.001f && !visible) return;
        }

        if (!visible) return;

        UiKit.SetText(nameLabel, waves.ActiveBossLabel);

        float ratio = boss.MaxHealth > 0 ? Mathf.Clamp01((float)boss.health / boss.MaxHealth) : 0f;

        shownFill = Mathf.MoveTowards(shownFill, ratio, drainSpeed * Time.deltaTime);

        if (fillImage != null)
        {
            ApplyFill(shownFill);

            if (tintFill)
            {
                fillImage.color = ratio <= criticalThreshold ? criticalColor : healthyColor;
            }
        }

        if (showNumbers && healthLabel != null)
        {
            UiKit.SetText(healthLabel, Mathf.CeilToInt(boss.health) + " / " + boss.MaxHealth);
        }
        else if (healthLabel != null)
        {
            healthLabel.text = "";
        }
    }

    void ApplyFill(float value)
    {
        if (fillImage == null) return;

        value = Mathf.Clamp01(value);

        if (fillImage.sprite != null && fillImage.type == Image.Type.Filled)
        {
            fillImage.fillAmount = value;
            return;
        }

        RectTransform rt = fillImage.rectTransform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(value, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void BuildUI()
    {
        TMP_FontAsset font = UiKit.FindGameFont();

        GameObject canvas = UiKit.CreateCanvas("BossHealthBarCanvas", transform, 400);
        barRoot = canvas;

        GameObject holder = new GameObject("Bar", typeof(RectTransform));
        holder.transform.SetParent(canvas.transform, false);
        RectTransform hrt = holder.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.5f, 1f);
        hrt.anchorMax = new Vector2(0.5f, 1f);
        hrt.pivot = new Vector2(0.5f, 1f);
        hrt.anchoredPosition = new Vector2(0f, -34f);
        hrt.sizeDelta = new Vector2(760f, 110f);

        nameLabel = UiKit.CreateText("BossName", holder.transform, "", 40f, UiKit.Border, font);
        nameLabel.fontStyle = FontStyles.Bold;
        nameLabel.characterSpacing = 8f;
        UiKit.AnchorTop(nameLabel.rectTransform, 0f, -26f, 760f, 52f);

        Image border = UiKit.CreateImage("BarBorder", holder.transform, UiKit.Border);
        UiKit.AnchorTop(border.rectTransform, 0f, -72f, 706f, 32f);

        Image track = UiKit.CreateImage("BarTrack", holder.transform, UiKit.Track);
        UiKit.AnchorTop(track.rectTransform, 0f, -72f, 700f, 26f);

        Image fill = UiKit.CreateImage("BarFill", track.transform, healthyColor);
        UiKit.Stretch(fill.rectTransform);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.fillAmount = 1f;
        fillImage = fill;

        healthLabel = UiKit.CreateText("BossHealth", holder.transform, "", 20f, UiKit.TextColor, font);
        UiKit.AnchorTop(healthLabel.rectTransform, 0f, -72f, 700f, 26f);
    }
}
