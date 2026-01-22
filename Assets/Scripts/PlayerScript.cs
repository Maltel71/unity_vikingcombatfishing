using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] public int playerHealth = 100;
    [SerializeField] public int maxHealth = 100;  // NYTT: Max HP
    [SerializeField] public int playerScore = 0;   // NYTT: Poäng
    [SerializeField] public float playerSpeed = 5f;
    [SerializeField] public string playerName = "Ragnar";
    [SerializeField] public float AttackSpeed = 0.1f;
    [SerializeField] public float AttackPower = 100f;
    [SerializeField] public bool isAlive = true;

    [Header("Sound Effects")]
    public AudioClip[] hurtSounds;
    private AudioSource audioSource;

    [Header("Movement Settings")]
    private float moveInput;
    private bool facingRight = true;

    [Header("Ducking Settings")]
    [SerializeField] public bool DuckMode = false;
    [SerializeField] public float DuckingSpeed = 2f;
    [SerializeField] public float duckHeightScale = 0.5f;

    [Header("2D Combat Settings")]
    public float attackRange = 1.2f;
    public float attackOffset = 1.0f;
    private float nextAttackTime = 0f;

    private Vector3 originalScale;
    private float currentSpeed;

    void Start()
    {
        originalScale = transform.localScale;
        currentSpeed = playerSpeed;

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
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
        // Check if fishing rod exists and is reeling - disable movement
        FishingRod fishingRod = GetComponentInChildren<FishingRod>();
        if (fishingRod != null && fishingRod.isReelingIn)
        {
            return; // Skip movement entirely while reeling
        }

        moveInput = Input.GetAxis("Horizontal");

        //Apply Movement
        transform.Translate(Vector3.right * moveInput * currentSpeed * Time.deltaTime, Space.World);

        //Flip Logic 
        if (moveInput > 0 && !facingRight)
        {
            FlipCharacter();
            Debug.Log("Flipped Right");
        }
        else if (moveInput < 0 && facingRight)
        {
            FlipCharacter();
            Debug.Log("Flipped Left");
        }

        // Only Log if moving to avoid console spam
        if (moveInput != 0)
            Debug.Log($"{playerName} is moving.");
    }


    void HandleDuck()
    {
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftControl))
        {
            DuckMode = true;
            // HIGHLIGHTED CHANGE: Keep the current X scale when ducking
            transform.localScale = new Vector3(transform.localScale.x, originalScale.y * duckHeightScale, originalScale.z);
            currentSpeed = DuckingSpeed;
            Debug.Log($"{playerName} is ducking.");
        }
        else
        {
            DuckMode = false;
            // HIGHLIGHTED CHANGE: Keep the current X scale when standing back up
            transform.localScale = new Vector3(transform.localScale.x, originalScale.y, originalScale.z);
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
        float faceDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackPoint = (Vector2)transform.position + new Vector2(faceDir * attackOffset, 0);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRange);

        foreach (Collider2D hit in hitEnemies)
        {
            if (hit.CompareTag("Enemy"))
            {
                EnemyScript enemy = hit.GetComponent<EnemyScript>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)AttackPower);
                    Debug.Log("Hit an enemy for " + AttackPower + " damage!");
                    break; // Only hit ONE enemy per attack
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float faceDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackPoint = (Vector2)transform.position + new Vector2(faceDir * attackOffset, 0);
        Gizmos.DrawWireSphere(attackPoint, attackRange);
    }


    public void TakeDamage(int damage)
    {
        if (!isAlive || damage <= 0) return;

        playerHealth -= damage;
        Debug.Log($"{playerName} took {damage} damage! HP: {playerHealth}");

        // Play random hurt sound
        if (hurtSounds.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, hurtSounds.Length);
            audioSource.PlayOneShot(hurtSounds[randomIndex]);
        }

        if (playerHealth <= 0)
        {
            playerHealth = 0;
            Die();
        }
    }

    // NYTT: Samla fisk och få HP + poäng
    public void CollectFish(int health, int score)
    {
        if (!isAlive) return;

        // Lägg till HP (max upp till maxHealth)
        playerHealth = Mathf.Min(playerHealth + health, maxHealth);

        // Lägg till poäng
        playerScore += score;

        Debug.Log($"{playerName} fick +{health} HP och +{score} poäng! Total HP: {playerHealth}/{maxHealth}, Score: {playerScore}");
    }

    void Die()
    {
        isAlive = false;
        Debug.Log($"{playerName} has perished in battle.");
        // Don't destroy - let death animation play
    }

    void PlayerInteract()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(playerName + " is attempting to interact...");
        }
    }
    void FlipCharacter()
    {
        facingRight = !facingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }

}

