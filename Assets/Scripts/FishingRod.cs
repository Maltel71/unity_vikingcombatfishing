using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FishType
{
    public GameObject fishPrefab;
    [Range(0f, 100f)]
    public float weight = 10f;
}

public class FishingRod : MonoBehaviour
{
    [Header("Fish Types")]
    public FishType[] fishTypes;

    [Header("Bite Settings")]
    public float minBiteTime = 10f;
    public float maxBiteTime = 30f;

    [Header("Visual Effects")]
    public ParticleSystem waterSplashEffect;

    [Header("Catch Text")]
    [Tooltip("Show a label with the fish name when it comes out of the water.")]
    public bool showCatchPopup = true;

    [HideInInspector] public bool hasBite = false;
    [HideInInspector] public bool isWaitingForBite = false;
    [HideInInspector] public bool isReelingIn = false;
    [HideInInspector] public float reelInProgress = 0f;

    public float CurrentReelDuration
    {
        get { return currentReelDuration > 0f ? currentReelDuration : reelInDuration; }
    }

    [Header("Reel In Settings")]
    public float reelInDuration = 3f;
    public Transform reelingPosition;

    [Header("Cast Settings")]
    public float minUpwardForce = 15f;
    public float maxUpwardForce = 25f;
    public float minHorizontalForce = 10f;
    public float maxHorizontalForce = 20f;
    public Transform waterSpawnPoint;

    [Header("References")]
    public FishingZone fishingZone;
    public Transform playerTransform;
    public FishPile fishPile;

    [Header("Sound Effects")]
    public AudioClip biteSound;
    public AudioClip castSound;
    public AudioClip catchSound;
    private AudioSource audioSource;

    [Header("Reeling Sounds")]
    public AudioClip[] reelingGruntSounds;
    [Range(0f, 1f)]
    public float gruntVolume = 1f;
    private int currentGruntIndex = 0;

    public AudioClip[] reelingSFX;
    [Range(0f, 1f)]
    public float reelingSFXStartVolume = 0.3f;
    [Range(0f, 1f)]
    public float reelingSFXMaxVolume = 1f;
    private int currentReelingSFXIndex = 0;
    private AudioSource reelingSFXSource;

    [Header("Fade Out Settings")]
    [Range(0.1f, 2f)]
    public float soundFadeOutDuration = 0.5f;

    [Header("Animation (Optional)")]
    public Animator playerAnimator;
    public string fishingAnimTrigger = "StartFishing";
    public string catchAnimTrigger = "CatchFish";

    [Header("Pickup Settings")]
    public Transform pickupRangeObject;

    private Coroutine biteCoroutine;
    private GameObject pendingFish;
    private float currentReelDuration;
    private Coroutine fadeOutCoroutine;

