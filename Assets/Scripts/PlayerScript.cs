using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] public int playerHealth = 100;
    [SerializeField] public float playerSpeed = 5f;
    [SerializeField] public string playerName = "Ragnar";
    [SerializeField] public float AttackSpeed = 1.5f;
    [SerializeField] public float AttackPower = 100f; 
    [SerializeField] public bool isAlive = true;

    [Header("Movement Settings")]
    private float moveInput;

    [Header("Ducking Settings")]
    [SerializeField] public bool DuckMode = false;
    [SerializeField] public float DuckingSpeed = 2f;
    [SerializeField] public float duckHeightScale = 0.5f;

    [Header("Combat Settings")]
    private float nextAttackTime = 0f;

    private Vector3 originalScale;
    private float currentSpeed;

    void Start()
    {
        originalScale = transform.localScale;
        currentSpeed = playerSpeed;
    }

    void Update()
    {
        if (!isAlive) return; // Stop logic if Ragnar is dead

        HandleMovement();
        HandleDuck();
        HandleAttack();
        PlayerInteract();
    }

    void HandleMovement()
    {
        moveInput = Input.GetAxis("Horizontal");
        transform.Translate(Vector3.right * moveInput * currentSpeed * Time.deltaTime);

        // Only Log if moving to avoid console spam
        if (moveInput != 0)
            Debug.Log($"{playerName} is moving.");
    }

    void HandleDuck()
    {
        // Use Input.GetKey (not Down) so it stays ducked while holding the key
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftControl))
        {
            DuckMode = true;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * duckHeightScale, originalScale.z);
            currentSpeed = DuckingSpeed;
        }
        else
        {
            DuckMode = false;
            transform.localScale = originalScale;
            currentSpeed = playerSpeed;
        }
    }

    void HandleAttack()
    {
        // Space to attack
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextAttackTime)
        {
            Debug.Log($"{playerName} swings his weapon with the Spacebar!");

            // Apply Attack Speed cooldown
            nextAttackTime = Time.time + (1f / AttackSpeed);

            // Logic to check if an enemy is hit
            CheckForEnemyHit();
        }
    }

    void CheckForEnemyHit()
    {
        // Raycast to hit Gnomes in front of you
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
        {
            if (hit.collider.CompareTag("Gnome"))
            {
                // If the Gnome has a script, you can damage it here
                Debug.Log("Hit a Gnome!");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isAlive || damage <= 0) return;

        playerHealth -= damage;
        Debug.Log($"{playerName} took {damage} damage! HP: {playerHealth}");

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
    }

    void Die()
    {
        isAlive = false;
        Debug.Log($"{playerName} has perished in battle.");
        // Disable movement or trigger animation here
    }

    void PlayerInteract()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(playerName + " is attempting to interact...");
        }
    }
}

