using UnityEngine;

public class FlyingFish : MonoBehaviour
{
    [Header("Fisk Stats - STÄLL IN I INSPECTOR")]
    public string fishName = "Fisk";
    public int healthValue = 20;
    public int scoreValue = 20;

    [Header("Physics Settings")]
    public float gravityScale = 2f;

    [Header("Animation (Optional)")]
    public Animator fishAnimator;
    public string landAnimationTrigger = "Land";

    [HideInInspector] public FishPile fishPile;  // Sätts av FishingRod

    private Rigidbody2D rb;
    private Collider2D fishCollider;
    private bool hasLanded = false;
    private float landingSpeed = 0.5f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = gravityScale;
        rb.freezeRotation = false;

        fishCollider = GetComponent<Collider2D>();
        if (fishCollider != null)
        {
            fishCollider.isTrigger = true;
            fishCollider.enabled = false;
        }

        if (fishAnimator == null)
        {
            fishAnimator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (!hasLanded && rb != null)
        {
            if (rb.linearVelocity.magnitude < landingSpeed)
            {
                Land();
            }
        }
    }

    void Land()
    {
        hasLanded = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
        }

        if (fishCollider != null)
        {
            fishCollider.enabled = true;
        }

        if (fishAnimator != null && !string.IsNullOrEmpty(landAnimationTrigger))
        {
            fishAnimator.SetTrigger(landAnimationTrigger);
        }

        Debug.Log($"{fishName} landade! Gå och plocka upp den.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasLanded && other.CompareTag("Player"))
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player != null)
            {
                // Ge HP och poäng till spelaren
                player.CollectFish(healthValue, scoreValue);
                Debug.Log($"✅ Plockade upp {fishName}! +{healthValue} HP, +{scoreValue} poäng");

                // NYTT: Lägg i högen istället för att förstöra direkt
                if (fishPile != null)
                {
                    fishPile.AddFishToPile(gameObject, healthValue, scoreValue);
                }
                else
                {
                    // Om ingen fish pile finns, förstör som vanligt
                    Destroy(gameObject);
                }
            }
        }
    }
}