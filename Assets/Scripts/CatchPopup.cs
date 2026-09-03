using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Liten text som poppar upp och visar vilken fisk man just fick upp.
///
/// Anropas fran FishingRod nar fisken kastas upp ur vattnet.
/// Har ingen text tilldelats bygger den en egen canvas i kod, sa den fungerar
/// direkt utan att nagot behover satas upp i editorn. Vill du styla den sjalv:
/// gor en egen TextMeshProUGUI, lagg det har scriptet pa nagot i scenen och
/// dra in texten i Popup Text.
/// </summary>
public class CatchPopup : MonoBehaviour
{
    public static CatchPopup Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Lamnas den tom byggs en egen text i kod.")]
    public TextMeshProUGUI popupText;

    [Header("Timing")]
    public float fadeInDuration = 0.15f;
    public float holdDuration = 1.3f;
    public float fadeOutDuration = 0.5f;

    [Header("Rorelse")]
    [Tooltip("Hur langt texten glider uppat medan den visas.")]
    public float riseDistance = 45f;
    [Tooltip("Hur mycket den studsar till nar den dyker upp.")]
    public float popScale = 1.25f;

    [Header("Farger")]
    public Color catchColor = new Color(0.878f, 0.698f, 0.290f, 1f);   // guld
    public Color junkColor = new Color(0.62f, 0.58f, 0.52f, 1f);       // dov - stoveln

    [Header("Innehall")]
    [Tooltip("Visa HP och poang pa rad tva.")]
    public bool showValues = true;

    private RectTransform rt;
    private Vector2 restPosition;
    private Coroutine routine;

    void Awake()
    {
        Instance = this;

        if (popupText == null)
        {
            BuildUI();
        }

        if (popupText != null)
        {
            rt = popupText.rectTransform;
            restPosition = rt.anchoredPosition;
            SetAlpha(0f);
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ---------------- Publikt anrop ----------------

    /// <summary>Visar fangsten. Skapar en popup om scenen saknar en.</summary>
    public static void Show(string fishName, int healthValue, int scoreValue)
    {
        CatchPopup popup = Instance;

        if (popup == null)
        {
            // Ta med avstangda objekt - annars byggs en dubblett ovanpa en utlagd popup
            popup = FindFirstObjectByType<CatchPopup>(FindObjectsInactive.Include);

            if (popup != null && !popup.gameObject.activeSelf)
            {
                popup.gameObject.SetActive(true);
            }
        }

        if (popup == null)
        {
            GameObject go = new GameObject("CatchPopup");
            popup = go.AddComponent<CatchPopup>();
        }

        popup.ShowCatch(fishName, healthValue, scoreValue);
    }

    public void ShowCatch(string fishName, int healthValue, int scoreValue)
    {
        if (popupText == null) return;

        bool junk = healthValue <= 0 && scoreValue <= 0;

        string text = string.IsNullOrEmpty(fishName) ? "?" : fishName.ToUpper();

        if (showValues)
        {
            string values = BuildValueLine(healthValue, scoreValue);
            if (!string.IsNullOrEmpty(values)) text += "\n" + values;
        }

        UiKit.SetText(popupText, text);
        popupText.color = junk ? junkColor : catchColor;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PlayPopup());
    }

    static string BuildValueLine(int healthValue, int scoreValue)
    {
        string line = "";

        if (healthValue != 0)
        {
            line += (healthValue > 0 ? "+" : "") + healthValue + " HP";
        }

        if (scoreValue != 0)
        {
            if (line.Length > 0) line += "   ";
            line += (scoreValue > 0 ? "+" : "") + scoreValue + " pts";
        }

        return line;
    }

    // ---------------- Animation ----------------

    IEnumerator PlayPopup()
    {
        rt.anchoredPosition = restPosition;

        // Fade in med en liten studs
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = fadeInDuration > 0f ? elapsed / fadeInDuration : 1f;

            SetAlpha(t);
            float scale = Mathf.Lerp(popScale, 1f, t);
            rt.localScale = Vector3.one * scale;

            yield return null;
        }

        SetAlpha(1f);
        rt.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(holdDuration);

        // Fade out medan den glider uppat
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = fadeOutDuration > 0f ? elapsed / fadeOutDuration : 1f;

            SetAlpha(1f - t);
            rt.anchoredPosition = restPosition + new Vector2(0f, riseDistance * t);

            yield return null;
        }

        SetAlpha(0f);
        rt.anchoredPosition = restPosition;
        routine = null;
    }

    void SetAlpha(float a)
    {
        if (popupText == null) return;
        Color c = popupText.color;
        c.a = Mathf.Clamp01(a);
        popupText.color = c;
    }

    // ---------------- Reserv-UI ----------------

    void BuildUI()
    {
        TMP_FontAsset font = UiKit.FindGameFont();

        GameObject canvas = UiKit.CreateCanvas("CatchPopupCanvas", transform, 300);

        popupText = UiKit.CreateText("CatchText", canvas.transform, "", 46f, catchColor, font);
        popupText.alignment = TextAlignmentOptions.Center;
        popupText.fontStyle = FontStyles.Bold;

        RectTransform r = popupText.rectTransform;
        r.anchorMin = new Vector2(0.5f, 0.5f);
        r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = new Vector2(0f, 190f);
        r.sizeDelta = new Vector2(900f, 140f);
    }
}
