using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public enum GnomeType { Garden, Berserker, Brute }

    [Header("Gnome Settings")]
    public GnomeType type;
    public int damage;
    public int health;
    public string gnomeName;

    [Header("Combat Settings")]
    public float attackSpeed = 1.0f;
    private float nextAttackTime = 0f;

    [Header("Movement Settings")]
    public float movementSpeed;
    private Transform playerTransform;

    // --- ADDED FOR WAVE SYSTEM ---
    [Header("Wave System Connection")]
    public EndlessWaveManager manager;
    // ------------------------------

    void Start()
    {
        // Search for the object tagged "Player"
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Gnome cannot find Ragnar! Is he tagged as 'Player'?");
        }

        // Find the player by tag
       

        switch (type)
        {
            case GnomeType.Garden:
                gnomeName = "C_Common_Gnome";
                health = 100;
                damage = 10;
                movementSpeed = 0.5f;
                break;
            case GnomeType.Berserker:
                gnomeName = "C_Berserker_Gnome";
                health = 50;
                damage = 5;
                movementSpeed = 1.5f;
                break;
            case GnomeType.Brute:
                gnomeName = "C_Brute_Gnome";
                health = 150;
                damage = 20;
                movementSpeed = 0.3f;
                break;
        }
    }

    // --- ADDED MOVEMENT LOGIC ---
    void Update()
    {

        if (playerTransform == null)
        {
            Debug.LogWarning("Gnome is lost! It can't find anything with the 'Player' tag.");
        }
        else
        {
            Debug.Log("Gnome is chasing " + playerTransform.name);
        }

        if (playerTransform != null)
        {
            // 2. Calculate the direction to Ragnar
            Vector3 direction = (playerTransform.position - transform.position).normalized;

            // 3. Lock the Y axis so gnomes don't fly or sink into the floor
            direction.y = 0;

            // 4. Move the gnome forward
            transform.position += direction * movementSpeed * Time.deltaTime;

            // 5. Rotate the gnome to face Ragnar
            if (direction != Vector3.zero)
            {
                transform.forward = direction;
            }
        }

    }
    // ----------------------------

    // --- ADDED DAMAGE LOGIC ---
    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (manager != null)
        {
            manager.OnGnomeKilled(); // Tells the wave manager to count the death
        }
        Destroy(gameObject);
    }
    // ----------------------------

    private void OnCollisionStay(Collision collision)
    {
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();

        if (player != null)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack(player);
                nextAttackTime = Time.time + (1f / attackSpeed);
            }
        }
    }

    void Attack(PlayerScript player)
    {
        Debug.Log($"{gnomeName} hits you for {damage} damage!");
        player.TakeDamage(damage);
    }
}
