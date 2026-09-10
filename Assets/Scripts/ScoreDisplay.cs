using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

    [Header("Blood Money")]
    [Tooltip("Separate label for blood money. Leave empty to print it on line two of scoreText.")]
    public TextMeshProUGUI bloodText;
    [Tooltip("Show the boss kill count in brackets.")]
    public bool showBossCount = true;

    [Header("Animation Settings")]
    public float bounceScale = 1.3f;
    public float bounceDuration = 0.3f;

    [Header("Particle Effect")]
    public ParticleSystem scoreParticle;

    [Header("Sound Effect")]
    public AudioClip scoreSound;
    [Range(0f, 1f)]
    public float scoreVolume = 1f;

    private PlayerScript player;
    private int lastScore = 0;
    private int lastBlood = 0;
    private Vector3 originalScale;
    private float bounceTimer = 0f;
    private bool isBouncing = false;
    private AudioSource audioSource;

    void Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }

        player = FindFirstObjectByType<PlayerScript>();

        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
        }

        if (player != null)
        {
            lastScore = player.playerScore;
            lastBlood = player.bloodMoney;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (player != null && scoreText != null)
        {

            if (player.playerScore != lastScore)
            {
                OnScoreChanged();
                lastScore = player.playerScore;
            }

            if (player.bloodMoney != lastBlood)
            {
                lastBlood = player.bloodMoney;
            }

            string blood = BuildBloodLine();

            if (bloodText != null)
            {
                scoreText.text = "Score: " + player.playerScore;
                bloodText.text = blood;
            }
            else
            {

                scoreText.text = "Score: " + player.playerScore + "\n" + blood;
            }

            if (isBouncing)
            {
                bounceTimer += Time.deltaTime;
                float progress = bounceTimer / bounceDuration;

                if (progress < 0.5f)
                {

                    float scale = Mathf.Lerp(1f, bounceScale, progress * 2f);
                    scoreText.transform.localScale = originalScale * scale;
                }
                else
                {

                    float scale = Mathf.Lerp(bounceScale, 1f, (progress - 0.5f) * 2f);
                    scoreText.transform.localScale = originalScale * scale;
                }

                if (progress >= 1f)
                {
                    isBouncing = false;
                    scoreText.transform.localScale = originalScale;
                }
            }
        }
    }

    string BuildBloodLine()
    {
        string line = "Blood: " + player.bloodMoney;

        if (showBossCount && player.bossesKilled > 0)
        {
            line += player.bossesKilled == 1 ? "  (1 boss)" : "  (" + player.bossesKilled + " bosses)";
        }

        return line;
    }

    void OnScoreChanged()
    {

        isBouncing = true;
        bounceTimer = 0f;

        if (scoreParticle != null)
        {
            scoreParticle.Play();
        }

        if (scoreSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scoreSound, scoreVolume);
        }
    }
}
