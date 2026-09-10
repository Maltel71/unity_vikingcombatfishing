using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishPile : MonoBehaviour
{
    [Header("Pile Settings")]
    public bool keepFishInPile = true;
    public int maxFishInPile = 60;

    [Header("Random Placement")]
    public Collider2D pileAreaCollider;

    [Header("Throw To Pile")]
    [Tooltip("Throw the fish in an arc to the pile instead of teleporting it there.")]
    public bool throwFishToPile = true;
    [Tooltip("Flight time. A pile far away can take a higher value.")]
    public float throwDuration = 0.7f;
    [Tooltip("Arc height at the midpoint, in world units.")]
    public float throwArcHeight = 3.5f;
    [Tooltip("Spin during flight, degrees per second. Randomised in both directions.")]
    public float throwSpin = 720f;
    [Tooltip("Scale on landing. Below 1 sells the feeling of flying off into the distance.")]
    public float throwEndScale = 1f;

    [Header("Cast Sounds")]
    public AudioClip throwSound;
    [Range(0f, 1f)]
    public float throwVolume = 0.6f;
    public AudioClip landSound;
    [Range(0f, 1f)]
    public float landVolume = 0.5f;

    private List<GameObject> fishInPile = new List<GameObject>();
    private AudioSource audioSource;

    void Start()
    {

        if (pileAreaCollider == null)
        {
            pileAreaCollider = GetComponent<Collider2D>();
        }

        if (pileAreaCollider == null)
        {
            Debug.LogWarning("FishPile: No collider assigned or found! Add a BoxCollider2D for pile area.");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void AddFishToPile(GameObject fish, int health, int score)
    {
        if (fish == null) return;

        if (!keepFishInPile)
        {
            Destroy(fish);
            return;
        }

        fishInPile.Add(fish);

        if (fishInPile.Count > maxFishInPile)
        {
            GameObject oldestFish = fishInPile[0];
            fishInPile.RemoveAt(0);
            if (oldestFish != null)
                Destroy(oldestFish);
        }

        DisableFishPhysics(fish);

        Vector3 target = GetPilePosition();

        if (throwFishToPile && throwDuration > 0f && isActiveAndEnabled)
        {
            StartCoroutine(ThrowToPile(fish, target));
        }
        else
        {
            PlaceFish(fish, target);
        }
    }

    void DisableFishPhysics(GameObject fish)
    {
        Rigidbody2D rb = fish.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

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
    }

    IEnumerator ThrowToPile(GameObject fish, Vector3 target)
    {
        if (fish == null) yield break;

        Vector3 start = fish.transform.position;
        Vector3 startScale = fish.transform.localScale;
        Vector3 endScale = startScale * Mathf.Max(0.05f, throwEndScale);

        float spin = throwSpin * (Random.value < 0.5f ? -1f : 1f);
        float startAngle = fish.transform.eulerAngles.z;

        PlaySound(throwSound, throwVolume);

        float elapsed = 0f;
        while (elapsed < throwDuration)
        {

            if (fish == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / throwDuration);

            Vector3 flat = Vector3.Lerp(start, target, t);
            flat.y += Mathf.Sin(t * Mathf.PI) * throwArcHeight;

            fish.transform.position = flat;
            fish.transform.rotation = Quaternion.Euler(0f, 0f, startAngle + spin * elapsed);
            fish.transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        if (fish == null) yield break;

        fish.transform.localScale = endScale;
        PlaceFish(fish, target);
        PlaySound(landSound, landVolume);
    }

    void PlaceFish(GameObject fish, Vector3 position)
    {
        if (fish == null) return;

        fish.transform.position = position;

        fish.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-180f, 180f));
    }

    Vector3 GetPilePosition()
    {
        if (pileAreaCollider != null)
        {
            Bounds bounds = pileAreaCollider.bounds;
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                transform.position.z
            );
        }

        return transform.position + new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            0f
        );
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void ClearPile()
    {
        foreach (GameObject fish in fishInPile)
        {
            if (fish != null)
                Destroy(fish);
        }
        fishInPile.Clear();
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
