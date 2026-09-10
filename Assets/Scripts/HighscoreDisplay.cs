using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class HighscoreDisplay : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Leave empty to use the TextMeshProUGUI on the same object.")]
    public TextMeshProUGUI targetText;

    [Header("Appearance")]
    public string title = "HighScore";
    public string steamTitle = "HighScore";
    [Tooltip("Show rows for empty slots as well.")]
    public bool showEmptySlots = false;

    [Header("Steam")]
    [Tooltip("Pull the global list from Steam when available.")]
    public bool useSteamWhenAvailable = true;
    [Tooltip("How long to wait for Steam to find the leaderboard before giving up.")]
    public float steamTimeout = 6f;

    void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }

        WarnIfDuplicate();
    }

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

        Debug.LogWarning("HighscoreDisplay is attached to " + all.Length + " objects: " + objects +
                         ". Varje kopia skriver over sin egen text med topplistan. " +
                         "Remove the component from every object except the one that shows the list.");
    }

    void OnEnable()
    {
        Refresh();

        if (useSteamWhenAvailable && SteamLeaderboards.SteamRunning)
        {
            StartCoroutine(TryFetchFromSteam());
        }
    }

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
