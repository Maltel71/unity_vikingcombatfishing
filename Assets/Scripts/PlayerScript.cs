using System.Collections;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] public int playerHealth = 100;
    [SerializeField] public int maxHealth = 100;
    [SerializeField] public int playerScore = 0;
    [Tooltip("Blood money, earned by killing gnomes and bosses.")]
    [SerializeField] public int bloodMoney = 0;
    [SerializeField] public int gnomesKilled = 0;
    [SerializeField] public int bossesKilled = 0;
    [SerializeField] public float playerSpeed = 5f;
    [SerializeField] public string playerName = "Ragnar";
    [Tooltip("Attacks per second. Cooldown = 1 / AttackSpeed.")]
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

    private FishingRod cachedFishingRod;
    private PlayerAnimationController cachedAnimController;

    void Start()
    {

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

    bool IsReeling()
    {
        return cachedFishingRod != null && cachedFishingRod.isReelingIn;
    }

    void HandleMovement()
    {

        if (IsReeling()) return;

        moveInput = Input.GetAxis("Horizontal");

        transform.Translate(Vector3.right * moveInput * playerSpeed * Time.deltaTime, Space.World);

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

        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextAttackTime)
        {

            if (cachedAnimController != null)
            {
                cachedAnimController.PlayAttack();
            }

            if (swordSwooshSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(swordSwooshSound, attackSoundVolume);
            }

            float safeAttackSpeed = Mathf.Max(0.01f, AttackSpeed);
            nextAttackTime = Time.time + (1f / safeAttackSpeed);

            if (attackCollider != null)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    IEnumerator AttackRoutine()
    {

        attackCollider.EnableCollider();

        yield return new WaitForSeconds(0.1f);

        attackCollider.ActivateAttack(AttackPower);

        if (enemyHitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(enemyHitSound, attackSoundVolume);
        }

        yield return new WaitForSeconds(0.2f);

        attackCollider.DisableCollider();
    }

    void HandleDance()
    {

        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrlHeld)
        {

            if (IsReeling()) return;
            if (!isAlive) return;

            if (cachedAnimController != null && !isDancing)
            {
                isDancing = true;
                cachedAnimController.StartDancing();

                EndlessWaveManager waves = FindFirstObjectByType<EndlessWaveManager>();
                SteamAchievements.OnDanceStarted(waves != null && waves.BossAlive);
            }
        }
        else
        {

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

        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

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

    public void CollectFish(int health, int score)
    {
        if (!isAlive) return;

        playerHealth = Mathf.Clamp(playerHealth + health, 0, maxHealth);

        playerScore = Mathf.Max(0, playerScore + score);

        SteamAchievements.OnScoreChanged(TotalScore);

        if (playerHealth <= 0)
        {
            Die();
        }
    }

    public int TotalScore { get { return playerScore + bloodMoney; } }

    public void AddBloodMoney(int amount, bool wasBoss)
    {
        if (!isAlive) return;

        bloodMoney = Mathf.Max(0, bloodMoney + amount);

        SteamAchievements.OnScoreChanged(TotalScore);

        if (wasBoss) bossesKilled++;
        else gnomesKilled++;
    }

    void Die()
    {
        isAlive = false;
        Debug.Log($"{playerName} has perished in battle.");

    }

    void PlayerInteract()
    {

    }

    void FlipCharacter()
    {

        if (IsReeling()) return;

        facingRight = !facingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }

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
