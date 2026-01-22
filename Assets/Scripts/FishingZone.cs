using UnityEngine;

public class FishingZone : MonoBehaviour
{
    [Header("Zone Settings")]
    public bool playerInZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            Debug.Log("Spelaren är vid vattnet! Tryck E för att fiska.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            Debug.Log("Spelaren lämnade fiskezonen.");
        }
    }
}