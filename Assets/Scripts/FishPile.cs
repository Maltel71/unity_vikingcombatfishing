using UnityEngine;
using System.Collections.Generic;

public class FishPile : MonoBehaviour
{
    [Header("Pile Settings")]
    public bool keepFishInPile = true;  // Toggle: ska fiskar ligga kvar?
    public float pileSpacing = 0.3f;    // Avst�nd mellan fiskar
    public int maxFishInPile = 20;      // Max antal fiskar innan gamla tas bort

    [Header("Visual")]
    public bool stackVertically = false; // Stapla upp�t eller bredvid?
    public float stackHeight = 0.2f;     // H�jd mellan lager om vertical

    private List<GameObject> fishInPile = new List<GameObject>();

    public void AddFishToPile(GameObject fish, int health, int score)
    {
        if (!keepFishInPile)
        {
            // Om toggle �r av, f�rst�r fisken direkt
            Destroy(fish);
            return;
        }

        // L�gg till i listan
        fishInPile.Add(fish);

        // Ta bort �ldsta fisken om vi har f�r m�nga
        if (fishInPile.Count > maxFishInPile)
        {
            GameObject oldestFish = fishInPile[0];
            fishInPile.RemoveAt(0);
            if (oldestFish != null)
                Destroy(oldestFish);
        }

        // Positionera fisken i h�gen
        ArrangeFish(fish);

        // Inaktivera physics p� fisken s� den stannar kvar
        Rigidbody2D rb = fish.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Inaktivera collider s� spelaren inte snubblar p� fisken
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

        Debug.Log($"Fisk tillagd i h�gen! Total: {fishInPile.Count}");
    }

    void ArrangeFish(GameObject fish)
    {
        int index = fishInPile.Count - 1;

        if (stackVertically)
        {
            // Stapla upp�t
            fish.transform.position = transform.position + new Vector3(0, index * stackHeight, 0);
        }
        else
        {
            // L�gg bredvid varandra (som en rad)
            fish.transform.position = transform.position + new Vector3(index * pileSpacing, 0, 0);
        }

        // Rotera lite slumpm�ssigt f�r naturlig look (valfritt)
        fish.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
    }

    // Rensa hela h�gen (t.ex. n�r spelaren d�r eller niv� b�rjar om)
    public void ClearPile()
    {
        foreach (GameObject fish in fishInPile)
        {
            if (fish != null)
                Destroy(fish);
        }
        fishInPile.Clear();
        Debug.Log("Fisk-h�gen rensad!");
    }

    // F�r att visualisera var h�gen �r i editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
    }
}