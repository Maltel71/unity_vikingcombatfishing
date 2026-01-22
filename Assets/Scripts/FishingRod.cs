using UnityEngine;
using System.Collections;

public class FishingRod : MonoBehaviour
{
    [Header("Fisk Prefabs")]
    public GameObject aborrePrefab;
    public GameObject gaddaPrefab;

    [Header("Napp Settings")]
    public float minBiteTime = 10f;
    public float maxBiteTime = 30f;

    [HideInInspector] public bool hasBite = false;
    [HideInInspector] public bool isWaitingForBite = false;
    [HideInInspector] public bool isReelingIn = false;
    [HideInInspector] public float reelInProgress = 0f;

    [Header("Reel In Settings")]
    public float reelInDuration = 3f;

    [Header("Kast Settings")]
    public float upwardForce = 20f;
    public float horizontalForce = 15f;
    public Transform waterSpawnPoint;

    [Header("References")]
    public FishingZone fishingZone;
    public Transform playerTransform;
    public FishPile fishPile;

    [Header("Sound Effects 🔊")]
    public AudioClip biteSound;
    public AudioClip castSound;
    public AudioClip catchSound;
    private AudioSource audioSource;

    [Header("Animation (Optional)")]
    public Animator playerAnimator;
    public string fishingAnimTrigger = "StartFishing";
    public string catchAnimTrigger = "CatchFish";

    private Coroutine biteCoroutine;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform.parent;
        }

        if (fishPile == null)
        {
            fishPile = FindObjectOfType<FishPile>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (fishingZone == null || !fishingZone.playerInZone)
        {
            if (biteCoroutine != null || isReelingIn)
            {
                CancelFishing();
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!hasBite && biteCoroutine == null && !isReelingIn)
            {
                biteCoroutine = StartCoroutine(WaitForBite());
                PlaySound(castSound);

                if (playerAnimator != null && !string.IsNullOrEmpty(fishingAnimTrigger))
                {
                    playerAnimator.SetTrigger(fishingAnimTrigger);
                }
            }
            else if (hasBite && !isReelingIn)
            {
                StartReelingIn();
            }
        }

        if (Input.GetKey(KeyCode.E) && isReelingIn)
        {
            reelInProgress += Time.deltaTime;

            if (reelInProgress >= reelInDuration)
            {
                CatchFish();
            }
        }

        if (Input.GetKeyUp(KeyCode.E) && isReelingIn)
        {
            LostFish();
        }
    }

    IEnumerator WaitForBite()
    {
        isWaitingForBite = true;
        float waitTime = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(waitTime);

        hasBite = true;
        isWaitingForBite = false;
        PlaySound(biteSound);

        biteCoroutine = null;
    }

    void StartReelingIn()
    {
        isReelingIn = true;
        reelInProgress = 0f;
    }

    void CatchFish()
    {
        PlaySound(catchSound);

        if (playerAnimator != null && !string.IsNullOrEmpty(catchAnimTrigger))
        {
            playerAnimator.SetTrigger(catchAnimTrigger);
        }

        bool isPerch = Random.value < 0.6f;
        GameObject fishPrefab = isPerch ? aborrePrefab : gaddaPrefab;

        if (fishPrefab == null)
        {
            ResetFishing();
            return;
        }

        Vector3 spawnPos;
        if (waterSpawnPoint != null)
        {
            spawnPos = waterSpawnPoint.position;
        }
        else if (fishingZone != null)
        {
            spawnPos = fishingZone.transform.position;
        }
        else
        {
            spawnPos = playerTransform.position;
        }

        GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rb = fish.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = fish.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
        }

        // Hitta målposition
        Vector3 targetPos;
        if (fishPile != null)
        {
            targetPos = fishPile.transform.position;
        }
        else
        {
            targetPos = playerTransform.position + Vector3.right * 3f;
        }

        // Beräkna horizontal riktning
        float horizontalDirection = targetPos.x - spawnPos.x;

        // VIKTIGT: Om FishPile är direkt över vattnet (nästan 0), tvinga åt höger!
        if (Mathf.Abs(horizontalDirection) < 0.5f)
        {
            Debug.LogWarning("⚠️ FishPile är nästan direkt över vattnet! Tvingar horizontal kraft åt höger.");
            horizontalDirection = 1f; // Tvinga åt höger
        }
        else
        {
            // Normalisera riktningen
            horizontalDirection = Mathf.Sign(horizontalDirection);
        }

        // Skapa force vektor
        Vector2 force = new Vector2(
            horizontalDirection * horizontalForce,  // Åt sidan (garanterat != 0)
            upwardForce                              // Uppåt
        );

        rb.AddForce(force, ForceMode2D.Impulse);

        Debug.Log($"🚀 KASTA FISK:");
        Debug.Log($"   Spawn: {spawnPos}");
        Debug.Log($"   Target: {targetPos}");
        Debug.Log($"   Horizontal riktning: {horizontalDirection}");
        Debug.Log($"   Force: X={force.x}, Y={force.y}");

        FlyingFish flyingFish = fish.GetComponent<FlyingFish>();
        if (flyingFish != null && fishPile != null)
        {
            flyingFish.fishPile = fishPile;
        }

        ResetFishing();
    }

    void LostFish()
    {
        ResetFishing();
    }

    void CancelFishing()
    {
        if (biteCoroutine != null)
        {
            StopCoroutine(biteCoroutine);
            biteCoroutine = null;
        }

        ResetFishing();
    }

    void ResetFishing()
    {
        hasBite = false;
        isReelingIn = false;
        isWaitingForBite = false;
        reelInProgress = 0f;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void OnDrawGizmos()
    {
        if (fishPile != null && waterSpawnPoint != null)
        {
            // Rita linje från vatten till pile
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(waterSpawnPoint.position, fishPile.transform.position);

            // Rita pile position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(fishPile.transform.position, 0.5f);

            // Skriv position info
            Gizmos.color = Color.white;
        }
    }
}

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class FishingRod : MonoBehaviour
//{
//    public List<GameObject> fishPrefabs; // Assign different fish prefabs here
//    public int totalScore = 0;

//    private bool isFishing = false;
//    private float waitTime;

//    void Update()
//    {
//        // Press F to start/stop fishing
//        if (Input.GetKeyDown(KeyCode.F))
//        {
//            if (!isFishing) StartCoroutine(StartFishing());
//            else StopFishing();
//        }
//    }

//    IEnumerator StartFishing()
//    {
//        isFishing = true;
//        Debug.Log("Casting line... waiting for a bite.");

//        // Random wait time between 2 and 5 seconds for a bite
//        waitTime = Random.Range(2f, 5f);
//        yield return new WaitForSeconds(waitTime);

//        CatchFish();
//    }

//    void CatchFish()
//    {
//        if (!isFishing) return;

//        // Randomly select a fish from your list
//        int randomIndex = Random.Range(0, fishPrefabs.Count);
//        GameObject caughtPrefab = fishPrefabs[randomIndex];
//        Fish fishData = caughtPrefab.GetComponent<Fish>();

//        totalScore += fishData.scoreValue;
//        Debug.Log($"Caught a {fishData.fishName}! +{fishData.scoreValue} points. Total Score: {totalScore}");

//        StopFishing();
//    }

//    void StopFishing()
//    {
//        isFishing = false;
//        Debug.Log("Line reeled in.");
//    }
//}
