using UnityEngine;
using UnityEngine.SceneManagement;

public class RageMeter : MonoBehaviour
{
    public static RageMeter Instance { get; private set; }

    [Header("Build Up")]
    [Tooltip("Rage needed before the berserk swing can be unleashed.")]
    public float maxRage = 100f;
    [Tooltip("Rage gained per enemy actually hit. Two gnomes in one swing count as two.")]
    public float ragePerHit = 12f;

    [Header("Losing It")]
    [Tooltip("Share of the meter lost every time Ragnar takes a hit. 1 empties it completely.")]
    [Range(0f, 1f)]
    public float lossOnDamage = 0.4f;
    [Tooltip("Rage lost per second once the fighting stops. 0 banks it forever.")]
    public float decayPerSecond = 4f;
    [Tooltip("Quiet seconds before the decay starts.")]
    public float decayDelay = 5f;

    [Header("Berserk Swing")]
    [Tooltip("Damage multiplier on the berserk swing.")]
    public float damageMultiplier = 3f;
    [Tooltip("Seconds the attack collider stays open, so the swing sweeps a crowd.")]
    public float swingDuration = 0.35f;

    public float Rage { get; private set; }
    public bool IsFull { get { return Rage >= maxRage; } }

    public float Fill
    {
        get { return maxRage > 0f ? Mathf.Clamp01(Rage / maxRage) : 0f; }
    }

    private float lastGain;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (FindFirstObjectByType<RageMeter>(FindObjectsInactive.Include) != null) return;

        PlayerScript player = FindFirstObjectByType<PlayerScript>();
        if (player == null) return;

        player.gameObject.AddComponent<RageMeter>();
    }

    void Awake()
    {
        Instance = this;
        lastGain = Time.time;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (decayPerSecond <= 0f) return;
        if (Rage <= 0f) return;
        if (Time.time < lastGain + decayDelay) return;

        Rage = Mathf.Max(0f, Rage - decayPerSecond * Time.deltaTime);
    }

    public static void Add(float amount)
    {
        if (amount <= 0f) return;

        RageMeter meter = Instance;
        if (meter == null)
        {
            meter = FindFirstObjectByType<RageMeter>(FindObjectsInactive.Include);
        }

        if (meter != null) meter.AddRage(amount);
    }

    public void AddRage(float amount)
    {
        if (amount <= 0f) return;

        Rage = Mathf.Min(maxRage, Rage + amount);
        lastGain = Time.time;
    }

    public void AddHits(int hits)
    {
        if (hits <= 0) return;

        AddRage(ragePerHit * hits);
    }

    public void LoseOnDamage()
    {
        if (lossOnDamage <= 0f) return;

        Rage = Mathf.Max(0f, Rage - maxRage * lossOnDamage);
        lastGain = Time.time;
    }

    public bool Consume()
    {
        if (!IsFull) return false;

        Rage = 0f;
        lastGain = Time.time;
        return true;
    }

    public void Clear()
    {
        Rage = 0f;
        lastGain = Time.time;
    }
}