//using UnityEngine;
//using System.Collections;

//public class PlayerScript : MonoBehaviour
//{
//    [SerializeField] public int playerHealth = 100;
//    [SerializeField] public float playerSpeed = 5f;
//    [SerializeField] public string playerName = "Ragnar";
//    [SerializeField] public float AttackSpeed = 1.5f;
//    [SerializeField] public float AttackPower = 100f; 
//    [SerializeField] public bool isAlive = true;

//    [Header("Movement Settings")]
//    private float moveInput;
//    private bool facingRight = true; // Tracks which way Ragnar is facing

//    [Header("Ducking Settings")]
//    [SerializeField] public bool DuckMode = false;
//    [SerializeField] public float DuckingSpeed = 2f;
//    [SerializeField] public float duckHeightScale = 0.5f;

//    [Header("Combat Settings")]
//    private float nextAttackTime = 0f;

//    private Vector3 originalScale;
//    private float currentSpeed;

//    void Start()
//    {
//        originalScale = transform.localScale;
//        currentSpeed = playerSpeed;
//    }

//    void Update()
//    {
//        if (!isAlive) return; // Stop logic if Ragnar is dead

//        HandleMovement();
//        HandleDuck();
//        HandleAttack();
//        PlayerInteract();

//    }

//    void HandleMovement()
//    {
//        moveInput = Input.GetAxis("Horizontal");

//        //Apply Movement
//        transform.Translate(Vector3.right * moveInput * currentSpeed * Time.deltaTime, Space.World);

//        //Flip Logic 
//        if (moveInput > 0 && !facingRight) 
//    {
//        FlipCharacter(); // If moving right but looking left, flip!
//            Debug.Log("Flipped Right");
//        }
//    else if (moveInput < 0 && facingRight) 
//    {
//        FlipCharacter(); // If moving left but looking right, flip!
//            Debug.Log("Flipped Left");
//        }


//        // Only Log if moving to avoid console spam
//        if (moveInput != 0)
//            Debug.Log($"{playerName} is moving.");
//    }


//    void HandleDuck()
//    {
//        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.LeftControl))
//        {
//            DuckMode = true;
//            // HIGHLIGHTED CHANGE: Keep the current X scale when ducking
//            transform.localScale = new Vector3(transform.localScale.x, originalScale.y * duckHeightScale, originalScale.z);
//            currentSpeed = DuckingSpeed;
//            Debug.Log($"{playerName} is ducking.");
//        }
//        else
//        {
//            DuckMode = false;
//            // HIGHLIGHTED CHANGE: Keep the current X scale when standing back up
//            transform.localScale = new Vector3(transform.localScale.x, originalScale.y, originalScale.z);
//            currentSpeed = playerSpeed;
//        }
//    }

//    void HandleAttack()
//    {
//        // Space to attack
//        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextAttackTime)
//        {
//            Debug.Log($"{playerName} swings his weapon with the Spacebar!");

//            // Apply Attack Speed cooldown
//            nextAttackTime = Time.time + (1f / AttackSpeed);

//            // Logic to check if an enemy is hit
//            CheckForEnemyHit();
//        }
//    }

//    void CheckForEnemyHit()
//    {
//        // Raycast to hit Gnomes in front of you
//        RaycastHit hit;
//        if (Physics.Raycast(transform.position, transform.forward, out hit, 2f))
//        {
//            if (hit.collider.CompareTag("Gnome"))
//            {
//                // If the Gnome has a script, you can damage it here
//                Debug.Log("Hit a Gnome!");
//            }
//        }
//    }

//    public void TakeDamage(int damage)
//    {
//        if (!isAlive || damage <= 0) return;

//        playerHealth -= damage;
//        Debug.Log($"{playerName} took {damage} damage! HP: {playerHealth}");

//        if (playerHealth <= 0)
//        {
//            playerHealth = 0;
//            Die();

//        }
//    }

//    void Die()
//    {
//        isAlive = false;
//        Debug.Log($"{playerName} has perished in battle.");
//        // Disable movement or trigger animation here
//        Destroy(gameObject, 1f);
//    }

//    void PlayerInteract()
//    {
//        if (Input.GetKeyDown(KeyCode.E))
//        {
//            Debug.Log(playerName + " is attempting to interact...");
//        }
//    }
//    void FlipCharacter()
//    {
//        facingRight = !facingRight;
//        Vector3 currentScale = transform.localScale;
//        currentScale.x *= -1;
//        transform.localScale = currentScale;
//    }

//}

