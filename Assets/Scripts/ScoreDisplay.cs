using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private PlayerScript player;

    void Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }

        player = FindObjectOfType<PlayerScript>();
    }

    void Update()
    {
        if (player != null && scoreText != null)
        {
            scoreText.text = "Score: " + player.playerScore;
        }
    }
}