using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] public int playerHealth = 100;
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public int playerScore = 0;
    [Tooltip("Blodspengar - tjanas pa att doda gnomer och bossar.")]
    [SerializeField] public int bloodMoney = 0;
    [SerializeField] public int gnomesKilled = 0;
    [SerializeField] public int bossesKilled = 0;
    [SerializeField] public float playerSpeed = 5f;
    [SerializeField] public string playerName = "Ragnar";
    [Tooltip("Attacker per sekund. Cooldown = 1 / AttackSpeed.")]
    [SerializeField] public float AttackSpeed = 1.5f;
    [SerializeField] public float AttackPower = 50f;
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

    // Cachade komponenter - slipper GetComponent varje frame
    private FishingRod cachedFishingRod;
    private PlayerAnimationController cachedAnimController;

    void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        cachedFishingRod = GetComponentInChildren<FishingRod>();
        cachedAnimController = GetComponent<PlayerAnimationController>();
    }

    void Update()
    {
        if (!isAlive) return;
        if (PauseMenu.IsPaused) return;

        HandleMovement();
        HandleAttack();
        HandleDance();
        PlayerInteract();
    }

    // Sant nar spelaren haller pa att veva in en fisk - da ar rorelse och flip last
    bool IsReeling()
    {
        return cachedFishingRod != null && cachedFishingRod.isReelingIn;
    }

    void HandleMovement()
    {
        // Rorelse ar avstangd medan man vevar in
        if (IsReeling()) return;

        moveInput = Input.GetAxis("Horizontal");

        // Apply Movement
        transform.Translate(Vector3.right * moveInput * playerSpeed * Time.deltaTime, Space.World);

        // Flip Logic
        if (moveInput > 0 && !facingRight)
        {
            FlipCharacter();
        }
        else if (moveInput < 0 && facingRight)
        {
            FlipCharacter();
        }
    }

    void HandleAttack()
    {
        // Space to attack
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextAttackTime)
        {
            // Trigger attack animation
            if (cachedAnimController != null)
            {
                cachedAnimController.PlayAttack();
            }

            // Play sword swoosh sound
            if (swordSwooshSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(swordSwooshSound, attackSoundVolume);
            }

            // Apply Attack Speed cooldown (skyddad mot 0 som skulle ge division med noll)
            float safeAttackSpeed = Mathf.Max(0.01f, AttackSpeed);
            nextAttackTime = Time.time + (1f / safeAttackSpeed);

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
        // Check if Ctrl is being held
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrlHeld)
        {
            // Don't dance if reeling, dead, or during other action animation
            if (IsReeling()) return;
            if (!isAlive) return;

            // Start dancing (will loop while held)
            if (cachedAnimController != null && !isDancing)
            {
                isDancing = true;
                cachedAnimController.StartDancing();
            }
        }
        else
        {
            // Stop dancing when Ctrl is released
            if (isDancing)
            {
                isDancing = false;
                if (cachedAnimController != null)
                {
                    cachedAnimController.StopDancing();
                }
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive || damage <= 0) return;

        playerHealth -= damage;

        // Play blood particle effect
        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

        // Play random hurt sound
        if (hurtSounds != null && hurtSounds.Length > 0 && audioSource != null)
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

    // Anropas nar spelaren plockar upp en fangst.
    // health och score kan vara negativa (t.ex. stoveln) - darfor clampas badadera.
    public void CollectFish(int health, int score)
    {
        if (!isAlive) return;

        // Add HP (never below 0, never above maxHealth)
        playerHealth = Mathf.Clamp(playerHealth + health, 0, maxHealth);

        // Add score (never below 0)
        playerScore = Mathf.Max(0, playerScore + score);

        if (playerHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>Fiskepoang + blodspengar. Det ar den har summan som hamnar pa highscore-listan.</summary>
    public int TotalScore { get { return playerScore + bloodMoney; } }

    /// <summary>Anropas av EndlessWaveManager nar en fiende dor.</summary>
    public void AddBloodMoney(int amount, bool wasBoss)
    {
        if (!isAlive) return;

        bloodMoney = Mathf.Max(0, bloodMoney + amount);

        if (wasBoss) bossesKilled++;
        else gnomesKilled++;
    }

    void Die()
    {
        isAlive = false;
        Debug.Log($"{playerName} has perished in battle.");
        // Don't destroy - let death animation play
    }

    void PlayerInteract()
    {
        // Placeholder for framtida interaktioner (E anvands av FishingRod och FlyingFish)
    }

    void FlipCharacter()
    {
        // Don't flip while reeling
        if (IsReeling()) return;

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
