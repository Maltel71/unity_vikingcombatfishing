using UnityEngine;
using System.Collections;

public class EndlessWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int currentWave = 1;
    public float timeBetweenWaves = 10f;
    public GameObject[] gnomePrefabs;
    public Transform[] spawnPoints;

    [Header("Static Scaling")]
    // Set this to 5 in the Inspector to lock every round to 5 enemies
    public int enemiesPerWaveFixed = 5;

    private int enemiesRemaining;
    private bool isSpawning = false;

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        if (enemiesRemaining <= 0 && !isSpawning)
        {
            StartCoroutine(WaveDelay());
        }
    }

    IEnumerator WaveDelay()
    {
        isSpawning = true;
        Debug.Log($"Wave {currentWave} Cleared! Next wave in {timeBetweenWaves} seconds.");
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        StartNextWave();
    }

    void StartNextWave()
    {
        // Removed exponential math. Now always spawns 5.
        enemiesRemaining = enemiesPerWaveFixed;

        Debug.Log($"Starting Wave {currentWave}. Gnomes to slay: {enemiesRemaining}");
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        isSpawning = true;
        for (int i = 0; i < enemiesPerWaveFixed; i++)
        {
            SpawnGnome();

            // Fixed spawn delay (1.0s) so it doesn't get faster over time
            yield return new WaitForSeconds(1.0f);
        }
        isSpawning = false;
    }

    void SpawnGnome()
    {
        if (gnomePrefabs.Length == 0 || spawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, gnomePrefabs.Length);
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject newGnome = Instantiate(gnomePrefabs[randomIndex], sp.position, sp.rotation);

        EnemyScript enemy = newGnome.GetComponent<EnemyScript>();
        if (enemy != null)
        {
            enemy.manager = this;
            // Stat scaling (speed, health, damage) has been removed here 
            // to ensure every round feels exactly the same.
        }
    }

    public void OnGnomeKilled()
    {
        enemiesRemaining--;
    }
}
