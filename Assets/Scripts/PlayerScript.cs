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
    private float moveInput; // Our movement variable

    [Header("Ducking Settings")]
    [SerializeField] public bool DuckMode = false;
    [SerializeField] public float DuckingSpeed = 2f; // float for smoother movement
    [SerializeField] public float duckHeightScale = 0.5f; // How short the player gets

    private Vector3 originalScale;
    private float currentSpeed;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Store the starting size of the player
        originalScale = transform.localScale;
        currentSpeed = playerSpeed;

    }

    // Update is called once per frame
    void Update()
    {
        // 3. DUCKING LOGIC
        HandleDuck();
        // 2. CHECK IF PLAYER IS ALIVE
        PlayerAlive();
        // 3. MOVEMENT LOGIC
        // Get horizontal movement (A = -1, D = 1)
        moveInput = Input.GetAxis("Horizontal");

        // Apply movement to the player's position
        transform.Translate(Vector3.right * moveInput * currentSpeed * Time.deltaTime);
        Debug.Log($"{playerName} is moving at speed {currentSpeed}.");

        // 4. DAMAGE LOGIC
        PlayerDamage();
    }

    void PlayerAlive()
    {         if (playerHealth <= 0)
        {
            isAlive = false;
            Debug.Log($"{playerName} has perished in battle.");
        }
    }
    void HandleDuck()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            DuckMode = true;
            // Reduce speed when ducking
            transform.localScale = new Vector3(originalScale.x, originalScale.y * duckHeightScale, originalScale.z);
            currentSpeed = DuckingSpeed;
            
        }
        else
        {
            DuckMode = false;
            //Return to normal size and speed
            currentSpeed = playerSpeed;
        }

    }
    void PlayerDamage()
    {           
        // Placeholder for damage logic

        if (isAlive == false)
        {
            return; // Exit if player is dead
        }
    }
    void PlayerInteract()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"{playerName} is interacting with an object.");
        }
        // Placeholder for interaction logic
    }
}
