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

    [HideInInspector] public FishPile fishPile;

    private Rigidbody2D rb;
    private Collider2D fishCollider;
    private bool hasLanded = false;
    private bool hasHitCeiling = false;
    private bool hasGivenReward = false;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = gravityScale;
        rb.freezeRotation = false;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // Bättre kollision!

        fishCollider = GetComponent<Collider2D>();
        if (fishCollider != null)
        {
            fishCollider.isTrigger = false;  // MÅSTE vara false för att kollidera!
        }

        spawnTime = Time.time;
    }

    void Update()
    {
        if (!hasLanded && rb != null)
        {
            float timeSinceSpawn = Time.time - spawnTime;

            // Enklare landningscheck
            if (hasHitCeiling && timeSinceSpawn > minFallTime && rb.linearVelocity.magnitude < landingSpeed)
            {
                Land();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug för att se vad fisken träffar
        Debug.Log($"🐟 {fishName} träffade: {collision.gameObject.name} (Tag: {collision.gameObject.tag})");

        // Kolla om fisken träffar taket
        if (!hasHitCeiling && collision.gameObject.CompareTag("Wall_Up"))
        {
            hasHitCeiling = true;
            Debug.Log($"💥 {fishName} BONK på taket!");

            Vector2 bounceVelocity = new Vector2(rb.linearVelocity.x, -Mathf.Abs(rb.linearVelocity.y) * bounceForce);
            rb.linearVelocity = bounceVelocity;
            rb.angularVelocity = Random.Range(-360f, 360f);
        }

        // Kolla om fisken träffar marken - LANDA DIREKT!
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
        // Om fisken fortsätter vara på marken, landa!
        if (!hasLanded && collision.gameObject.CompareTag("Ground"))
        {
            Land();
        }
    }

    void Land()
    {
        if (hasLanded) return;

        hasLanded = true;
        Debug.Log($"✅ {fishName} LANDADE!");

        // STOPPA ALL RÖRELSE!
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Nu blir det trigger så spelaren kan plocka upp
        if (fishCollider != null)
        {
            fishCollider.isTrigger = true;
        }

        if (fishAnimator != null && !string.IsNullOrEmpty(landAnimationTrigger))
        {
            fishAnimator.SetTrigger(landAnimationTrigger);
        }

        // Ge belöning
        GiveRewardToPlayer();
    }

    void GiveRewardToPlayer()
    {
        if (hasGivenReward) return;
        hasGivenReward = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerScript player = playerObj.GetComponent<PlayerScript>();
            if (player != null)
            {
                player.CollectFish(healthValue, scoreValue);
                Debug.Log($"💰 Gave {healthValue} HP and {scoreValue} points!");
            }
        }

        if (fishPile != null)
        {
            StartCoroutine(AddToPileAfterDelay(0.2f));
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    System.Collections.IEnumerator AddToPileAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (fishPile != null)
        {
            fishPile.AddFishToPile(gameObject, healthValue, scoreValue);
        }
    }
}