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

    public float attackSpeed = 1.0f; // Attacks per second
    private float nextAttackTime = 0f; // Cooldown timer

    [Header("Movement Settings")]
    public float movementSpeed;
    private Transform playerTransform;
    void Start()
    {
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
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object hit has the PlayerScript attached
        PlayerScript player = collision.gameObject.GetComponent<PlayerScript>();

        if (player != null)
        {
            // Only damage if enough time has passed (Attack Speed Logic)
            if (Time.time >= nextAttackTime)
            {
                Attack(player);
                // Calculate next available attack time
                nextAttackTime = Time.time + (1f / attackSpeed);
                Debug.Log(gnomeName + " attacked for " + damage + " damage!");
                player.TakeDamage(damage);
            }
           
            void Attack(PlayerScript player)
            {
                Debug.Log($"{gnomeName} hits you for {damage} damage!");
                player.TakeDamage(damage);
            }

           
        }
    }
}