    void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform.parent;
        }

        if (fishPile == null)
        {
            fishPile = FindFirstObjectByType<FishPile>();
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        reelingSFXSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (PauseMenu.IsPaused) return;

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

        if (isReelingIn && Input.GetKey(KeyCode.E))
        {
            reelInProgress += Time.deltaTime;
        }

        if (isReelingIn && reelInProgress >= CurrentReelDuration)
        {
            CatchFish();
        }

        if (Input.GetKeyUp(KeyCode.E) && isReelingIn)
        {
            LostFish();
        }

        if (isReelingIn && !audioSource.isPlaying && reelingGruntSounds.Length > 0)
        {
            audioSource.PlayOneShot(reelingGruntSounds[currentGruntIndex], gruntVolume);
            currentGruntIndex = (currentGruntIndex + 1) % reelingGruntSounds.Length;
        }

        if (isReelingIn && !reelingSFXSource.isPlaying && reelingSFX.Length > 0)
        {

            float progress = reelInProgress / CurrentReelDuration;
            float currentVolume = Mathf.Lerp(reelingSFXStartVolume, reelingSFXMaxVolume, progress);

            reelingSFXSource.PlayOneShot(reelingSFX[currentReelingSFXIndex], currentVolume);
            currentReelingSFXIndex = (currentReelingSFXIndex + 1) % reelingSFX.Length;
        }
    }

    IEnumerator WaitForBite()
    {
        isWaitingForBite = true;

        pendingFish = GetRandomFish();

        float biteMultiplier = 1f;
        currentReelDuration = reelInDuration;

        if (pendingFish != null)
        {
            FlyingFish info = pendingFish.GetComponent<FlyingFish>();
            if (info != null)
            {
                biteMultiplier = Mathf.Max(0.05f, info.biteTimeMultiplier);
                currentReelDuration = reelInDuration * Mathf.Max(0.05f, info.reelDifficulty);
            }
        }

        float waitTime = Random.Range(minBiteTime, maxBiteTime) * biteMultiplier;
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
        currentGruntIndex = 0;
        currentReelingSFXIndex = 0;

        if (playerTransform != null)
        {
            PlayerScript player = playerTransform.GetComponent<PlayerScript>();
            if (player != null)
            {
                player.FaceRight();
            }

            if (reelingPosition != null)
            {
                playerTransform.position = reelingPosition.position;
            }
        }
    }

    void CatchFish()
    {
        PlaySound(catchSound);

        PlayerAnimationController animController = playerTransform != null
            ? playerTransform.GetComponent<PlayerAnimationController>()
            : null;
        if (animController != null)
        {
            animController.PlayCatch();
        }

        GameObject fishPrefab = pendingFish != null ? pendingFish : GetRandomFish();
        pendingFish = null;

        if (fishPrefab == null)
        {
            Debug.LogWarning("No fish selected or no fish types configured!");
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
            spawnPos = playerTransform != null ? playerTransform.position : transform.position;
        }

        GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);

        FlyingFish caught = fish.GetComponent<FlyingFish>();
        if (caught != null)
        {

            PlayerScript player = playerTransform != null
                ? playerTransform.GetComponent<PlayerScript>()
                : FindFirstObjectByType<PlayerScript>();

            if (player != null)
            {
                player.AddFishScore(caught.scoreValue);
            }

            if (showCatchPopup)
            {
                CatchPopup.Show(caught.fishName, 0, caught.scoreValue);
            }

            bool junk = caught.healthValue <= 0 && caught.scoreValue <= 0;
            SteamAchievements.OnFishCaught(caught.fishName, junk, CountRealSpecies());
        }

        if (waterSplashEffect != null)
        {
            waterSplashEffect.transform.position = spawnPos;
            waterSplashEffect.Play();
        }

        Rigidbody2D rb = fish.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = fish.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
        }

        float randomUpwardForce = Random.Range(minUpwardForce, maxUpwardForce);
        float randomHorizontalForce = Random.Range(minHorizontalForce, maxHorizontalForce);

        Vector2 direction = waterSpawnPoint != null ? (Vector2)waterSpawnPoint.right : Vector2.right;

        Vector2 force = new Vector2(
            direction.x * randomHorizontalForce,
            randomUpwardForce
        );

        rb.AddForce(force, ForceMode2D.Impulse);

        FlyingFish flyingFish = fish.GetComponent<FlyingFish>();
        if (flyingFish != null && fishPile != null)
        {
            flyingFish.fishPile = fishPile;
        }

        ResetFishing();
    }

    int CountRealSpecies()
    {
        if (fishTypes == null) return 0;

        List<string> seen = new List<string>();

        foreach (FishType ft in fishTypes)
        {
            if (ft == null || ft.fishPrefab == null) continue;

            FlyingFish info = ft.fishPrefab.GetComponent<FlyingFish>();
            if (info == null) continue;
            if (info.healthValue <= 0 && info.scoreValue <= 0) continue;
            if (string.IsNullOrEmpty(info.fishName)) continue;

            if (!seen.Contains(info.fishName)) seen.Add(info.fishName);
        }

        return seen.Count;
    }

    GameObject GetRandomFish()
    {
        if (fishTypes == null || fishTypes.Length == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        foreach (FishType fishType in fishTypes)
        {
            if (fishType == null || fishType.fishPrefab == null) continue;
            totalWeight += Mathf.Max(0f, fishType.weight);
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning("FishingRod: fishTypes has no valid prefabs with weight > 0.");
            return null;
        }

        float randomValue = Random.Range(0f, totalWeight);

        float currentWeight = 0f;
        foreach (FishType fishType in fishTypes)
        {
            if (fishType == null || fishType.fishPrefab == null) continue;
            currentWeight += Mathf.Max(0f, fishType.weight);
            if (randomValue <= currentWeight)
            {
                return fishType.fishPrefab;
            }
        }

        foreach (FishType fishType in fishTypes)
        {
            if (fishType != null && fishType.fishPrefab != null) return fishType.fishPrefab;
        }

        return null;
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
        pendingFish = null;
        currentReelDuration = reelInDuration;
        hasBite = false;
        isReelingIn = false;
        isWaitingForBite = false;
        reelInProgress = 0f;

        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        fadeOutCoroutine = StartCoroutine(FadeOutReelingSounds(soundFadeOutDuration));
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    IEnumerator FadeOutReelingSounds(float duration)
    {
        float startGruntVolume = audioSource != null ? audioSource.volume : 1f;
        float startReelVolume = reelingSFXSource != null ? reelingSFXSource.volume : 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (audioSource != null)
            {
                audioSource.volume = Mathf.Lerp(startGruntVolume, 0f, t);
            }
            if (reelingSFXSource != null)
            {
                reelingSFXSource.volume = Mathf.Lerp(startReelVolume, 0f, t);
            }

            yield return null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.volume = startGruntVolume;
        }
        if (reelingSFXSource != null)
        {
            reelingSFXSource.Stop();
            reelingSFXSource.volume = startReelVolume;
        }
    }

    void OnDrawGizmos()
    {
        if (fishPile != null && waterSpawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(waterSpawnPoint.position, fishPile.transform.position);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(fishPile.transform.position, 0.5f);

            Gizmos.color = Color.white;
        }

        if (reelingPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(reelingPosition.position, 0.3f);
            Gizmos.DrawLine(reelingPosition.position, reelingPosition.position + Vector3.up * 0.5f);
        }
    }
}
