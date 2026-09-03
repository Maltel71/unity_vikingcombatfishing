using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Ritar topplistan i en TextMeshPro-text. Ligger pa "Highscore"-texten i MenuScene.
///
/// Visar Steams globala lista nar Steam ar igang, annars den lokala i Highscores.
/// Den lokala ritas alltid ut direkt sa rutan aldrig star tom medan Steam svarar.
/// </summary>
public class HighscoreDisplay : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Lamnas den tom anvands TextMeshProUGUI pa samma objekt.")]
    public TextMeshProUGUI targetText;

    [Header("Utseende")]
    public string title = "HighScore";
    public string steamTitle = "HighScore";
    [Tooltip("Visa rader aven for tomma platser.")]
    public bool showEmptySlots = false;

    [Header("Steam")]
    [Tooltip("Hamta den globala listan fran Steam nar det gar.")]
    public bool useSteamWhenAvailable = true;
    [Tooltip("Hur lange vi vantar pa att Steam ska hitta topplistan innan vi ger upp.")]
    public float steamTimeout = 6f;

    void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }

        WarnIfDuplicate();
    }

    /// <summary>
    /// Hamnar scriptet av misstag pa fel textobjekt skriver det over den texten
    /// med topplistan - t.ex. spelets titel. Det ar svart att lista ut i efterhand,
    /// sa vi sager ifran med en gang och namnger objekten.
    /// </summary>
    void WarnIfDuplicate()
    {
        HighscoreDisplay[] all = FindObjectsByType<HighscoreDisplay>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (all.Length <= 1) return;

        string objects = "";
        foreach (HighscoreDisplay d in all)
        {
            if (d == null) continue;
            if (objects.Length > 0) objects += ", ";
            objects += d.gameObject.name;
        }

        Debug.LogWarning("HighscoreDisplay sitter pa " + all.Length + " objekt: " + objects +
                         ". Varje kopia skriver over sin egen text med topplistan. " +
                         "Ta bort komponenten fran alla utom det objekt som ska visa listan.");
    }

    void OnEnable()
    {
        Refresh();

        if (useSteamWhenAvailable && SteamLeaderboards.SteamRunning)
        {
            StartCoroutine(TryFetchFromSteam());
        }
    }

    /// <summary>Ritar den lokala listan.</summary>
    public void Refresh()
    {
        if (targetText == null) return;

        List<Highscores.Entry> entries = Highscores.Load();

        StringBuilder sb = new StringBuilder();
        AppendTitle(sb, title);

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

        Apply(sb);
    }

    IEnumerator TryFetchFromSteam()
    {
        // Topplistan hittas asynkront strax efter uppstart - ge den en stund
        float waited = 0f;
        while (!SteamLeaderboards.IsReady && waited < steamTimeout)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!SteamLeaderboards.IsReady) yield break;

        SteamLeaderboards.RequestTopEntries(Highscores.MaxEntries, ShowSteamEntries);
    }

    void ShowSteamEntries(List<SteamLeaderboards.Entry> entries)
    {
        // Tom global lista? Behall den lokala, den ser mindre trakig ut
        if (targetText == null || entries == null || entries.Count == 0) return;

        StringBuilder sb = new StringBuilder();
        AppendTitle(sb, steamTitle);

        foreach (SteamLeaderboards.Entry e in entries)
        {
            sb.AppendLine(e.rank + "." + e.name + "  " + e.score);
        }

        Apply(sb);
    }

    void AppendTitle(StringBuilder sb, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            sb.AppendLine(text);
        }
    }

    void Apply(StringBuilder sb)
    {
        UiKit.SetText(targetText, sb.ToString().TrimEnd());
    }
}
