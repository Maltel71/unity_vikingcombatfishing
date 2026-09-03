using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour
{
    [Header("Gnome Settings")]
    public int damage;
    public int health;
    public string gnomeName;

    [Header("Combat Settings")]
    public float attackSpeed = 1.0f;
    private float nextAttackTime = 0f;
    public float attackRange = 1.5f;

    [Header("Movement Settings")]
    public float minMovementSpeed = 0.3f;
    public float maxMovementSpeed = 0.5f;
    private float movementSpeed;

    [Header("Variation Settings")]
    [Range(0.5f, 2f)]
    public float minSizeMultiplier = 0.8f;
    [Range(0.5f, 2f)]
    public float maxSizeMultiplier = 1.2f;

    [Header("Wave System Connection")]
    public EndlessWaveManager manager;

    // Satts av EndlessWaveManager vid spawn. Elitfiender (Muscle) raknas separat.
    [HideInInspector] public bool isElite = false;

    /// <summary>Halsan vid spawn, efter elitmultiplikatorerna. Anvands av hpbaren.</summary>
    public int MaxHealth { get; private set; }
    [HideInInspector] public float eliteHealthMultiplier = 1f;
    [HideInInspector] public float eliteDamageMultiplier = 1f;
    [HideInInspector] public float eliteSizeMultiplier = 1.6f;
    [HideInInspector] public float eliteSpeedMultiplier = 1.5f;

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
    private PlayerScript playerScript;
    private bool isDying = false;
    private EnemyAnimationController animController;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerScript = playerObj.GetComponent<PlayerScript>();
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

        animController = GetComponent<EnemyAnimationController>();

        nextIdleSoundTime = Time.time + Random.Range(minIdleSoundTime, maxIdleSoundTime);

        ApplyVariations();

        // Efter ApplyVariations - elitens health har redan multiplicerats upp
        MaxHealth = Mathf.Max(1, health);
    }

    void ApplyVariations()
    {
        if (isElite)
        {
            // Eliten ska se likadan ut varje gang - ingen slump, bara storre och tuffare
            transform.localScale *= eliteSizeMultiplier;
            movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed) * eliteSpeedMultiplier;
            health = Mathf.RoundToInt(health * eliteHealthMultiplier);
            damage = Mathf.RoundToInt(damage * eliteDamageMultiplier);
            return;
        }

        // Random size
        float sizeMultiplier = Random.Range(minSizeMultiplier, maxSizeMultiplier);
        transform.localScale *= sizeMultiplier;

        // Random movement speed from range
        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);

    }

    void Update()
    {
        // Stop updating if dead
        if (health <= 0) return;

        if (playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Check if player is in attack range
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            if (playerScript != null)
            {
                Attack(playerScript);
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
        if (isDying) return;

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
        if (isDying) return;
        isDying = true;

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
            manager.OnEnemyKilled(this);
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
        if (animController != null)
        {
            // Skadan kommer via Animation Event -> DealDamage()
            animController.PlayAttack();
        }
        else
        {
            // Fiender utan Animator (t.ex. en enkel sprite-prefab) slar direkt,
            // annars skulle de aldrig gora nagon skada alls.
            DealDamage();
        }
    }

    // Called by Animation Event
    public void DealDamage()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= attackRange)
        {
            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
        }
    }
}