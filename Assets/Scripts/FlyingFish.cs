using UnityEngine;

public class FlyingFish : MonoBehaviour
{
    [Header("Fish Stats")]
    public string fishName = "Fisk";
    public int healthValue = 20;
    public int scoreValue = 20;

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
                player.CollectFish(healthValue, scoreValue);

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
