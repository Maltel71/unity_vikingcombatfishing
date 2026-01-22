using UnityEngine;

public class FlyingFish : MonoBehaviour
{
    [Header("Fisk Stats - STÄLL IN I INSPECTOR")]
    public string fishName = "Fisk";
    public int healthValue = 20;
    public int scoreValue = 20;

    [Header("Physics Settings")]
    public float gravityScale = 2f;
    public float bounceForce = 0.3f;

    [Header("Landing Detection")]
    public float landingSpeed = 0.5f;
    public float minFallTime = 1.0f;

    [Header("Animation (Optional)")]
    public Animator fishAnimator;
    public string landAnimationTrigger = "Land";

    [Header("Pickup Settings")]
    public Transform pickupRangeObject;

    [HideInInspector] public FishPile fishPile;

    private Rigidbody2D rb;
    private Collider2D fishCollider;
    private bool hasLanded = false;
    private bool hasHitCeiling = false;
    private bool hasGivenReward = false;
    private float spawnTime;

    private bool canPickup = false;
    private GameObject playerInRange = null;

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

        fishCollider = GetComponent<Collider2D>();
        if (fishCollider != null)
        {
            fishCollider.isTrigger = false;
        }

        spawnTime = Time.time;
    }

    void Update()
    {
        if (!hasLanded && rb != null)
        {
            float timeSinceSpawn = Time.time - spawnTime;

            if (hasHitCeiling && timeSinceSpawn > minFallTime && rb.linearVelocity.magnitude < landingSpeed)
            {
                Land();
            }
        }

        // Check for pickup input
        if (hasLanded && canPickup && Input.GetKeyDown(KeyCode.E))
        {
            PickupFish();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"🐟 {fishName} träffade: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        if (!hasHitCeiling && collision.gameObject.CompareTag("Wall_Up"))
        {
            hasHitCeiling = true;
            Debug.Log($"💥 {fishName} BONK på taket!");

            Vector2 bounceVelocity = new Vector2(rb.linearVelocity.x, -Mathf.Abs(rb.linearVelocity.y) * bounceForce);
            rb.linearVelocity = bounceVelocity;
            rb.angularVelocity = Random.Range(-360f, 360f);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log($"🌍 {fishName} träffade marken!");

            if (!hasLanded)
            {
                Land();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!hasLanded && collision.gameObject.CompareTag("Ground"))
        {
            Land();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasLanded && other.CompareTag("Player"))
        {
            canPickup = true;
            playerInRange = other.gameObject;
            Debug.Log($"Press E to pick up {fishName}");
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

    void Land()
    {
        if (hasLanded) return;

        hasLanded = true;
        Debug.Log($"✅ {fishName} LANDADE!");

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (fishCollider != null)
        {
            fishCollider.isTrigger = false;
            fishCollider.enabled = false; // Disable main collider
        }

        // Enable the pickup range collider
        if (pickupRangeObject != null)
        {
            Collider2D pickupCollider = pickupRangeObject.GetComponent<Collider2D>();
            if (pickupCollider != null)
            {
                pickupCollider.enabled = true;
                pickupCollider.isTrigger = true;
            }
        }

        if (fishAnimator != null && !string.IsNullOrEmpty(landAnimationTrigger))
        {
            fishAnimator.SetTrigger(landAnimationTrigger);
        }

        GiveRewardToPlayer();
    }

    void GiveRewardToPlayer()
    {
        if (hasGivenReward) return;
        hasGivenReward = true;

        Debug.Log($"{fishName} landed! Press E to pick up.");
    }

    void PickupFish()
    {
        if (playerInRange != null)
        {
            PlayerScript player = playerInRange.GetComponent<PlayerScript>();
            if (player != null)
            {
                player.CollectFish(healthValue, scoreValue);
                Debug.Log($"Picked up {fishName}! +{healthValue} HP, +{scoreValue} points");

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