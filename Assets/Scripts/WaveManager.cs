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

    [Header("Bosses")]
    [Tooltip("One entry per boss. If the list is empty the Muscle Prefab below is used instead.")]
    public BossType[] bosses;

    [Tooltip("Fallback, only used when the Bosses list is empty.")]
    public GameObject musclePrefab;

    public BossOrder bossOrder = BossOrder.InOrder;

    [Tooltip("Gnome kills required before the first boss.")]
    public int gnomeKillsPerBoss = 20;
    [Tooltip("Extra gnome kills required per boss already defeated.")]
    public int killsIncreasePerBoss = 5;

    [Tooltip("The boss arrives alone. No regular gnomes spawn while it is alive.")]
    public bool bossComesAlone = true;
    [Tooltip("Safety valve. Resume normal waves if the boss is not dead after this many seconds. 0 waits forever.")]
    public float bossMaxDuration = 120f;
    [Tooltip("Optional dedicated boss spawn points. Empty falls back to the normal spawnPoints.")]
    public Transform[] bossSpawnPoints;

    [Header("Boss Scaling")]
    [Tooltip("0.3 = +30% HP per level. Level 1 is unchanged, level 2 gets +30%, level 3 +60% and so on.")]
    public float healthScalePerLevel = 0.3f;
    public float damageScalePerLevel = 0.12f;
    public float sizeScalePerLevel = 0.05f;
    public float speedScalePerLevel = 0.05f;
    [Tooltip("Cap on boss level. 0 means no cap.")]
    public int maxBossLevel = 0;
    [Tooltip("Print the level as a roman numeral after the name, for example MUSCLE III.")]
    public bool showBossLevel = true;

    [Header("Blood Money")]
    [Tooltip("Blood money per regular gnome killed.")]
    public int bloodPerGnome = 5;
    [Tooltip("Blood money per boss killed.")]
    public int bloodPerBoss = 50;

    [Header("UI")]
    public WaveAnnouncer waveAnnouncer;

    [Header("Music")]
    [Tooltip("When combat music plays. BossOnly means boss fights only.")]
    public CombatMusicMode combatMusic = CombatMusicMode.BossOnly;

    public int TotalGnomesKilled { get; private set; }
    public int LiveEnemies { get; private set; }
    public int BossesDefeated { get; private set; }

    public int BossLevel
    {
        get
        {
            int level = BossesDefeated + 1;
            return maxBossLevel > 0 ? Mathf.Min(level, maxBossLevel) : level;
        }
    }

    public int KillsNeededForNextBoss
    {
        get { return gnomeKillsPerBoss + killsIncreasePerBoss * BossesDefeated; }
    }

    private readonly List<BossType> activeBosses = new List<BossType>();
    private int killsSinceLastBoss = 0;
    private int nextBossIndex = 0;
    private bool bossQueued = false;
    private bool bossAlive = false;
    private bool bossEncounterActive = false;
    private string currentBossName = "";
    private bool combatMusicPlaying = false;
    private PlayerScript player;

    public bool BossAlive { get { return bossAlive; } }

    public EnemyScript ActiveBoss { get; private set; }

    public string ActiveBossLabel { get; private set; }

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

    IEnumerator WaveLoop()
    {

        yield return new WaitForSeconds(0.5f);

        currentWave = 1;

        while (true)
        {

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

    IEnumerator BossWave()
    {
        bossQueued = false;

        BossType boss = PickNextBoss();
        if (boss == null)
        {
            yield break;
        }

        if (bossComesAlone)
        {
            while (LiveEnemies > 0)
            {
                yield return null;
            }
        }

        bossEncounterActive = true;

        if (waveAnnouncer != null)
        {
            waveAnnouncer.AnnounceWave(BuildAnnouncement(boss));
            yield return new WaitForSeconds(2f);
        }

        SpawnBoss(boss);

        float elapsed = 0f;
        while (bossAlive)
        {
            elapsed += Time.deltaTime;
            if (bossMaxDuration > 0f && elapsed >= bossMaxDuration)
            {
                bossAlive = false;
                ActiveBoss = null;
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
        ActiveBossLabel = BuildAnnouncement(boss);
        ActiveBoss = SpawnEnemy(boss.prefab, sp, boss);
    }

    EnemyScript SpawnEnemy(GameObject prefab, Transform spawnPoint, BossType boss)
    {
        GameObject newEnemy = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        EnemyScript enemy = newEnemy.GetComponent<EnemyScript>();
        if (enemy != null)
        {
            enemy.manager = this;
            enemy.isElite = boss != null;

            if (boss != null)
            {

                int steps = BossLevel - 1;
                enemy.eliteHealthMultiplier = boss.healthMultiplier * (1f + healthScalePerLevel * steps);
                enemy.eliteDamageMultiplier = boss.damageMultiplier * (1f + damageScalePerLevel * steps);
                enemy.eliteSizeMultiplier = boss.sizeMultiplier * (1f + sizeScalePerLevel * steps);
                enemy.eliteSpeedMultiplier = boss.speedMultiplier * (1f + speedScalePerLevel * steps);
            }
        }

        LiveEnemies++;
        return enemy;
    }

    public void OnEnemyKilled(EnemyScript enemy)
    {
        LiveEnemies = Mathf.Max(0, LiveEnemies - 1);

        if (enemy != null && enemy.isElite)
        {
            bossAlive = false;
            ActiveBoss = null;
            BossesDefeated++;
            AwardBlood(bloodPerBoss, true);
            SteamAchievements.OnBossKilled(currentBossName);
            return;
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

    public void OnGnomeKilled()
    {
        OnEnemyKilled(null);
    }

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
