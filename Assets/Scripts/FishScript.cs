
using UnityEngine;

public class Fish : MonoBehaviour
{
    public string fishName;
    public int scoreValue; // Different values for different fish
    public float catchDifficulty = 1.0f; //  make some fish harder to catch

    private void Start()
    {
        switch (Random.Range(0, 5)) // Randomly assign fish type
        {
            // Initialize fish properties based on type
            case 1:
            fishName = "Perch";
            scoreValue = 10;
            catchDifficulty = 1.0f;
            break;
            
            case 2:
            fishName = "Northern Pike";
            scoreValue = 20;
            catchDifficulty = 1.5f;
            break;

            case 3:
            fishName = "Trout";
            scoreValue = 30;
            catchDifficulty = 2.0f;
            break;

            case 4:
            fishName = "Legendary";
            scoreValue = 100;
            catchDifficulty = 5.0f;
            break;
        }
    }

}