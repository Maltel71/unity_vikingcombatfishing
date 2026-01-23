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

    private CanvasGroup canvasGroup;
    private PlayerScript player;
    private bool hasTriggered = false;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0f;

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