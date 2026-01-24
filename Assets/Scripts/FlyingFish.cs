using UnityEngine;

public class FlyingFish : MonoBehaviour
{
    [Header("Fisk Stats - STÄLL IN I INSPECTOR")]
    public string fishName = "Fisk";
    public int healthValue = 20;
    public int scoreValue = 20;

    [Header("Physics Settings")]
    public float gravityScale = 2f;

    [Header("Rotation Settings")]
    public float minAngularVelocity = -360f;
    public float maxAngularVelocity = 360f;

    [Header("Layer Change Settings")]
    public float timeUntilGroundLayer = 4f;

    [Header("Pickup Settings")]
    public Transform pickupRangeObject;

    [HideInInspector] public FishPile fishPile;

    private Rigidbody2D rb;
    private Collider2D fishCollider;
    private bool canPickup = false;
    private GameObject playerInRange = null;
    private float spawnTime;
    private bool layerChanged = false;

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

        // Apply random rotation spin
        rb.angularVelocity = Random.Range(minAngularVelocity, maxAngularVelocity);

        fishCollider = GetComponent<Collider2D>();
        if (fishCollider != null)
        {
            fishCollider.isTrigger = false;
        }

        // Enable the pickup range collider immediately
        if (pickupRangeObject != null)
        {
            Collider2D pickupCollider = pickupRangeObject.GetComponent<Collider2D>();
            if (pickupCollider != null)
            {
                pickupCollider.enabled = true;
                pickupCollider.isTrigger = true;
            }
        }

        spawnTime = Time.time;
    }

    void Update()
    {
        // Check for pickup input
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            PickupFish();
        }

        // Change layer after time has passed
        if (!layerChanged && Time.time >= spawnTime + timeUntilGroundLayer)
        {
            ChangeToGroundLayer();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !layerChanged)
        {
            ChangeToGroundLayer();
        }
    }

    void ChangeToGroundLayer()
    {
        layerChanged = true;
        int groundFishLayer = LayerMask.NameToLayer("GroundFish");
        if (groundFishLayer != -1)
        {
            gameObject.layer = groundFishLayer;
            Debug.Log($"{fishName} changed to GroundFish layer");
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