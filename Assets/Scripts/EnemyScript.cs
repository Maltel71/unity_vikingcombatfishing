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
    public float attackRange = 1.5f;

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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Gnome cannot find Ragnar! Is he tagged as 'Player'?");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        nextIdleSoundTime = Time.time + Random.Range(minIdleSoundTime, maxIdleSoundTime);

        // Only set defaults if not configured in Inspector
        if (movementSpeed == 0f && damage == 0 && string.IsNullOrEmpty(gnomeName))
        {
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
        }

        ApplyVariations();
    }

    void ApplyVariations()
    {
        float sizeMultiplier = Random.Range(minSizeMultiplier, maxSizeMultiplier);
        transform.localScale *= sizeMultiplier;

        float speedMultiplier = Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
        movementSpeed *= speedMultiplier;

        Debug.Log($"{gnomeName} spawned with size: {sizeMultiplier:F2}x, speed: {speedMultiplier:F2}x");
    }

    void Update()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Gnome is lost! It can't find anything with the 'Player' tag.");
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Check if player is in attack range
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            PlayerScript player = playerTransform.GetComponent<PlayerScript>();
            if (player != null)
            {
                Attack(player);
                nextAttackTime = Time.time + (1f / attackSpeed);
            }
        }
        else if (distanceToPlayer > attackRange)
        {
            // Move towards player only if not in attack range
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * movementSpeed * Time.deltaTime;
        }

        // Play idle sounds
        if (Time.time >= nextIdleSoundTime && idleSounds.Length > 0)
        {
            PlayRandomIdleSound();
            nextIdleSoundTime = Time.time + Random.Range(minIdleSoundTime, maxIdleSoundTime);
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        ParticleSystem bloodParticle = GetComponentInChildren<ParticleSystem>();
        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

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
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        ParticleSystem bloodParticle = GetComponentInChildren<ParticleSystem>();
        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

        StartCoroutine(FadeOutAndDestroy());

        if (manager != null)
        {
            manager.OnGnomeKilled();
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        yield return new WaitForSeconds(3f);

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

    void Attack(PlayerScript player)
    {
        EnemyAnimationController animController = GetComponent<EnemyAnimationController>();
        if (animController != null)
        {
            animController.PlayAttack();
        }
    }

    // Called by Animation Event
    public void DealDamage()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            PlayerScript player = playerTransform.GetComponent<PlayerScript>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"{gnomeName} hits you for {damage} damage!");
            }
        }
    }
}