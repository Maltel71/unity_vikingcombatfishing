using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour
{
    public enum GnomeType { Garden, Berserker, Brute }

    [Header("Gnome Settings")]
    public GnomeType type;
    public int damage;
    public int health;
    public string gnomeName;

    [Header("Combat Settings")]
    public float attackSpeed = 1.0f;
    private float nextAttackTime = 0f;

    [Header("Movement Settings")]
    public float movementSpeed;

    [Header("Variation Settings")]
    [Range(0.5f, 2f)]
    public float minSizeMultiplier = 0.8f;
    [Range(0.5f, 2f)]
    public float maxSizeMultiplier = 1.2f;
    [Range(0.5f, 2f)]
    public float minSpeedMultiplier = 0.8f;
    [Range(0.5f, 2f)]
    public float maxSpeedMultiplier = 1.2f;

    [Header("Wave System Connection")]
    public EndlessWaveManager manager;

    [Header("Sound Effects")]
    public AudioClip[] hurtSounds;
    [Range(0f, 1f)]
    public float hurtSoundVolume = 1f;

    public AudioClip[] idleSounds;
    [Range(0f, 1f)]
    public float idleSoundVolume = 1f;

    [Header("Idle Sound Settings")]
    public float minIdleSoundTime = 3f;
    public float maxIdleSoundTime = 8f;
    private float nextIdleSoundTime;

    private AudioSource audioSource;
    private Transform playerTransform;

    void Start()
    {
        // Search for the object tagged "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Gnome cannot find Ragnar! Is he tagged as 'Player'?");
        }

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Set first idle sound time
        nextIdleSoundTime = Time.time + Random.Range(minIdleSoundTime, maxIdleSoundTime);

        // Set base stats based on type (removed health assignments)
        switch (type)
        {
            case GnomeType.Garden:
                gnomeName = "C_Common_Gnome";
                damage = 10;
                movementSpeed = 0.3f;
                break;
            case GnomeType.Berserker:
                gnomeName = "C_Berserker_Gnome";
                damage = 5;
                movementSpeed = 1.5f;
                break;
            case GnomeType.Brute:
                gnomeName = "C_Brute_Gnome";
                damage = 20;
                movementSpeed = 0.1f;
                break;
        }

        // Apply random variations
        ApplyVariations();
    }

    void ApplyVariations()
    {
        // Random size variation
        float sizeMultiplier = Random.Range(minSizeMultiplier, maxSizeMultiplier);
        transform.localScale *= sizeMultiplier;

        // Random speed variation
        float speedMultiplier = Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
        movementSpeed *= speedMultiplier;

        Debug.Log($"{gnomeName} spawned with size: {sizeMultiplier:F2}x, speed: {speedMultiplier:F2}x");
    }

    void Update()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Gnome is lost! It can't find anything with the 'Player' tag.");
        }

        if (playerTransform != null)
        {
            // Calculate the direction to Ragnar
            Vector3 direction = (playerTransform.position - transform.position).normalized;

            // Lock the Y axis so gnomes don't fly or sink into the floor
            direction.y = 0;

            // Move the gnome forward
            transform.position += direction * movementSpeed * Time.deltaTime;
        }

        // Play idle sounds at random intervals
        if (Time.time >= nextIdleSoundTime && idleSounds.Length > 0)
        {
            PlayRandomIdleSound();
            nextIdleSoundTime = Time.time + Random.Range(minIdleSoundTime, maxIdleSoundTime);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        // Play blood particle effect on every hit
        ParticleSystem bloodParticle = GetComponentInChildren<ParticleSystem>();
        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

        // Play hurt sound
        if (hurtSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, hurtSounds.Length);
            audioSource.PlayOneShot(hurtSounds[randomIndex], hurtSoundVolume);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Disable physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // Play blood particle system
        ParticleSystem bloodParticle = GetComponentInChildren<ParticleSystem>();
        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

        // Start fade out and destroy after 4 seconds
        StartCoroutine(FadeOutAndDestroy());

        if (manager != null)
        {
            manager.OnGnomeKilled();
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        // Wait 3 seconds before starting fade
        yield return new WaitForSeconds(3f);

        // Fade out over 1 second
        if (spriteRenderer != null)
        {
            float elapsed = 0f;
            float fadeDuration = 1f;
            Color startColor = spriteRenderer.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }

        // Destroy the gnome
        Destroy(gameObject);
    }

    void PlayRandomIdleSound()
    {
        if (audioSource != null && idleSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, idleSounds.Length);
            audioSource.PlayOneShot(idleSounds[randomIndex], idleSoundVolume);
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();

            if (player != null)
            {
                // Cooldown Logic for Attacks
                if (Time.time >= nextAttackTime)
                {
                    Attack(player);
                    // Set the next attack time based on attack speed
                    nextAttackTime = Time.time + (1f / attackSpeed);
                }
            }
        }
    }

    void Attack(PlayerScript player)
    {
        Debug.Log($"{gnomeName} hits you for {damage} damage!");

        // Trigger attack animation
        EnemyAnimationController animController = GetComponent<EnemyAnimationController>();
        if (animController != null)
        {
            animController.PlayAttack();
        }

        player.TakeDamage(damage);
    }
}