using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] public int playerHealth = 100;
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public int playerScore = 0;
    [SerializeField] public float playerSpeed = 5f;
    [SerializeField] public string playerName = "Ragnar";
    [SerializeField] public float AttackSpeed = 0.1f;
    [SerializeField] public float AttackPower = 100f;
    [SerializeField] public bool isAlive = true;

    [Header("Sound Effects")]
    public AudioClip[] hurtSounds;
    [Range(0f, 1f)]
    public float hurtSoundVolume = 1f;
    public AudioClip swordSwooshSound;
    public AudioClip enemyHitSound;
    [Range(0f, 1f)]
    public float attackSoundVolume = 1f;
    private AudioSource audioSource;

    [Header("Visual Effects")]
    public ParticleSystem bloodParticle;

    [Header("Animation")]
    public string danceAnimationName = "danceanimragnar";
    private bool isDancing = false;

    [Header("Movement Settings")]
    private float moveInput;
    private bool facingRight = true;

    [Header("2D Combat Settings")]
    public float attackRange = 1.2f;
    public float attackOffset = 1.0f;
    private float nextAttackTime = 0f;

    void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (!isAlive) return;

        HandleMovement();
        HandleAttack();
        HandleDance();
        PlayerInteract();
    }

    void HandleMovement()
    {
        // Check if fishing rod exists and is reeling - disable movement
        FishingRod fishingRod = GetComponentInChildren<FishingRod>();
        if (fishingRod != null && fishingRod.isReelingIn)
        {
            return;
        }

        moveInput = Input.GetAxis("Horizontal");

        // Apply Movement
        transform.Translate(Vector3.right * moveInput * playerSpeed * Time.deltaTime, Space.World);

        // Flip Logic 
        if (moveInput > 0 && !facingRight)
        {
            FlipCharacter();
            Debug.Log("Flipped Right");
        }
        else if (moveInput < 0 && facingRight)
        {
            FlipCharacter();
            Debug.Log("Flipped Left");
        }

        // Only Log if moving to avoid console spam
        if (moveInput != 0)
            Debug.Log($"{playerName} is moving.");
    }

    void HandleAttack()
    {
        // Space to attack
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextAttackTime)
        {
            Debug.Log($"{playerName} swings his weapon with the Spacebar!");

            // Play sword swoosh sound
            if (swordSwooshSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(swordSwooshSound, attackSoundVolume);
            }

            // Apply Attack Speed cooldown
            nextAttackTime = Time.time + (1f / AttackSpeed);

            // Logic to check if an enemy is hit
            CheckForEnemyHit();
        }
    }

    void HandleDance()
    {
        FishingRod fishingRod = GetComponentInChildren<FishingRod>();
        PlayerAnimationController animController = GetComponent<PlayerAnimationController>();

        // Check if Ctrl is being held
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrlHeld)
        {
            // Don't dance if reeling, dead, or during other action animation
            if (fishingRod != null && fishingRod.isReelingIn) return;
            if (!isAlive) return;

            // Start dancing (will loop while held)
            if (animController != null && !isDancing)
            {
                isDancing = true;
                animController.StartDancing();
            }
        }
        else
        {
            // Stop dancing when Ctrl is released
            if (isDancing)
            {
                isDancing = false;
                if (animController != null)
                {
                    animController.StopDancing();
                }
            }
        }
    }

    void CheckForEnemyHit()
    {
        float faceDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackPoint = (Vector2)transform.position + new Vector2(faceDir * attackOffset, 0);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange);

        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyScript enemy = hit.GetComponent<EnemyScript>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)AttackPower);

                    // Play enemy hit sound
                    if (enemyHitSound != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(enemyHitSound, attackSoundVolume);
                    }

                    Debug.Log("Hit an enemy for " + AttackPower + " damage!");
                    break; // Only hit ONE enemy per attack
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float faceDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackPoint = (Vector2)transform.position + new Vector2(faceDir * attackOffset, 0);
        Gizmos.DrawWireSphere(attackPoint, attackRange);
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive || damage <= 0) return;

        playerHealth -= damage;
        Debug.Log($"{playerName} took {damage} damage! HP: {playerHealth}");

        // Play blood particle effect
        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

        // Play random hurt sound
        if (hurtSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, hurtSounds.Length);
            audioSource.PlayOneShot(hurtSounds[randomIndex], hurtSoundVolume);
        }

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
    }

    public void CollectFish(int health, int score)
    {
        if (!isAlive) return;

        // Add HP (max up to maxHealth)
        playerHealth = Mathf.Min(playerHealth + health, maxHealth);

        // Add score
        playerScore += score;

        Debug.Log($"{playerName} fick +{health} HP och +{score} poäng! Total HP: {playerHealth}/{maxHealth}, Score: {playerScore}");
    }

    void Die()
    {
        isAlive = false;
        Debug.Log($"{playerName} has perished in battle.");
        // Don't destroy - let death animation play
    }

    void PlayerInteract()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(playerName + " is attempting to interact...");
        }
    }

    void FlipCharacter()
    {
        // Don't flip while reeling
        FishingRod fishingRod = GetComponentInChildren<FishingRod>();
        if (fishingRod != null && fishingRod.isReelingIn)
        {
            return;
        }

        facingRight = !facingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }
}