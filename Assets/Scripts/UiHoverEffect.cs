using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Hover-effekt for meny-knappar dar sjalva knappbilden ar osynlig och det bara
/// ar texten som syns.
///
/// Unitys inbyggda Color Tint fargar knappens Image, inte texten - och ar
/// knappens fargtoner satta med alpha 0 (for att gomma rutan) blir det aldrig
/// nagot synligt alls. Det har scriptet fargar och skalar TEXTEN istallet.
///
/// Lagg det pa knappen. Texten hittas automatiskt bland barnen.
/// </summary>
[RequireComponent(typeof(Selectable))]
public class UiHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Text")]
    [Tooltip("Lamnas den tom hamtas forsta TextMeshProUGUI bland barnen.")]
    public TextMeshProUGUI label;

    [Header("Farger")]
    public Color normalColor = new Color(1f, 1f, 1f, 1f);
    public Color hoverColor = new Color(0.878f, 0.698f, 0.290f, 1f);   // guld
    public Color pressedColor = new Color(0.988f, 0.855f, 0.545f, 1f);

    [Header("Skala")]
    public float normalScale = 1f;
    public float hoverScale = 1.08f;
    public float pressedScale = 0.97f;

    [Tooltip("Hur snabbt den glider mot mallvardet. Hogre = snabbare.")]
    public float speed = 14f;

    [Header("Ljud")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.6f;

    private Selectable selectable;
    private AudioSource audioSource;
    private bool pointerInside;
    private bool selected;
    private bool pressed;

    void Awake()
    {
        selectable = GetComponent<Selectable>();

        if (label == null)
        {
            label = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (label != null)
        {
            // Utga fran textens egen farg om ingen ar satt i inspektorn
            if (normalColor.a <= 0f) normalColor = label.color;
            label.color = normalColor;
            label.transform.localScale = Vector3.one * normalScale;
        }
    }

    void OnDisable()
    {
        pointerInside = false;
        selected = false;
        pressed = false;

        if (label != null)
        {
            label.color = normalColor;
            label.transform.localScale = Vector3.one * normalScale;
        }
    }

    void Update()
    {
        if (label == null) return;

        bool active = selectable == null || selectable.interactable;
        bool highlighted = active && (pointerInside || selected);

        Color targetColor = !active ? normalColor
                          : pressed ? pressedColor
                          : highlighted ? hoverColor
                          : normalColor;

        float targetScale = !active ? normalScale
                          : pressed ? pressedScale
                          : highlighted ? hoverScale
                          : normalScale;

        // Ospalad tid sa effekten funkar aven nar spelet ar pausat
        float t = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);

        label.color = Color.Lerp(label.color, targetColor, t);
        label.transform.localScale = Vector3.Lerp(
            label.transform.localScale, Vector3.one * targetScale, t);
    }

    void PlayOnce(AudioClip clip)
    {
        if (clip == null) return;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        audioSource.PlayOneShot(clip, soundVolume);
    }

    // ---------------- Event-handlers ----------------

    public void OnPointerEnter(PointerEventData e)
    {
        pointerInside = true;
        PlayOnce(hoverSound);
    }

    public void OnPointerExit(PointerEventData e)
    {
        pointerInside = false;
        pressed = false;
    }

    public void OnPointerDown(PointerEventData e)
    {
        pressed = true;
        PlayOnce(clickSound);
    }

    public void OnPointerUp(PointerEventData e)
    {
        pressed = false;
    }

    // Sa att tangentbord och handkontroll ocksa ger markering
    public void OnSelect(BaseEventData e)
    {
        selected = true;
        PlayOnce(hoverSound);
    }

    public void OnDeselect(BaseEventData e)
    {
        selected = false;
    }
}
