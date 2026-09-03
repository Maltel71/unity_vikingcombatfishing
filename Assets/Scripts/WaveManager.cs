using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int currentWave = 0;
    public float timeBetweenWaves = 10f;
    public GameObject[] gnomePrefabs;
    public Transform[] spawnPoints;

    [Header("Enemy Scaling")]
    public int startingEnemies = 5;
    public int enemiesIncreasePerWave = 2;
    public float spawnDelay = 1.0f;

    [Header("Bossar")]
    [Tooltip("En post per boss. Ar listan tom anvands Muscle Prefab nedan istallet.")]
    public BossType[] bosses;

    [Tooltip("RESERV: anvands bara om listan Bosses ar tom. Gamla enskilda bossfaltet.")]
    public GameObject musclePrefab;

    public BossOrder bossOrder = BossOrder.InOrder;

    [Tooltip("Antal dodade gnomer innan forsta bossen.")]
    public int gnomeKillsPerBoss = 20;
    [Tooltip("Hur manga fler gnomer som kravs for varje boss du besegrat.")]
    public int killsIncreasePerBoss = 5;

    [Tooltip("Bossen kommer ensam - inga vanliga gnomer spawnas medan han lever.")]
    public bool bossComesAlone = true;
    [Tooltip("Sakerhetsventil: fortsatt med vanliga vagor om bossen inte dott inom sa har manga sekunder. 0 = vanta hur lange som helst.")]
    public float bossMaxDuration = 120f;
    [Tooltip("Valfria egna spawnpunkter for bossar. Tom = anvander vanliga spawnPoints.")]
    public Transform[] bossSpawnPoints;

    [Header("Bossen blir starkare for varje boss du besegrat")]
    [Tooltip("0.3 = +30% HP per niva. Niva 1 ar oforandrad, niva 2 far +30%, niva 3 +60% osv.")]
    public float healthScalePerLevel = 0.3f;
    public float damageScalePerLevel = 0.12f;
    public float sizeScalePerLevel = 0.05f;
    public float speedScalePerLevel = 0.05f;
    [Tooltip("Tak for hur hogt bossnivan kan ga. 0 = inget tak.")]
    public int maxBossLevel = 0;
    [Tooltip("Skriver ut nivan som romersk siffra efter namnet, t.ex. MUSCLE III.")]
    public bool showBossLevel = true;

    [Header("Blodspengar")]
    [Tooltip("Blodspengar per dodad vanlig gnom.")]
    public int bloodPerGnome = 5;
    [Tooltip("Blodspengar per dodad boss.")]
    public int bloodPerBoss = 50;

    [Header("UI")]
    public WaveAnnouncer waveAnnouncer;

    [Header("Musik")]
    [Tooltip("Nar stridsmusiken ska spelas. BossOnly = bara under bossmoten.")]
    public CombatMusicMode combatMusic = CombatMusicMode.BossOnly;

    // Lasbar statistik for UI / framtida highscore
    public int TotalGnomesKilled { get; private set; }
    public int LiveEnemies { get; private set; }
    public int BossesDefeated { get; private set; }

    /// <summary>Nivan pa nasta boss. Forsta bossen ar niva 1.</summary>
    public int BossLevel
    {
        get
        {
            int level = BossesDefeated + 1;
            return maxBossLevel > 0 ? Mathf.Min(level, maxBossLevel) : level;
        }
    }

    /// <summary>Hur manga gnomer som kravs innan nasta boss.</summary>
    public int KillsNeededForNextBoss
    {
        get { return gnomeKillsPerBoss + killsIncreasePerBoss * BossesDefeated; }
    }

    private readonly List<BossType> activeBosses = new List<BossType>();
    private int killsSinceLastBoss = 0;
    private int nextBossIndex = 0;
    private bool bossQueued = false;
    private bool bossAlive = false;
    private bool bossEncounterActive = false;   // sant fran utropet tills han ar dod
    private string currentBossName = "";
    private bool combatMusicPlaying = false;
    private PlayerScript player;

    /// <summary>Lever en boss just nu? Anvands bl.a. av dans-achievementen.</summary>
    public bool BossAlive { get { return bossAlive; } }

    void Start()
    {
        player = FindFirstObjectByType<PlayerScript>();
        BuildBossList();
        StartCoroutine(WaveLoop());
    }

    void Update()
    {
        UpdateCombatMusic();
    }

    // ---------------- Bosslista ----------------

    void BuildBossList()
    {
        activeBosses.Clear();

        if (bosses != null)
        {
            foreach (BossType boss in bosses)
            {
                if (boss != null && boss.prefab != null)
                {
                    activeBosses.Add(boss);
                }
            }
        }

        // Ingen lista ifylld men det gamla enskilda faltet ar satt - anvand det
        if (activeBosses.Count == 0 && musclePrefab != null)
        {
            BossType fallback = new BossType();
            fallback.bossName = "Muscle";
            fallback.prefab = musclePrefab;
            activeBosses.Add(fallback);
        }
    }

    bool HasBosses { get { return activeBosses.Count > 0; } }

    BossType PickNextBoss()
    {
        if (activeBosses.Count == 0) return null;

        if (bossOrder == BossOrder.Random)
        {
            return activeBosses[Random.Range(0, activeBosses.Count)];
        }

        BossType boss = activeBosses[nextBossIndex % activeBosses.Count];
        nextBossIndex++;
        return boss;
    }

    // ---------------- Musik ----------------

    void UpdateCombatMusic()
    {
        if (combatMusic == CombatMusicMode.Off) return;
        if (MusicManager.Instance == null) return;

        bool shouldPlayCombat = combatMusic == CombatMusicMode.BossOnly
            ? bossEncounterActive
            : LiveEnemies > 0;

        if (shouldPlayCombat && !combatMusicPlaying)
        {
            MusicManager.Instance.StartaStrid();
            combatMusicPlaying = true;
        }
        else if (!shouldPlayCombat && combatMusicPlaying)
        {
            MusicManager.Instance.AvslutaStrid();
            combatMusicPlaying = false;
        }
    }

    // ---------------- Vagloop ----------------

    IEnumerator WaveLoop()
    {
        // Wait a moment to ensure all UI is initialized
        yield return new WaitForSeconds(0.5f);

        currentWave = 1;

        while (true)
        {
            // Star en boss pa tur? Da kor vi en bossvag istallet for en vanlig vag.
            if (bossQueued && HasBosses)
            {
                yield return StartCoroutine(BossWave());
                continue;
            }

            SteamAchievements.OnWaveReached(currentWave);

            if (waveAnnouncer != null)
            {
                waveAnnouncer.AnnounceWave(currentWave);
                yield return new WaitForSeconds(2f);
            }

            int enemiesToSpawn = startingEnemies + (currentWave - 1) * enemiesIncreasePerWave;

            yield return StartCoroutine(SpawnRoutine(enemiesToSpawn));

            yield return new WaitForSeconds(timeBetweenWaves);

            currentWave++;
        }
    }

    IEnumerator SpawnRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Blev en boss koad mitt i vagen: avbryt resten sa han kan komma ensam
            if (bossQueued && HasBosses && bossComesAlone)
            {
                yield break;
            }

            SpawnGnome();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    void SpawnGnome()
    {
        if (gnomePrefabs == null || gnomePrefabs.Length == 0) return;
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        GameObject prefab = gnomePrefabs[Random.Range(0, gnomePrefabs.Length)];
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (prefab == null || sp == null) return;

        SpawnEnemy(prefab, sp, null);
    }

    // ---------------- Bossvag ----------------

    IEnumerator BossWave()
    {
        bossQueued = false;

        BossType boss = PickNextBoss();
        if (boss == null)
        {
            yield break;
        }

        // Vanta ut kvarvarande gnomer sa han verkligen kommer ensam
        if (bossComesAlone)
        {
            while (LiveEnemies > 0)
            {
                yield return null;
            }
        }

        // Musiken slar om redan vid utropet sa han far en entre
        bossEncounterActive = true;

        if (waveAnnouncer != null)
        {
            waveAnnouncer.AnnounceWave(BuildAnnouncement(boss));
            yield return new WaitForSeconds(2f);
        }

        SpawnBoss(boss);

        // Vanta tills han ar dod (eller tills sakerhetsventilen loser ut)
        float elapsed = 0f;
        while (bossAlive)
        {
            elapsed += Time.deltaTime;
            if (bossMaxDuration > 0f && elapsed >= bossMaxDuration)
            {
                bossAlive = false;
                break;
            }
            yield return null;
        }

        bossEncounterActive = false;

        yield return new WaitForSeconds(timeBetweenWaves);
        currentWave++;
    }

    string BuildAnnouncement(BossType boss)
    {
        string text = string.IsNullOrEmpty(boss.announcementText)
            ? (boss.bossName != null ? boss.bossName.ToUpper() : "BOSS")
            : boss.announcementText;

        int level = BossLevel;
        if (showBossLevel && level > 1)
        {
            text += " " + ToRoman(level);
        }

        return text;
    }

    void SpawnBoss(BossType boss)
    {
        Transform[] points = (bossSpawnPoints != null && bossSpawnPoints.Length > 0)
            ? bossSpawnPoints
            : spawnPoints;

        if (points == null || points.Length == 0) return;

        Transform sp = points[Random.Range(0, points.Length)];
        if (sp == null) return;

        bossAlive = true;
        currentBossName = boss.bossName;
        SpawnEnemy(boss.prefab, sp, boss);
    }

    // ---------------- Gemensam spawn ----------------

    void SpawnEnemy(GameObject prefab, Transform spawnPoint, BossType boss)
    {
        GameObject newEnemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        EnemyScript enemy = newEnemy.GetComponent<EnemyScript>();
        if (enemy != null)
        {
            enemy.manager = this;
            enemy.isElite = boss != null;

            if (boss != null)
            {
                // Maste satas fore EnemyScript.Start() - dar anvands de i ApplyVariations().
                // Niva 1 = bossens grundvarden, varje niva darefter skalar upp dem.
                int steps = BossLevel - 1;
                enemy.eliteHealthMultiplier = boss.healthMultiplier * (1f + healthScalePerLevel * steps);
                enemy.eliteDamageMultiplier = boss.damageMultiplier * (1f + damageScalePerLevel * steps);
                enemy.eliteSizeMultiplier = boss.sizeMultiplier * (1f + sizeScalePerLevel * steps);
                enemy.eliteSpeedMultiplier = boss.speedMultiplier * (1f + speedScalePerLevel * steps);
            }
        }

        LiveEnemies++;
    }

    // ---------------- Callbacks fran EnemyScript ----------------

    public void OnEnemyKilled(EnemyScript enemy)
    {
        LiveEnemies = Mathf.Max(0, LiveEnemies - 1);

        if (enemy != null && enemy.isElite)
        {
            bossAlive = false;
            BossesDefeated++;
            AwardBlood(bloodPerBoss, true);
            SteamAchievements.OnBossKilled(currentBossName);
            return; // bosskill raknas inte mot nasta boss
        }

        TotalGnomesKilled++;
        killsSinceLastBoss++;
        AwardBlood(bloodPerGnome, false);
        SteamAchievements.OnGnomeKilled();

        if (HasBosses && KillsNeededForNextBoss > 0 && killsSinceLastBoss >= KillsNeededForNextBoss)
        {
            killsSinceLastBoss = 0;
            bossQueued = true;
        }
    }

    void AwardBlood(int amount, bool wasBoss)
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerScript>();
        }

        if (player != null)
        {
            player.AddBloodMoney(amount, wasBoss);
        }
    }

    // Bakatkompatibel wrapper
    public void OnGnomeKilled()
    {
        OnEnemyKilled(null);
    }

    // ---------------- Smatt och gott ----------------

    static readonly int[] RomanValues = { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
    static readonly string[] RomanNumerals = { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

    static string ToRoman(int number)
    {
        if (number <= 0 || number > 3999) return number.ToString();

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < RomanValues.Length; i++)
        {
            while (number >= RomanValues[i])
            {
                sb.Append(RomanNumerals[i]);
                number -= RomanValues[i];
            }
        }
        return sb.ToString();
    }
}
