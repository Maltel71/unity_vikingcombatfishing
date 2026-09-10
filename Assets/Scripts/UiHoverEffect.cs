using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Selectable))]
public class UiHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    [Header("Text")]
    [Tooltip("Leave empty to take the first TextMeshProUGUI among the children.")]
    public TextMeshProUGUI label;

    [Header("Colours")]
    public Color normalColor = new Color(1f, 1f, 1f, 1f);
    public Color hoverColor = new Color(0.878f, 0.698f, 0.290f, 1f);
    public Color pressedColor = new Color(0.988f, 0.855f, 0.545f, 1f);

    [Header("Scale")]
    public float normalScale = 1f;
    public float hoverScale = 1.08f;
    public float pressedScale = 0.97f;

    [Tooltip("How fast it eases toward the target value. Higher is faster.")]
    public float speed = 14f;

    [Header("Audio")]
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
