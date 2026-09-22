using UnityEngine;

[RequireComponent(typeof(EnemyScript))]
public class RusherGnome : MonoBehaviour
{
    [Header("Charge")]
    [Tooltip("World units per second. The whole point is that it is far faster than a normal gnome.")]
    public float chargeSpeed = 6f;
    [Tooltip("How close it has to get before it connects.")]
    public float contactRange = 0.9f;
    [Tooltip("Seconds before it gives up and drops dead on its own.")]
    public float lifetime = 15f;

    [Header("Impact")]
    [Tooltip("Spend itself on the hit. One charge, one chance.")]
    public bool dieOnImpact = true;
    public AudioClip impactSound;
    [Range(0f, 1f)]
    public float impactVolume = 1f;

    [Header("Look")]
    [Tooltip("Flip the sprite to face the way it is running.")]
    public bool faceTravelDirection = true;

    private EnemyScript enemy;
    private Transform player;
    private PlayerScript playerScript;
    private AudioSource audioSource;
    private bool spent = false;
    private float bornAt;
    private float baseScaleX;

    void Awake()
    {
        enemy = GetComponent<EnemyScript>();
        enemy.externalBehaviour = true;

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Start()
    {
        bornAt = Time.time;

        GameObject found = GameObject.FindGameObjectWithTag("Player");
        if (found != null)
        {
            player = found.transform;
            playerScript = found.GetComponent<PlayerScript>();
        }

        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (spent) return;
        if (PlayerScript.IsGameOver) return;
        if (enemy == null || enemy.health <= 0) return;
        if (player == null) return;

        if (Time.time >= bornAt + lifetime)
        {
            Expire();
            return;
        }

        float gap = player.position.x - transform.position.x;

        if (Mathf.Abs(gap) <= contactRange)
        {
            Impact();
            return;
        }

        float step = Mathf.Sign(gap) * chargeSpeed * Time.deltaTime;
        transform.position += new Vector3(step, 0f, 0f);

        if (faceTravelDirection)
        {
            Vector3 scale = transform.localScale;
            scale.x = gap >= 0f ? baseScaleX : -baseScaleX;
            transform.localScale = scale;
        }
    }

    void Impact()
    {
        spent = true;

        if (playerScript != null)
        {
            playerScript.TakeDamage(enemy.damage);
        }

        if (impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactSound, impactVolume);
        }

        if (dieOnImpact)
        {
            enemy.TakeDamage(enemy.health);
        }
    }

    void Expire()
    {
        spent = true;
        enemy.TakeDamage(enemy.health);
    }
}
