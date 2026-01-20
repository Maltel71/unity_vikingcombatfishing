using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishingRod : MonoBehaviour
{
    public List<GameObject> fishPrefabs; // Assign different fish prefabs here
    public int totalScore = 0;

    private bool isFishing = false;
    private float waitTime;

    void Update()
    {
        // Press F to start/stop fishing
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isFishing) StartCoroutine(StartFishing());
            else StopFishing();
        }
    }

    IEnumerator StartFishing()
    {
        isFishing = true;
        Debug.Log("Casting line... waiting for a bite.");

        // Random wait time between 2 and 5 seconds for a bite
        waitTime = Random.Range(2f, 5f);
        yield return new WaitForSeconds(waitTime);

        CatchFish();
    }

    void CatchFish()
    {
        if (!isFishing) return;

        // Randomly select a fish from your list
        int randomIndex = Random.Range(0, fishPrefabs.Count);
        GameObject caughtPrefab = fishPrefabs[randomIndex];
        Fish fishData = caughtPrefab.GetComponent<Fish>();

        totalScore += fishData.scoreValue;
        Debug.Log($"Caught a {fishData.fishName}! +{fishData.scoreValue} points. Total Score: {totalScore}");

        StopFishing();
    }

    void StopFishing()
    {
        isFishing = false;
        Debug.Log("Line reeled in.");
    }
}
