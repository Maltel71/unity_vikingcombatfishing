using UnityEngine;

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
    private Transform playerTransform;

    [Header("Wave System Connection")]
    public EndlessWaveManager manager;

    [Header("Sound Effects")]
    public AudioClip[] hurtSounds;
    public AudioClip[] idleSounds;
    [Range(0f, 1f)]
    public float enemySoundVolume = 1f;

    [Header("Idle Sound Settings")]
    public float minIdleSoundTime = 3f;
    public float maxIdleSoundTime = 8f;
    private float nextIdleSoundTime;

    private AudioSource audioSource;

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

        switch (type)
        {
            case GnomeType.Garden:
                gnomeName = "C_Common_Gnome";
                health = 5;
                damage = 10;
                movementSpeed = 0.3f;
                break;
            case GnomeType.Berserker:
                gnomeName = "C_Berserker_Gnome";
                health = 100;
                damage = 5;
                movementSpeed = 1.5f;
                break;
            case GnomeType.Brute:
                gnomeName = "C_Brute_Gnome";
                health = 200;
                damage = 20;
                movementSpeed = 0.1f;
                break;
        }
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

        // Play hurt sound
        if (hurtSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, hurtSounds.Length);
            audioSource.PlayOneShot(hurtSounds[randomIndex], enemySoundVolume);
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Hide sprite
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
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

            // Destroy after particle finishes
            Destroy(gameObject, bloodParticle.main.duration);
        }
        else
        {
            Destroy(gameObject);
        }

        if (manager != null)
        {
            manager.OnGnomeKilled();
        }
    }

    void PlayRandomIdleSound()
    {
        if (audioSource != null && idleSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, idleSounds.Length);
            audioSource.PlayOneShot(idleSounds[randomIndex], enemySoundVolume);
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
        player.TakeDamage(damage);
    }
}