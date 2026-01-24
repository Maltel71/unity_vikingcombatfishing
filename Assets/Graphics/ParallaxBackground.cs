using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float speedMultiplier = 0.5f;

    [Header("Optional: Limit Parallax to Horizontal Only")]
    public bool horizontalOnly = true;

    private Transform playerTransform;
    private Vector3 lastPlayerPosition;

    void Start()
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            lastPlayerPosition = playerTransform.position;
        }
        else
        {
            Debug.LogError("ParallaxBackground: No GameObject with 'Player' tag found!");
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // Get current player position
        Vector3 playerPosition = playerTransform.position;

        // Calculate how much the player moved
        Vector3 deltaMovement = playerPosition - lastPlayerPosition;

        // If horizontal only, ignore Y movement
        if (horizontalOnly)
        {
            deltaMovement.y = 0;
        }

        // Move background in the OPPOSITE direction of player movement
        // Multiplied by speedMultiplier for parallax effect
        transform.position -= deltaMovement * speedMultiplier;

        // Update last position for next frame
        lastPlayerPosition = playerPosition;
    }
}