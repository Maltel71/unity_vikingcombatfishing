using UnityEngine;
using System.Collections;
// Add this if GnomeEnemy is in another namespace, e.g. MyGame.Enemies
// using MyGame.Enemies;

public class EndlessWaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public int currentWave = 1;
    public float timeBetweenWaves = 5f;
    public GameObject[] gnomePrefabs; // Your different Gnome types
    public Transform[] spawnPoints;

    [Header("Scaling Variables")]
    public int enemiesPerWaveBase = 5;
    public float difficultyMultiplier = 1.05f; // Enemies increase by 20% each wave

    private int enemiesToSpawn;
    private int enemiesRemaining;
    private bool isSpawning = false;

    void Start()
    {
        StartNextWave();
    }

    void Update()
    {
        // Start next wave if all gnomes are dead and we aren't spawning
        if (enemiesRemaining <= 0 && !isSpawning)
        {
            StartCoroutine(WaveDelay());
        }
    }

    IEnumerator WaveDelay()
    {
        isSpawning = true; // Prevents multiple calls
        Debug.Log("Wave Cleared! Next wave in " + timeBetweenWaves + " seconds.");
        yield return new WaitForSeconds(timeBetweenWaves);

        currentWave++;
        StartNextWave();
    }

    void StartNextWave()
    {
        // COD ZOMBIES FORMULA: Scale enemy count based on wave number
        enemiesToSpawn = Mathf.RoundToInt(enemiesPerWaveBase * Mathf.Pow(currentWave, difficultyMultiplier));
        enemiesRemaining = enemiesToSpawn;

        Debug.Log("Starting Wave " + currentWave + ". Gnomes to slay: " + enemiesToSpawn);
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        isSpawning = true;
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnGnome();
            // Faster spawning in higher rounds
            float spawnDelay = Mathf.Clamp(1.5f - (currentWave * 0.05f), 0.2f, 1.5f);
            yield return new WaitForSeconds(spawnDelay);
        }
        isSpawning = false;
    }

    void SpawnGnome()
    {
        int randomIndex = Random.Range(0, gnomePrefabs.Length);
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject newGnome = Instantiate(gnomePrefabs[randomIndex], sp.position, sp.rotation);

        // CHANGE THIS: Match the name of your script (EnemyScript)
        EnemyScript gnomeScript = newGnome.GetComponent<EnemyScript>();

        if (gnomeScript != null)
        {
            // This gives the Gnome the "Address" of this manager
            gnomeScript.manager = this;

            // Speed scaling
            gnomeScript.movementSpeed += (currentWave * 0.15f);
        }
    }

    public void OnGnomeKilled()
    {
        enemiesRemaining--;
    }
}
