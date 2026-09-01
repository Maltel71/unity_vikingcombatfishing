using UnityEngine;
using TMPro;
using System.Collections;

public class WaveAnnouncer : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI waveText;

    [Header("Animation Settings")]
    public float fadeInDuration = 0.5f;
    public float holdDuration = 2f;
    public float fadeOutDuration = 0.5f;
    public float scaleMultiplier = 1.5f;

    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    void Start()
    {
        // If waveText not assigned, try to get it from this object
        if (waveText == null)
        {
            waveText = GetComponent<TextMeshProUGUI>();
        }

        if (waveText == null)
        {
            Debug.LogError("WaveAnnouncer: No TextMeshProUGUI found!");
            return;
        }

        // Get or add CanvasGroup
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("WaveAnnouncer: No CanvasGroup found on " + gameObject.name);
            return;
        }

        originalScale = transform.localScale;
        canvasGroup.alpha = 0f;

    }

    public void AnnounceWave(int waveNumber)
    {
        Announce($"Wave {waveNumber}");
    }

    // Overload for fri text, t.ex. elitfienden "MUSCLE"
    public void AnnounceWave(string message)
    {
        Announce(message);
    }

    void Announce(string message)
    {
        if (canvasGroup == null || waveText == null)
        {
            Debug.LogError("WaveAnnouncer: Missing references!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowWaveAnnouncement(message));
    }

    IEnumerator ShowWaveAnnouncement(string message)
    {
        waveText.text = message;

        float elapsed = 0f;

        // Fade in + Scale up
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * scaleMultiplier, progress);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = originalScale * scaleMultiplier;

        // Hold
        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;

        // Fade out + Scale down
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, progress);
            transform.localScale = Vector3.Lerp(originalScale * scaleMultiplier, originalScale, progress);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        transform.localScale = originalScale;
    }
}