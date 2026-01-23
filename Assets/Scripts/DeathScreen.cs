using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathScreen : MonoBehaviour
{
    [Header("Settings")]
    public float fadeInDuration = 2f;
    public float waitBeforeMenu = 3f;
    public string mainMenuSceneName = "MainMenu";

    [Header("Sound Effects")]
    public AudioClip deathSound;
    [Range(0f, 1f)]
    public float deathSoundVolume = 1f;
    public float soundDelay = 0f;

    private CanvasGroup canvasGroup;
    private PlayerScript player;
    private AudioSource audioSource;
    private bool hasTriggered = false;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        player = FindObjectOfType<PlayerScript>();
    }

    void Update()
    {
        if (player != null && !player.isAlive && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(FadeInAndReturnToMenu());
        }
    }

    IEnumerator FadeInAndReturnToMenu()
    {
        // Wait for sound delay
        if (soundDelay > 0f)
        {
            yield return new WaitForSeconds(soundDelay);
        }

        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound, deathSoundVolume);
        }

        float elapsed = 0f;

        // Fade in
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(waitBeforeMenu);

        // Return to main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}