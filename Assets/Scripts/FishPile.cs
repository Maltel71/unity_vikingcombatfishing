using UnityEngine;
using System.Collections.Generic;

public class FishPile : MonoBehaviour
{
    [Header("Pile Settings")]
    public bool keepFishInPile = true;
    public int maxFishInPile = 60;

    [Header("Random Placement")]
    public Collider2D pileAreaCollider;

    private List<GameObject> fishInPile = new List<GameObject>();

    void Start()
    {
        // Get collider if not assigned
        if (pileAreaCollider == null)
        {
            pileAreaCollider = GetComponent<Collider2D>();
        }

        if (pileAreaCollider == null)
        {
            Debug.LogWarning("FishPile: No collider assigned or found! Add a BoxCollider2D for pile area.");
        }
    }

    public void AddFishToPile(GameObject fish, int health, int score)
    {
        if (!keepFishInPile)
        {
            Destroy(fish);
            return;
        }

        // Add to list
        fishInPile.Add(fish);

        // Remove oldest fish if too many
        if (fishInPile.Count > maxFishInPile)
        {
            GameObject oldestFish = fishInPile[0];
            fishInPile.RemoveAt(0);
            if (oldestFish != null)
                Destroy(oldestFish);
        }

        // Position fish randomly in pile area
        ArrangeFish(fish);

        // Disable physics
        Rigidbody2D rb = fish.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Disable collider
        Collider2D col = fish.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        FlyingFish flyingFish = fish.GetComponent<FlyingFish>();
        if (flyingFish != null)
        {
            flyingFish.enabled = false;
        }

        Debug.Log($"Fish added to pile! Total: {fishInPile.Count}");
    }

    void ArrangeFish(GameObject fish)
    {
        Vector3 randomPosition;

        if (pileAreaCollider != null)
        {
            // Get random position within collider bounds
            Bounds bounds = pileAreaCollider.bounds;
            randomPosition = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                transform.position.z
            );
        }
        else
        {
            // Fallback if no collider
            randomPosition = transform.position + new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(-0.5f, 0.5f),
                0
            );
        }

        fish.transform.position = randomPosition;

        // Random rotation for natural look
        fish.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180f, 180f));
    }

    public void ClearPile()
    {
        foreach (GameObject fish in fishInPile)
        {
            if (fish != null)
                Destroy(fish);
        }
        fishInPile.Clear();
        Debug.Log("Fish pile cleared!");
    }

    void OnDrawGizmos()
    {
        if (pileAreaCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(pileAreaCollider.bounds.center, pileAreaCollider.bounds.size);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(1f, 1f, 0.1f));
        }
    }
}