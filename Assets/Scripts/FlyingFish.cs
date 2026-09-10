using UnityEngine;

public class FlyingFish : MonoBehaviour
{
    [Header("Fish Stats")]
    public string fishName = "Fisk";
    public int healthValue = 20;
    public int scoreValue = 20;

    [Header("Freshness")]
    [Tooltip("Seconds the fish stays good after it comes out of the water. 0 keeps it fresh forever.")]
    public float freshDuration = 12f;
    [Tooltip("Health handed over once it has gone bad. Negative hurts Ragnar.")]
    public int spoiledHealthValue = -8;
    [Tooltip("Seconds the rotten fish lies there before the flies carry it off. 0 leaves it.")]
    public float spoiledDuration = 10f;
    [Tooltip("How long it takes to fade out when the flies are done with it.")]
    public float rotFadeDuration = 1.2f;
    [Tooltip("Colour laid over the sprite once it has gone bad.")]
    public Color spoiledTint = new Color(0.55f, 0.68f, 0.36f, 1f);
    [Tooltip("Buzz played now and then while the fish is rotten.")]
    public AudioClip flySound;
    [Range(0f, 1f)]
    public float flyVolume = 0.45f;
    [Tooltip("Shortest wait between buzzes from this fish.")]
    public float minFlyInterval = 3f;
    [Tooltip("Longest wait between buzzes from this fish.")]
    public float maxFlyInterval = 7f;
    [Range(0.5f, 2f)]
    public float minFlyPitch = 0.9f;
    [Range(0.5f, 2f)]
    public float maxFlyPitch = 1.2f;
    [Tooltip("Shortest gap between buzzes from any rotten fish, so a pile of them does not swarm.")]
    public float sharedFlyGap = 1.5f;

    [Header("Rage")]
    [Tooltip("Rage handed to Ragnar on pickup. Junk like the boot makes him angry, real fish give nothing.")]
    public float rageOnPickup = 0f;

    [Header("Difficulty")]
    [Tooltip("Multiplier on the rod bite delay. 0.5 bites twice as fast, 2 takes twice as long.")]
    public float biteTimeMultiplier = 1f;
    [Tooltip("Multiplier on reel time. Higher means a heavier fish that takes longer to land.")]
    public float reelDifficulty = 1f;

    [Header("Physics Settings")]
    public float gravityScale = 2f;

    [Header("Rotation Settings")]
    public float minAngularVelocity = -360f;
    public float maxAngularVelocity = 360f;

    [Header("Layer Change Settings")]
    public float timeUntilGroundLayer = 4f;

    [Header("Pickup Settings")]
    public Transform pickupRangeObject;

    [Header("Bounce Sound Effects")]
    public AudioClip[] bounceSounds;
    [Range(0f, 1f)]
    public float bounceVolume = 1f;
    public float bounceSoundCooldown = 0.2f;
    [Range(0.5f, 2f)]
    public float minBouncePitch = 0.8f;
    [Range(0.5f, 2f)]
    public float maxBouncePitch = 1.2f;

    [HideInInspector] public FishPile fishPile;

    private Rigidbody2D rb;
    private Collider2D fishCollider;
    private bool canPickup = false;
    private GameObject playerInRange = null;
    private float spawnTime;
    private bool layerChanged = false;
    private AudioSource audioSource;
    private float lastBounceSoundTime = 0f;
    private bool spoiled = false;
    private bool rotting = false;
    private float spoilTime;
    private float nextFlyTime;

    private static float nextSharedFly = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = gravityScale;
        rb.freezeRotation = false;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        rb.angularVelocity = Random.Range(minAngularVelocity, maxAngularVelocity);

        fishCollider = GetComponent<Collider2D>();
        if (fishCollider != null)
        {
            fishCollider.isTrigger = false;
        }

        if (pickupRangeObject != null)
        {
            Collider2D pickupCollider = pickupRangeObject.GetComponent<Collider2D>();
            if (pickupCollider != null)
            {
                pickupCollider.enabled = true;
                pickupCollider.isTrigger = true;
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        spawnTime = Time.time;
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            PickupFish();
        }

        if (!layerChanged && Time.time >= spawnTime + timeUntilGroundLayer)
        {
            ChangeToGroundLayer();
        }

        if (!spoiled && freshDuration > 0f && Time.time >= spawnTime + freshDuration)
        {
            Spoil();
        }

        if (spoiled && !rotting && spoiledDuration > 0f && Time.time >= spoilTime + spoiledDuration)
        {
            rotting = true;
            StartCoroutine(RotAway());
        }

        if (spoiled && !rotting)
        {
            BuzzWhenDue();
        }
    }

    void BuzzWhenDue()
    {
        if (flySound == null || audioSource == null) return;
        if (Time.time < nextFlyTime) return;

        nextFlyTime = Time.time + Random.Range(minFlyInterval, maxFlyInterval);

        if (Time.time < nextSharedFly) return;
        nextSharedFly = Time.time + sharedFlyGap;

        audioSource.pitch = Random.Range(minFlyPitch, maxFlyPitch);
        audioSource.PlayOneShot(flySound, flyVolume);
    }

    void Spoil()
    {
        spoiled = true;
        spoilTime = Time.time;
        healthValue = spoiledHealthValue;

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sprite.color = spoiledTint;
        }

        nextFlyTime = Time.time + Random.Range(minFlyInterval, maxFlyInterval) * 0.3f;
    }

    System.Collections.IEnumerator RotAway()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        float elapsed = 0f;
        Color start = sprite != null ? sprite.color : Color.white;

        while (elapsed < rotFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = rotFadeDuration > 0f ? elapsed / rotFadeDuration : 1f;

            if (sprite != null)
            {
                sprite.color = new Color(start.r, start.g, start.b, 1f - t);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {

        if (Time.time >= lastBounceSoundTime + bounceSoundCooldown)
        {
            PlayRandomBounceSound();
            lastBounceSoundTime = Time.time;
        }

        if (collision.gameObject.CompareTag("Ground") && !layerChanged)
        {
            ChangeToGroundLayer();
        }
    }

    void PlayRandomBounceSound()
    {
        if (bounceSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, bounceSounds.Length);

            audioSource.pitch = Random.Range(minBouncePitch, maxBouncePitch);

            audioSource.PlayOneShot(bounceSounds[randomIndex], bounceVolume);
        }
    }

    void ChangeToGroundLayer()
    {
        layerChanged = true;
        int groundFishLayer = LayerMask.NameToLayer("GroundFish");
        if (groundFishLayer != -1)
        {
            gameObject.layer = groundFishLayer;
        }
        else
        {
            Debug.LogWarning("GroundFish layer not found! Create it in Layer settings.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = true;
            playerInRange = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
            playerInRange = null;
        }
    }

    void PickupFish()
    {
        if (playerInRange != null)
        {
            PlayerScript player = playerInRange.GetComponent<PlayerScript>();
            if (player != null)
            {
                player.AddFishHealth(healthValue);
                RageMeter.Add(rageOnPickup);

                if (fishPile != null)
                {
                    fishPile.AddFishToPile(gameObject, healthValue, scoreValue);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
