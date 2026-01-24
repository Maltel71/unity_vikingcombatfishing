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
    [SerializeField] public float AttackPower = 25f;
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
    public AttackCollider attackCollider;
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

            // Trigger attack animation
            PlayerAnimationController animController = GetComponent<PlayerAnimationController>();
            if (animController != null)
            {
                animController.PlayAttack();
            }

            // Play sword swoosh sound
            if (swordSwooshSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(swordSwooshSound, attackSoundVolume);
            }

            // Apply Attack Speed cooldown
            nextAttackTime = Time.time + (1f / AttackSpeed);

            // Use the attack collider
            if (attackCollider != null)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        // Enable the attack collider
        attackCollider.EnableCollider();

        // Wait a tiny bit for collision detection
        yield return new WaitForSeconds(0.1f);

        // Activate the attack (damage all enemies in range)
        attackCollider.ActivateAttack(AttackPower);

        // Play enemy hit sound
        if (enemyHitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enemyHitSound, attackSoundVolume);
        }

        // Wait a bit more for the animation
        yield return new WaitForSeconds(0.2f);

        // Disable the collider
        attackCollider.DisableCollider();
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

    // Debug visualization - shows attack collider location
    void OnDrawGizmosSelected()
    {
        if (attackCollider != null)
        {
            // The AttackCollider will show its own collider in the scene view
        }
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

    // Public method to force player to face right (called by FishingRod)
    public void FaceRight()
    {
        if (!facingRight)
        {
            facingRight = true;
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x);
            transform.localScale = currentScale;
        }
    }
}