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

    [HideInInspector] public bool isElite = false;

    [HideInInspector] public bool externalBehaviour = false;

    public int MaxHealth { get; private set; }
    [HideInInspector] public float eliteHealthMultiplier = 1f;
    [HideInInspector] public float eliteDamageMultiplier = 1f;
    [HideInInspector] public float eliteSizeMultiplier = 1.6f;
    [HideInInspector] public float eliteSpeedMultiplier = 1.5f;
    [HideInInspector] public float eliteAttackSpeedMultiplier = 1f;
    [HideInInspector] public Color eliteTint = Color.white;
    [HideInInspector] public float eliteReachBonus = 0f;

    [Header("Auto Setup")]
    [Tooltip("Work out Attack Range from the collider once the boss size multiplier has been applied.")]
    public bool autoAttackRange = true;
    [Tooltip("How far past its own body the enemy can reach. Covers Ragnar's width plus a little slack.")]
    public float reachPadding = 1f;
    [Tooltip("Drop onto the ground at spawn so a tall boss never starts buried or floating.")]
    public bool snapToGroundOnSpawn = true;

    [Tooltip("Keep swinging on the cooldown even when Ragnar is out of reach. For an enemy that cannot walk, so it still looms instead of waiting to be approached.")]
    public bool swingWhenOutOfReach = false;

    [Header("Knockback")]
    [Tooltip("World units this enemy is shoved back when hit. 0 means it stands its ground.")]
    public float knockbackDistance = 0f;
    public float knockbackDuration = 0.15f;
    [Tooltip("Ignore knockback while the enemy is mid swing, so a hit cannot cancel its attack.")]
    public bool knockbackOnlyBetweenAttacks = false;
    [Tooltip("Only hits of at least this much damage shove the enemy. Set it above a normal swing so only heavy blows land a stagger.")]
    public int knockbackMinDamage = 0;
    [Tooltip("Shortest time between two shoves, so fast hits cannot push an enemy across the map.")]
    public float knockbackCooldown = 0.5f;
    [Tooltip("Slide back to where it stood once the shove is done. Keeps the punch without letting a slow enemy be pushed out of the fight.")]
    public bool knockbackReturn = true;

    [Header("Debug")]
    [Tooltip("Print distance and attack state to the console once a second.")]
    public bool logCombat = false;
    private float nextLogTime = 0f;

    [Header("When Ragnar Dies")]
    [Tooltip("Fade away once the player is dead instead of standing frozen.")]
    public bool vanishOnPlayerDeath = true;
    [Tooltip("Seconds to stand still before fading out.")]
    public float vanishDelay = 1f;
    public float vanishFadeDuration = 1f;

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
    private bool stoodDown = false;
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

        if (snapToGroundOnSpawn) SnapToGround();

        if (autoAttackRange) FitAttackRange();
        else attackRange += eliteReachBonus;

        MaxHealth = Mathf.Max(1, health);
    }

    Collider2D BodyCollider()
    {
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            if (col != null && col.enabled) return col;
        }

        return null;
    }

    void FitAttackRange()
    {
        Collider2D body = BodyCollider();
        if (body == null) return;

        attackRange = body.bounds.extents.x + reachPadding + eliteReachBonus;

        if (logCombat)
        {
            Debug.Log(name + "  auto attack range = " + attackRange.ToString("F2")
                + "  (half width " + body.bounds.extents.x.ToString("F2") + ")");
        }
    }

    void SnapToGround()
    {
        Collider2D body = BodyCollider();
        if (body == null) return;

        Vector2 origin = new Vector2(transform.position.x, transform.position.y + 5f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, Vector2.down, 40f);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null) continue;
            if (hit.collider.gameObject == gameObject) continue;
            if (!hit.collider.CompareTag("Ground")) continue;

            float lift = hit.point.y - body.bounds.min.y;
            transform.position += new Vector3(0f, lift, 0f);
            return;
        }
    }

    void ApplyVariations()
    {
        if (isElite)
        {

            transform.localScale *= eliteSizeMultiplier;
            movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed) * eliteSpeedMultiplier;
            health = Mathf.Max(1, Mathf.RoundToInt(health * eliteHealthMultiplier));
            damage = Mathf.Max(1, Mathf.RoundToInt(damage * eliteDamageMultiplier));
            attackSpeed = Mathf.Max(0.05f, attackSpeed * eliteAttackSpeedMultiplier);

            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null && eliteTint.a > 0f)
            {
                sprite.color = eliteTint;
            }
            return;
        }

        float sizeMultiplier = Random.Range(minSizeMultiplier, maxSizeMultiplier);
        transform.localScale *= sizeMultiplier;

        movementSpeed = Random.Range(minMovementSpeed, maxMovementSpeed);

    }

    void Update()
    {
        if (PlayerScript.IsGameOver)
        {
            StandDown();
            return;
        }

        if (health <= 0) return;
        if (externalBehaviour) return;

        if (playerTransform == null)
        {
            return;
        }

        float distanceToPlayer = HorizontalDistanceToPlayer();

        if (logCombat && Time.time >= nextLogTime)
        {
            nextLogTime = Time.time + 1f;
            Debug.Log(name + "  dist=" + distanceToPlayer.ToString("F2")
                + "  range=" + attackRange
                + "  inRange=" + (distanceToPlayer <= attackRange)
                + "  cooldownReady=" + (Time.time >= nextAttackTime)
                + "  playerScript=" + (playerScript != null)
                + "  animController=" + (animController != null));
        }

        if ((distanceToPlayer <= attackRange || swingWhenOutOfReach) && Time.time >= nextAttackTime)
        {
            if (playerScript != null)
            {
                Attack(playerScript);
                nextAttackTime = Time.time + (1f / attackSpeed);
            }
        }
        else if (distanceToPlayer > attackRange && !knockingBack)
        {

            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0;
            transform.position += direction * movementSpeed * Time.deltaTime;
        }

        if (Time.time >= nextIdleSoundTime && idleSounds.Length > 0)
        {
            PlayRandomIdleSound();
            nextIdleSoundTime = Time.time + Random.Range(minIdleSoundTime, maxIdleSoundTime);
        }
    }

    void StandDown()
    {
        if (stoodDown) return;
        stoodDown = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (vanishOnPlayerDeath && !isDying)
        {
            StartCoroutine(VanishAfterDelay());
        }
    }

    IEnumerator VanishAfterDelay()
    {
        yield return new WaitForSeconds(vanishDelay);

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        Color start = sprite != null ? sprite.color : Color.white;

        while (elapsed < vanishFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = vanishFadeDuration > 0f ? elapsed / vanishFadeDuration : 1f;

            if (sprite != null)
            {
                sprite.color = new Color(start.r, start.g, start.b, start.a * (1f - t));
            }

            yield return null;
        }

        Destroy(gameObject);
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
            return;
        }

        if (animController != null) animController.PlayHurt();

        if (ShouldKnockBack(amount)) StartCoroutine(Knockback());
    }

    private bool knockingBack = false;
    private float nextKnockbackTime = 0f;

    bool ShouldKnockBack(int amount)
    {
        if (knockbackDistance <= 0f || knockingBack) return false;
        if (amount < knockbackMinDamage) return false;
        if (Time.time < nextKnockbackTime) return false;
        if (knockbackOnlyBetweenAttacks && animController != null && animController.IsSwinging) return false;

        nextKnockbackTime = Time.time + knockbackCooldown;
        return true;
    }

    IEnumerator Knockback()
    {
        if (playerTransform == null) yield break;

        knockingBack = true;

        float away = transform.position.x >= playerTransform.position.x ? 1f : -1f;
        float homeX = transform.position.x;
        float targetX = homeX + away * knockbackDistance;

        yield return SlideTo(homeX, targetX, knockbackDuration);

        if (knockbackReturn && !isDying)
        {
            yield return SlideTo(targetX, homeX, knockbackDuration * 1.6f);
        }

        knockingBack = false;
    }

    IEnumerator SlideTo(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            SetX(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration && !isDying)
        {
            elapsed += Time.deltaTime;
            float step = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            SetX(Mathf.Lerp(from, to, step));
            yield return null;
        }

        if (!isDying) SetX(to);
    }

    void SetX(float x)
    {
        Vector3 pos = transform.position;
        pos.x = x;
        transform.position = pos;
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

        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
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
        if (logCombat) Debug.Log(name + "  ATTACK called, via animator = " + (animController != null));

        if (animController != null)
        {

            animController.PlayAttack();
        }
        else
        {

            DealDamage();
        }
    }

    float HorizontalDistanceToPlayer()
    {
        if (playerTransform == null) return float.MaxValue;

        return Mathf.Abs(playerTransform.position.x - transform.position.x);
    }

    public bool DamageEventFired { get; private set; }

    public void DealDamage()
    {
        if (animController != null)
        {
            if (animController.SuppressDamage) return;
            DealDamageScaled(animController.CurrentAttackMultiplier);
            return;
        }

        DealDamageScaled(1f);
    }

    public void DealDamageScaled(float damageMultiplier)
    {
        DamageEventFired = true;

        if (playerTransform == null)
        {
            if (logCombat) Debug.LogWarning(name + "  DealDamage fired but it never found Ragnar.");
            return;
        }

        float distanceToPlayer = HorizontalDistanceToPlayer();
        int blow = Mathf.Max(1, Mathf.RoundToInt(damage * damageMultiplier));

        if (logCombat)
        {
            Debug.Log(name + "  DealDamage fired  dist=" + distanceToPlayer.ToString("F2")
                + "  range=" + attackRange.ToString("F2")
                + "  inRange=" + (distanceToPlayer <= attackRange)
                + "  blow=" + blow
                + "  playerScript=" + (playerScript != null));
        }

        if (distanceToPlayer <= attackRange && playerScript != null)
        {
            playerScript.TakeDamage(blow);
        }
    }
}
