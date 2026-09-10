using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class RageBar : MonoBehaviour
{
    [Header("UI (leave empty to build in code)")]
    [Tooltip("Object toggled on and off. Leave empty to build the bar next to the health bar.")]
    public GameObject barRoot;
    [Tooltip("Image with Image Type = Filled, Horizontal.")]
    public Image fillImage;
    [Tooltip("Label shown while the swing is available. Drop in your own to style it yourself.")]
    public TextMeshProUGUI readyLabel;

    [Header("Placement")]
    [Tooltip("Sit above the health bar instead of below it.")]
    public bool placeAbove = true;
    [Tooltip("Gap to the health bar, counted in health bar heights.")]
    public float gapInBarHeights = 1.6f;
    [Tooltip("Height of the rage bar relative to the health bar.")]
    public float heightRatio = 0.75f;
    [Tooltip("Keep the bar facing the same way when Ragnar turns around.")]
    public bool keepUpright = true;

    [Header("Ready Text")]
    [Tooltip("Call out above the bar while the berserk swing is available.")]
    public bool showReadyText = true;
    public string readyText = "BERSERK";
    [Tooltip("Text height, counted in health bar heights.")]
    public float readyTextHeight = 1.3f;
    [Tooltip("Gap between the rage bar and the text, counted in health bar heights.")]
    public float readyGapInBarHeights = 1.4f;

    [Header("Appearance")]
    public Color trackColor = new Color(0.086f, 0.071f, 0.055f, 0.85f);
    public Color buildingColor = new Color(0.404f, 0.161f, 0.541f, 1f);
    public Color midColor = new Color(0.729f, 0.259f, 0.514f, 1f);
    public Color readyColor = new Color(0.949f, 0.729f, 0.263f, 1f);
    [Tooltip("How fast the bar chases the real value. Higher is snappier.")]
    public float fillSpeed = 8f;
    [Tooltip("How fast the bar fades in and out.")]
    public float fadeSpeed = 6f;
    [Tooltip("Pulses per second once the meter is full. 0 keeps it steady.")]
    public float pulseSpeed = 2.4f;
    [Tooltip("Fade the bar away while the meter is empty instead of leaving it on screen.")]
    public bool hideWhenEmpty = false;

    const float BaseFontSize = 40f;

    private RageMeter meter;
    private CanvasGroup group;
    private CanvasGroup readyGroup;
    private float shownFill;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainScene") return;
        if (FindFirstObjectByType<RageBar>(FindObjectsInactive.Include) != null) return;

        GameObject go = new GameObject("RageBar");
        go.AddComponent<RageBar>();
    }

    void Start()
    {
        meter = FindFirstObjectByType<RageMeter>(FindObjectsInactive.Include);

        if (fillImage == null)
        {
            Build();
        }

        if (barRoot != null)
        {
            HoldFacing(barRoot.transform);

            group = barRoot.GetComponent<CanvasGroup>();
            if (group == null) group = barRoot.AddComponent<CanvasGroup>();
            group.alpha = hideWhenEmpty ? 0f : 1f;
        }

        if (readyLabel != null)
        {
            HoldFacing(readyLabel.transform);

            readyGroup = readyLabel.GetComponent<CanvasGroup>();
            if (readyGroup == null) readyGroup = readyLabel.gameObject.AddComponent<CanvasGroup>();
            readyGroup.alpha = 0f;

            UiKit.SetText(readyLabel, readyText);
        }
    }

    void Update()
    {
        if (meter == null)
        {
            meter = FindFirstObjectByType<RageMeter>(FindObjectsInactive.Include);
        }

        float target = meter != null ? meter.Fill : 0f;
        bool full = meter != null && meter.IsFull;

        shownFill = Mathf.MoveTowards(shownFill, target, fillSpeed * Time.deltaTime);

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f);

        if (fillImage != null)
        {
            Color tint = full ? readyColor : Color.Lerp(buildingColor, midColor, shownFill);

            if (full && pulseSpeed > 0f)
            {
                tint = Color.Lerp(readyColor, Color.white, pulse * 0.55f);
            }

            fillImage.fillAmount = shownFill;
            fillImage.color = tint;
        }

        if (group != null)
        {
            float wanted = (!hideWhenEmpty || target > 0.001f) ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, wanted, fadeSpeed * Time.deltaTime);
        }

        if (readyGroup != null)
        {
            readyGroup.alpha = Mathf.MoveTowards(readyGroup.alpha, full ? 1f : 0f, fadeSpeed * Time.deltaTime);

            if (full && pulseSpeed > 0f)
            {
                readyLabel.color = Color.Lerp(readyColor, Color.white, pulse * 0.55f);
            }
        }
    }

    void HoldFacing(Transform target)
    {
        if (!keepUpright || target == null) return;
        if (target.GetComponent<KeepUpright>() != null) return;

        target.gameObject.AddComponent<KeepUpright>();
    }

    void Build()
    {
        HPBar health = FindFirstObjectByType<HPBar>(FindObjectsInactive.Include);

        if (health == null)
        {
            Debug.LogWarning("RageBar found no HPBar to sit next to. The rage bar was skipped.");
            return;
        }

        Image sourceImage = health.fillImage != null ? health.fillImage : health.GetComponentInChildren<Image>(true);

        if (sourceImage == null)
        {
            Debug.LogWarning("RageBar found an HPBar with no Fill Image. The rage bar was skipped.");
            return;
        }

        RectTransform source = sourceImage.rectTransform;
        if (source.parent == null) return;

        float barHeight = source.sizeDelta.y * source.localScale.y;
        float barWidth = source.sizeDelta.x * source.localScale.x;
        float direction = placeAbove ? 1f : -1f;

        GameObject holder = new GameObject("RageBar", typeof(RectTransform));
        holder.transform.SetParent(source.parent, false);
        holder.layer = source.gameObject.layer;
        barRoot = holder;

        RectTransform rt = holder.GetComponent<RectTransform>();
        rt.anchorMin = source.anchorMin;
        rt.anchorMax = source.anchorMax;
        rt.pivot = source.pivot;
        rt.sizeDelta = new Vector2(source.sizeDelta.x, source.sizeDelta.y * heightRatio);
        rt.localScale = source.localScale;
        rt.anchoredPosition = source.anchoredPosition + new Vector2(0f, barHeight * gapInBarHeights * direction);

        Image track = NewImage("RageTrack", holder.transform, sourceImage, trackColor);
        track.fillAmount = 1f;

        Image fill = NewImage("RageFill", holder.transform, sourceImage, buildingColor);
        fill.fillAmount = 0f;
        fillImage = fill;

        if (showReadyText)
        {
            BuildReadyText(source, rt, barWidth, barHeight, direction);
        }
    }

    void BuildReadyText(RectTransform source, RectTransform bar, float barWidth, float barHeight, float direction)
    {
        float textHeight = barHeight * readyTextHeight;
        if (textHeight <= 0f) return;

        float scale = textHeight / BaseFontSize;

        readyLabel = UiKit.CreateText("RageReady", source.parent, readyText,
                                      BaseFontSize, readyColor, UiKit.FindGameFont());
        readyLabel.gameObject.layer = source.gameObject.layer;
        readyLabel.fontStyle = FontStyles.Bold;
        readyLabel.characterSpacing = 6f;
        readyLabel.alignment = TextAlignmentOptions.Center;

        RectTransform rt = readyLabel.rectTransform;
        rt.anchorMin = source.anchorMin;
        rt.anchorMax = source.anchorMax;
        rt.pivot = source.pivot;
        rt.localScale = Vector3.one * scale;
        rt.sizeDelta = new Vector2(barWidth / scale, BaseFontSize * 1.4f);

        float rise = (bar.sizeDelta.y * bar.localScale.y * 0.5f) + (textHeight * 0.5f)
                     + (barHeight * readyGapInBarHeights);

        rt.anchoredPosition = bar.anchoredPosition + new Vector2(0f, rise * direction);
    }

    Image NewImage(string name, Transform parent, Image template, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = parent.gameObject.layer;

        Image image = go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        if (template != null)
        {
            image.sprite = template.sprite;
            image.type = template.type;
            image.fillMethod = template.fillMethod;
            image.fillOrigin = template.fillOrigin;
            image.fillClockwise = template.fillClockwise;
        }
        else
        {
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        UiKit.Stretch(image.rectTransform);
        return image;
    }
}
