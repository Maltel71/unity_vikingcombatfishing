using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DeathScreen : MonoBehaviour
{
    [Header("Settings")]
    public float fadeInDuration = 2f;
    public float waitBeforeMenu = 3f;
    public string mainMenuSceneName = "MainMenu";

    [Header("Highscore")]
    [Tooltip("Fraga efter namn nar poangen racker till topplistan.")]
    public bool askForNameOnHighscore = true;

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

        player = FindFirstObjectByType<PlayerScript>();
    }

    void Update()
    {
        if (player != null && !player.isAlive && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DeathSequence()
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

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        // Fiskepoang + blodspengar avgor plats pa topplistan
        int total = player != null ? player.TotalScore : 0;

        // Steam behaller bara ens basta, sa vi kan skicka upp varje runda.
        // Gor ingenting om Steam inte ar igang.
        SteamLeaderboards.UploadScore(total);

        if (askForNameOnHighscore && Highscores.Qualifies(total))
        {
            yield return StartCoroutine(AskForNameAndSave(total));
        }
        else
        {
            yield return new WaitForSeconds(waitBeforeMenu);
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    IEnumerator AskForNameAndSave(int total)
    {
        // Vilken plats hamnar man pa? Raknas ut innan resultatet lagts in.
        int placement = PredictPlacement(total);

        bool done = false;
        NameEntryScreen.Show(total, placement, delegate (string name)
        {
            Highscores.Add(name, total);
            done = true;
        });

        while (!done)
        {
            yield return null;
        }
    }

    int PredictPlacement(int score)
    {
        System.Collections.Generic.List<Highscores.Entry> entries = Highscores.Load();

        for (int i = 0; i < entries.Count; i++)
        {
            if (score > entries[i].score) return i + 1;
        }

        return entries.Count < Highscores.MaxEntries ? entries.Count + 1 : 0;
    }
}
