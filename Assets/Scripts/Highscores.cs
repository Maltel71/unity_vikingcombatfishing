using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Topplistan. Sparas i PlayerPrefs och overlever att spelet stangs av.
/// Poangen som lagras ar PlayerScript.TotalScore, alltsa fiskepoang + blodspengar.
/// </summary>
public static class Highscores
{
    public const int MaxEntries = 5;
    public const int MaxNameLength = 12;

    const string NameKey = "HS_Name_";
    const string ScoreKey = "HS_Score_";
    const string SeededKey = "HS_Seeded";

    public struct Entry
    {
        public string name;
        public int score;

        public Entry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    // Listan borjar inte tom - annars ser menyn trasig ut forsta gangen
    static readonly Entry[] DefaultEntries =
    {
        new Entry("Loke", 500),
        new Entry("Rosen", 400),
        new Entry("Malte", 300),
        new Entry("Martin", 200),
        new Entry("DiskWasher", 100),
    };

    public static List<Entry> Load()
    {
        EnsureSeeded();

        List<Entry> entries = new List<Entry>(MaxEntries);
        for (int i = 0; i < MaxEntries; i++)
        {
            string name = PlayerPrefs.GetString(NameKey + i, "");
            int score = PlayerPrefs.GetInt(ScoreKey + i, 0);

            if (!string.IsNullOrEmpty(name))
            {
                entries.Add(new Entry(name, score));
            }
        }

        entries.Sort((a, b) => b.score.CompareTo(a.score));
        return entries;
    }

    static void EnsureSeeded()
    {
        if (PlayerPrefs.GetInt(SeededKey, 0) == 1) return;

        for (int i = 0; i < DefaultEntries.Length && i < MaxEntries; i++)
        {
            PlayerPrefs.SetString(NameKey + i, DefaultEntries[i].name);
            PlayerPrefs.SetInt(ScoreKey + i, DefaultEntries[i].score);
        }

        PlayerPrefs.SetInt(SeededKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>Racker poangen till en plats pa listan?</summary>
    public static bool Qualifies(int score)
    {
        if (score <= 0) return false;

        List<Entry> entries = Load();
        if (entries.Count < MaxEntries) return true;

        return score > entries[entries.Count - 1].score;
    }

    /// <summary>Lagger in resultatet och returnerar placeringen (1-5), eller 0 om det inte rackte.</summary>
    public static int Add(string name, int score)
    {
        if (string.IsNullOrEmpty(name)) name = "Viking";
        if (name.Length > MaxNameLength) name = name.Substring(0, MaxNameLength);

        List<Entry> entries = Load();
        entries.Add(new Entry(name, score));
        entries.Sort((a, b) => b.score.CompareTo(a.score));

        if (entries.Count > MaxEntries)
        {
            entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
        }

        Save(entries);

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].name == name && entries[i].score == score)
            {
                return i + 1;
            }
        }

        return 0;
    }

    public static void Save(List<Entry> entries)
    {
        for (int i = 0; i < MaxEntries; i++)
        {
            if (i < entries.Count)
            {
                PlayerPrefs.SetString(NameKey + i, entries[i].name);
                PlayerPrefs.SetInt(ScoreKey + i, entries[i].score);
            }
            else
            {
                PlayerPrefs.DeleteKey(NameKey + i);
                PlayerPrefs.DeleteKey(ScoreKey + i);
            }
        }

        PlayerPrefs.SetInt(SeededKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>Nollar listan till standardnamnen igen.</summary>
    public static void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(SeededKey);
        for (int i = 0; i < MaxEntries; i++)
        {
            PlayerPrefs.DeleteKey(NameKey + i);
            PlayerPrefs.DeleteKey(ScoreKey + i);
        }
        EnsureSeeded();
    }
}
