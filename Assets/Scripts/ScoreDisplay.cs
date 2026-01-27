using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;

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

        player = FindObjectOfType<PlayerScript>();

        if (scoreText != null)
        {
            originalScale = scoreText.transform.localScale;
        }

        if (player != null)
        {
            lastScore = player.playerScore;
        }

        // Setup audio source
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
            // Check if score changed
            if (player.playerScore != lastScore)
            {
                OnScoreChanged();
                lastScore = player.playerScore;
            }

            scoreText.text = "Score: " + player.playerScore;

            // Handle bounce animation
            if (isBouncing)
            {
                bounceTimer += Time.deltaTime;
                float progress = bounceTimer / bounceDuration;

                if (progress < 0.5f)
                {
                    // Scale up
                    float scale = Mathf.Lerp(1f, bounceScale, progress * 2f);
                    scoreText.transform.localScale = originalScale * scale;
                }
                else
                {
                    // Scale down
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

    void OnScoreChanged()
    {
        // Start bounce animation
        isBouncing = true;
        bounceTimer = 0f;

        // Play particle effect
        if (scoreParticle != null)
        {
            scoreParticle.Play();
        }

        // Play sound effect
        if (scoreSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scoreSound, scoreVolume);
        }
    }
}