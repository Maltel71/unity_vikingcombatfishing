using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Delade byggstenar for de meny-paneler som byggs i kod (PauseMenu, NameEntryScreen).
/// Samlar farger och UI-bygge pa ett stalle sa allt ser likadant ut.
/// </summary>
public static class UiKit
{
    // Viking/tra-kansla med guldaccent
    public static readonly Color Dim = new Color(0f, 0f, 0f, 0.78f);
    public static readonly Color Panel = new Color(0.118f, 0.094f, 0.075f, 0.98f);
    public static readonly Color Border = new Color(0.78f, 0.635f, 0.294f, 1f);
    public static readonly Color ButtonNormal = new Color(0.227f, 0.184f, 0.141f, 1f);
    public static readonly Color ButtonHover = new Color(0.353f, 0.286f, 0.196f, 1f);
    public static readonly Color ButtonPress = new Color(0.478f, 0.388f, 0.243f, 1f);
    public static readonly Color TextColor = new Color(0.929f, 0.890f, 0.824f, 1f);
    public static readonly Color TextDim = new Color(0.929f, 0.890f, 0.824f, 0.55f);
    public static readonly Color Track = new Color(0.086f, 0.071f, 0.055f, 1f);

    /// <summary>Anvander samma typsnitt som resten av spelets UI om det gar att hitta.</summary>
    public static TMP_FontAsset FindGameFont()
    {
        TextMeshProUGUI[] texts = Object.FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TextMeshProUGUI t in texts)
        {
            if (t != null && t.font != null) return t.font;
        }
        return null;
    }

    /// <summary>Bada scenerna har redan en EventSystem, men om nagon tar bort den slutar knapparna funka.</summary>
    public static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        es.AddComponent<StandaloneInputModule>();
#endif
    }

    public static GameObject CreateCanvas(string name, Transform parent, int sortingOrder)
    {
        GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(parent, false);

        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        return go;
    }

    public static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    public static TextMeshProUGUI CreateText(string name, Transform parent, string content,
                                             float size, Color color, TMP_FontAsset font)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = Fit(content, font);
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static Button CreateButton(string name, Transform parent, string label, TMP_FontAsset font)
    {
        // Knappens Image ar vit - Button multiplicerar in state-fargen,
        // sa vit bas ger exakt de farger vi satter nedan.
        Image bg = CreateImage(name, parent, Color.white);
        Button btn = bg.gameObject.AddComponent<Button>();
        btn.targetGraphic = bg;

        ColorBlock colors = btn.colors;
        colors.normalColor = ButtonNormal;
        colors.highlightedColor = ButtonHover;
        colors.pressedColor = ButtonPress;
        colors.selectedColor = ButtonHover;
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        btn.colors = colors;

        TextMeshProUGUI text = CreateText(name + "Text", bg.transform, label, 30f, TextColor, font);
        Stretch(text.rectTransform);

        return btn;
    }

    public static Slider CreateSlider(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Slider slider = go.AddComponent<Slider>();

        Image background = CreateImage("Background", go.transform, Track);
        RectTransform bgRt = background.rectTransform;
        bgRt.anchorMin = new Vector2(0f, 0.5f);
        bgRt.anchorMax = new Vector2(1f, 0.5f);
        bgRt.pivot = new Vector2(0.5f, 0.5f);
        bgRt.offsetMin = new Vector2(0f, -7f);
        bgRt.offsetMax = new Vector2(0f, 7f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRt = fillArea.GetComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0f, 0.5f);
        fillAreaRt.anchorMax = new Vector2(1f, 0.5f);
        fillAreaRt.pivot = new Vector2(0.5f, 0.5f);
        fillAreaRt.offsetMin = new Vector2(0f, -7f);
        fillAreaRt.offsetMax = new Vector2(-16f, 7f);

        Image fill = CreateImage("Fill", fillArea.transform, Border);
        RectTransform fillRt = fill.rectTransform;
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.sizeDelta = new Vector2(16f, 0f);

        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        RectTransform handleAreaRt = handleArea.GetComponent<RectTransform>();
        handleAreaRt.anchorMin = new Vector2(0f, 0f);
        handleAreaRt.anchorMax = new Vector2(1f, 1f);
        handleAreaRt.offsetMin = new Vector2(8f, 0f);
        handleAreaRt.offsetMax = new Vector2(-8f, 0f);

        Image handle = CreateImage("Handle", handleArea.transform, TextColor);
        RectTransform handleRt = handle.rectTransform;
        handleRt.anchorMin = new Vector2(0f, 0f);
        handleRt.anchorMax = new Vector2(0f, 1f);
        handleRt.pivot = new Vector2(0.5f, 0.5f);
        handleRt.sizeDelta = new Vector2(26f, 0f);

        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        return slider;
    }

    /// <summary>Panel med guldram. Returnerar panelen (ramen ligger bakom).</summary>
    public static Image CreatePanel(Transform parent, float width, float height)
    {
        Image border = CreateImage("PanelBorder", parent, Border);
        Center(border.rectTransform, width + 8f, height + 8f);

        Image panel = CreateImage("Panel", parent, Panel);
        Center(panel.rectTransform, width, height);

        return panel;
    }


    // ---------------- Teckenanpassning ----------------
    // Spelets typsnitt (Rgf_v1) har bara 83 tecken och saknar a-ring och umlaut.
    // Skriver man "Gadda" med prick-a blir det en tom ruta. Fit() byter darfor ut
    // tecken som fonten saknar mot narmaste ASCII. Dagen du lagger till glyferna
    // i Font Asset Creator slutar den byta ut nagot av sig sjalv.
    static readonly char[] Missing = { 'å', 'ä', 'ö', 'Å', 'Ä', 'Ö', 'é', 'É', 'ü', 'Ü', 'æ', 'ø', 'Æ', 'Ø' };
    static readonly char[] Replacement = { 'a', 'a', 'o', 'A', 'A', 'O', 'e', 'E', 'u', 'U', 'a', 'o', 'A', 'O' };

    public static string Fit(string text, TMP_FontAsset font)
    {
        if (string.IsNullOrEmpty(text) || font == null) return text;

        System.Text.StringBuilder sb = null;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\n' || font.HasCharacter(c)) continue;

            if (sb == null) sb = new System.Text.StringBuilder(text);

            char replacement = '?';
            for (int m = 0; m < Missing.Length; m++)
            {
                if (Missing[m] == c) { replacement = Replacement[m]; break; }
            }
            sb[i] = replacement;
        }

        return sb != null ? sb.ToString() : text;
    }

    /// <summary>Satter texten pa en TMP och anpassar tecknen till dess typsnitt.</summary>
    public static void SetText(TextMeshProUGUI label, string text)
    {
        if (label == null) return;
        label.text = Fit(text, label.font);
    }

    // ---------------- Layouthjalpare ----------------

    public static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public static void Center(RectTransform rt, float width, float height)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(width, height);
    }

    /// <summary>Placerar ett element relativt foralderns ovankant.</summary>
    public static void AnchorTop(RectTransform rt, float x, float y, float width, float height)
    {
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(width, height);
    }
}
