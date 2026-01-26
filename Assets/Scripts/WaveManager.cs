using UnityEngine;
using System.Collections;

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

    [Header("UI")]
    public WaveAnnouncer waveAnnouncer;

    private bool isSpawning = false;

    void Start()
    {
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        // Wait one frame to ensure all UI is initialized
        yield return new WaitForSeconds(0.5f);

        currentWave = 1;

        while (true)
        {
            // Announce wave
            if (waveAnnouncer != null)
            {
                Debug.Log("Calling WaveAnnouncer for wave " + currentWave);
                waveAnnouncer.AnnounceWave(currentWave);
                yield return new WaitForSeconds(2f); // Wait for announcement
            }

            int enemiesToSpawn = startingEnemies + (currentWave - 1) * enemiesIncreasePerWave;

            Debug.Log($"Starting Wave {currentWave}. Spawning {enemiesToSpawn} gnomes.");

            yield return StartCoroutine(SpawnRoutine(enemiesToSpawn));

            yield return new WaitForSeconds(timeBetweenWaves);

            currentWave++; // Increment after wave completes
        }
    }

    IEnumerator SpawnRoutine(int count)
    {
        isSpawning = true;

        for (int i = 0; i < count; i++)
        {
            SpawnGnome();
            yield return new WaitForSeconds(spawnDelay);
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
        }
    }

    public void OnGnomeKilled()
    {
        // No longer needed for wave progression, but kept for compatibility
    }
}