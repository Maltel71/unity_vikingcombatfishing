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

    // PUBLIC states för UI att läsa
    [HideInInspector] public bool hasBite = false;
    [HideInInspector] public bool isWaitingForBite = false;
    [HideInInspector] public bool isReelingIn = false;
    [HideInInspector] public float reelInProgress = 0f;

    [Header("Reel In Settings")]
    public float reelInDuration = 3f;

    [Header("Kast Settings")]
    public float kastKraft = 10f;
    public float kastHojd = 3f;
    public Transform waterSpawnPoint;

    [Header("References")]
    public FishingZone fishingZone;
    public Transform playerTransform;
    public FishPile fishPile;  // NYTT: Reference till fisk-högen

    [Header("Animation (Optional)")]
    public Animator playerAnimator;
    public string fishingAnimTrigger = "StartFishing";
    public string catchAnimTrigger = "CatchFish";

    [Header("Debug")]
    public bool showDebugMessages = true;

    private Coroutine biteCoroutine;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform.parent;
        }

        // Hitta FishPile automatiskt om inte satt
        if (fishPile == null)
        {
            fishPile = FindObjectOfType<FishPile>();
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

        // TRYCK E
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!hasBite && biteCoroutine == null && !isReelingIn)
            {
                biteCoroutine = StartCoroutine(WaitForBite());

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

        // HÅLL E
        if (Input.GetKey(KeyCode.E) && isReelingIn)
        {
            reelInProgress += Time.deltaTime;

            if (reelInProgress >= reelInDuration)
            {
                CatchFish();
            }
        }

        // SLÄPP E
        if (Input.GetKeyUp(KeyCode.E) && isReelingIn)
        {
            LostFish();
        }
    }

    IEnumerator WaitForBite()
    {
        isWaitingForBite = true;

        if (showDebugMessages)
            Debug.Log("🎣 Kastar ut linan... väntar på napp.");

        float waitTime = Random.Range(minBiteTime, maxBiteTime);
        yield return new WaitForSeconds(waitTime);

        hasBite = true;
        isWaitingForBite = false;

        if (showDebugMessages)
            Debug.Log("⚡ NAPP! Håll E för att dra upp fisken!");

        biteCoroutine = null;
    }

    void StartReelingIn()
    {
        isReelingIn = true;
        reelInProgress = 0f;

        if (showDebugMessages)
            Debug.Log("Börjar dra upp fisken! HÅLL KVAR E!");
    }

    void CatchFish()
    {
        if (showDebugMessages)
            Debug.Log("✅ FÅNGAD! Fisken flyger upp från vattnet!");

        if (playerAnimator != null && !string.IsNullOrEmpty(catchAnimTrigger))
        {
            playerAnimator.SetTrigger(catchAnimTrigger);
        }

        bool isPerch = Random.value < 0.6f;
        GameObject fishPrefab = isPerch ? aborrePrefab : gaddaPrefab;

        if (fishPrefab == null)
        {
            Debug.LogError("Fisk prefab saknas!");
            ResetFishing();
            return;
        }

        // Spawn position (i vattnet)
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

        // Kasta mot spelaren
        Vector2 direction = (playerTransform.position - spawnPos).normalized;
        Vector2 force = new Vector2(direction.x * kastKraft, kastHojd);
        rb.AddForce(force, ForceMode2D.Impulse);

        // NYTT: Sätt fish pile reference på fisken
        FlyingFish flyingFish = fish.GetComponent<FlyingFish>();
        if (flyingFish != null && fishPile != null)
        {
            flyingFish.fishPile = fishPile;
        }

        if (showDebugMessages)
            Debug.Log($"Kastade en {(isPerch ? "Abborre" : "Gädda")} från vattnet!");

        ResetFishing();
    }

    void LostFish()
    {
        if (showDebugMessages)
            Debug.Log("❌ SLAPP! Du tappade fisken...");

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
