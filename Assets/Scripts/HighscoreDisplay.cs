using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Ritar topplistan i en TextMeshPro-text. Lagg den pa "Highscore"-texten i MenuScene.
/// Uppdaterar sig varje gang objektet aktiveras, sa listan ar farsk nar man kommer
/// tillbaka fran en runda.
/// </summary>
public class HighscoreDisplay : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Lamnas den tom anvands TextMeshProUGUI pa samma objekt.")]
    public TextMeshProUGUI targetText;

    [Header("Utseende")]
    public string title = "HighScore";
    [Tooltip("Visa rader aven for tomma platser.")]
    public bool showEmptySlots = false;

    void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (targetText == null) return;

        List<Highscores.Entry> entries = Highscores.Load();

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(title))
        {
            sb.AppendLine(title);
        }

        for (int i = 0; i < Highscores.MaxEntries; i++)
        {
            if (i < entries.Count)
            {
                sb.AppendLine((i + 1) + "." + entries[i].name + "  " + entries[i].score);
            }
            else if (showEmptySlots)
            {
                sb.AppendLine((i + 1) + ". ---");
            }
        }

        targetText.text = sb.ToString().TrimEnd();
    }
}
