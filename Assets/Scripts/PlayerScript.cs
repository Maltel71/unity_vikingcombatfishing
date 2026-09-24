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

    public static bool IsGameOver { get; private set; }

    [Header("Sound Effects")]
    public AudioClip[] hurtSounds;
    [Range(0f, 1f)]
    public float hurtSoundVolume = 1f;
    public AudioClip swordSwooshSound;
    [Tooltip("Impact sound. Only plays when the swing actually connects.")]
    public AudioClip enemyHitSound;
    [Range(0f, 1f)]
    public float attackSoundVolume = 1f;
    [Tooltip("Random pitch range on the impact so repeated hits do not sound identical.")]
    [Range(0.5f, 2f)]
    public float minHitPitch = 0.92f;
    [Range(0.5f, 2f)]
    public float maxHitPitch = 1.08f;

    [Header("Berserk Sound")]
    [Tooltip("Drop a separate berserk clip here. Left empty, the normal swing and impact sounds are used, just louder.")]
    public AudioClip berserkSound;
    [Range(0f, 1f)]
    public float berserkSoundVolume = 1f;
    [Tooltip("How much louder the berserk swing is than a normal one.")]
    [Range(1f, 3f)]
    public float berserkVolumeBoost = 1.5f;
    [Tooltip("Seconds into the berserk swing before the blow lands. Match it to the frame where the axe connects.")]
    public float berserkHitDelay = 0.1f;
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
    private RageMeter rage;
    private bool danceBlocked = false;

    void Start()
    {

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        IsGameOver = false;

        cachedFishingRod = GetComponentInChildren<FishingRod>();
        cachedAnimController = GetComponent<PlayerAnimationController>();
        rage = GetComponent<RageMeter>();
    }

    void Update()
    {
        if (!isAlive) return;
        if (PauseMenu.IsPaused) return;

        HandleMovement();
        HandleAttack();
        HandleBerserk();
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
                PlayOnPlayer(swordSwooshSound, attackSoundVolume, 1f);
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

        int hits = attackCollider.ActivateAttack(AttackPower);

        if (rage == null) rage = GetComponent<RageMeter>();
        if (rage != null) rage.AddHits(hits);

        PlayImpact(hits);

        yield return new WaitForSeconds(0.2f);

        attackCollider.DisableCollider();
    }

    void HandleBerserk()
    {
        if (rage == null) rage = GetComponent<RageMeter>();
        if (rage == null || !rage.IsFull) return;
        if (IsReeling()) return;

        bool pressed = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
        if (!pressed) return;

        if (!rage.Consume()) return;

        danceBlocked = true;

        if (isDancing)
        {
            isDancing = false;
            if (cachedAnimController != null) cachedAnimController.StopDancing();
        }

        StartCoroutine(BerserkRoutine());
    }

    IEnumerator BerserkRoutine()
    {
        if (cachedAnimController != null)
        {
            cachedAnimController.PlayBerserk();
        }

        StartCoroutine(BerserkRoar());

        nextAttackTime = Time.time + (1f / Mathf.Max(0.01f, AttackSpeed));

        if (attackCollider == null) yield break;

        attackCollider.EnableCollider();

        yield return new WaitForSeconds(Mathf.Max(0f, berserkHitDelay));

        int hits = attackCollider.ActivateAttack(AttackPower * rage.damageMultiplier);

        PlayImpact(hits, Random.Range(minHitPitch, maxHitPitch), BerserkVolume());

        yield return new WaitForSeconds(Mathf.Max(0.05f, rage.swingDuration));

        attackCollider.DisableCollider();
    }

    void PlayImpact(int hits)
    {
        PlayImpact(hits, Random.Range(minHitPitch, maxHitPitch));
    }

    void PlayImpact(int hits, float pitch)
    {
        PlayImpact(hits, pitch, attackSoundVolume);
    }

    void PlayImpact(int hits, float pitch, float volume)
    {
        if (hits <= 0) return;
        if (enemyHitSound == null || audioSource == null) return;

        PlayOnPlayer(enemyHitSound, volume, pitch);
    }

    IEnumerator BerserkRoar()
    {
        if (audioSource == null) yield break;

        if (berserkSound != null)
        {
            PlayDetached(berserkSound, berserkSoundVolume, 1f);
            yield break;
        }

        PlayDetached(swordSwooshSound, BerserkVolume(), 1f);
    }

    float BerserkVolume()
    {
        return Mathf.Clamp01(attackSoundVolume * berserkVolumeBoost);
    }

    void PlayOnPlayer(AudioClip clip, float volume, float pitch)
    {
        if (clip == null || audioSource == null) return;

        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume);
    }

    void PlayDetached(AudioClip clip, float volume, float pitch)
    {
        if (clip == null) return;

        float safePitch = Mathf.Max(0.05f, Mathf.Abs(pitch));

        GameObject go = new GameObject("BerserkVoice");
        go.transform.position = transform.position;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = pitch;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(go, (clip.length / safePitch) + 0.2f);
    }

    void HandleDance()
    {

        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (!ctrlHeld) danceBlocked = false;

        if (ctrlHeld && !danceBlocked)
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

        int lost = Mathf.Min(damage, playerHealth);
        playerHealth -= damage;

        HealthPopup.Show(-lost);

        if (rage == null) rage = GetComponent<RageMeter>();
        if (rage != null) rage.LoseOnDamage();

        if (bloodParticle != null)
        {
            bloodParticle.Play();
        }

        if (hurtSounds != null && hurtSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, hurtSounds.Length);
            PlayOnPlayer(hurtSounds[randomIndex], hurtSoundVolume, 1f);
        }

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
    }

    public void AddFishScore(int score)
    {
        if (!isAlive || score == 0) return;

        playerScore = Mathf.Max(0, playerScore + score);
        SteamAchievements.OnScoreChanged(TotalScore);
    }

    public void AddFishHealth(int health)
    {
        if (!isAlive || health == 0) return;

        playerHealth = Mathf.Clamp(playerHealth + health, 0, maxHealth);
        HealthPopup.Show(health);

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
        IsGameOver = true;
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
