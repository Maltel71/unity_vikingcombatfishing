using UnityEngine;
using Steamworks;

public static class SteamAchievements
{

    public const string FirstCatch = "ACH_FIRST_CATCH";
    public const string OldBoot = "ACH_OLD_BOOT";
    public const string AllFish = "ACH_ALL_FISH";
    public const string Gnomes50 = "ACH_GNOMES_50";
    public const string Gnomes250 = "ACH_GNOMES_250";
    public const string FirstBoss = "ACH_FIRST_BOSS";
    public const string AllBosses = "ACH_ALL_BOSSES";
    public const string Wave10 = "ACH_WAVE_10";
    public const string Score1000 = "ACH_SCORE_1000";
    public const string DanceBoss = "ACH_DANCE_BOSS";

    public const int GnomeGoalSmall = 50;
    public const int GnomeGoalLarge = 250;
    public const int WaveGoal = 10;
    public const int ScoreGoal = 1000;
    public const int BossKindGoal = 3;

    const string UnlockedPrefix = "ACH_UNLOCKED_";
    const string GnomeCountKey = "ACH_GNOMES_TOTAL";
    const string BossKindsKey = "ACH_BOSS_KINDS";
    const string SpeciesKey = "ACH_FISH_SPECIES";

    public static bool IsUnlocked(string id)
    {
        return PlayerPrefs.GetInt(UnlockedPrefix + id, 0) == 1;
    }

    public static void Unlock(string id)
    {
        if (string.IsNullOrEmpty(id) || IsUnlocked(id)) return;

        PlayerPrefs.SetInt(UnlockedPrefix + id, 1);
        PlayerPrefs.Save();

        if (!SteamLeaderboards.SteamRunning) return;

        bool already;
        if (SteamUserStats.GetAchievement(id, out already) && already) return;

        if (SteamUserStats.SetAchievement(id))
        {
            SteamUserStats.StoreStats();
            Debug.Log("Achievement unlocked: " + id);
        }
    }

    public static void OnFishCaught(string fishName, bool isJunk, int totalSpecies)
    {
        if (isJunk)
        {
            Unlock(OldBoot);
            return;
        }

        Unlock(FirstCatch);

        if (string.IsNullOrEmpty(fishName)) return;

        int caught = AddToSet(SpeciesKey, fishName);

        if (totalSpecies > 0 && caught >= totalSpecies)
        {
            Unlock(AllFish);
        }
    }

    public static void OnGnomeKilled()
    {
        int total = PlayerPrefs.GetInt(GnomeCountKey, 0) + 1;
        PlayerPrefs.SetInt(GnomeCountKey, total);

        if (total % 10 == 0) PlayerPrefs.Save();

        if (total >= GnomeGoalSmall) Unlock(Gnomes50);
        if (total >= GnomeGoalLarge) Unlock(Gnomes250);
    }

    public static void OnBossKilled(string bossName)
    {
        Unlock(FirstBoss);

        if (string.IsNullOrEmpty(bossName)) return;

        int kinds = AddToSet(BossKindsKey, bossName);
        if (kinds >= BossKindGoal) Unlock(AllBosses);
    }

    public static void OnWaveReached(int wave)
    {
        if (wave >= WaveGoal) Unlock(Wave10);
    }

    public static void OnScoreChanged(int totalScore)
    {
        if (totalScore >= ScoreGoal) Unlock(Score1000);
    }

    public static void OnDanceStarted(bool bossAlive)
    {
        if (bossAlive) Unlock(DanceBoss);
    }

    static int AddToSet(string key, string value)
    {
        string stored = PlayerPrefs.GetString(key, "");
        string padded = "|" + value + "|";

        if (!stored.Contains(padded))
        {
            stored = stored.Length == 0 ? padded : stored + value + "|";
            PlayerPrefs.SetString(key, stored);
            PlayerPrefs.Save();
        }

        int count = 0;
        for (int i = 0; i < stored.Length; i++)
        {
            if (stored[i] == '|') count++;
        }
        return Mathf.Max(0, count - 1);
    }

    public static void ResetLocalProgress()
    {
        string[] all = { FirstCatch, OldBoot, AllFish, Gnomes50, Gnomes250,
                         FirstBoss, AllBosses, Wave10, Score1000, DanceBoss };

        foreach (string id in all)
        {
            PlayerPrefs.DeleteKey(UnlockedPrefix + id);
        }

        PlayerPrefs.DeleteKey(GnomeCountKey);
        PlayerPrefs.DeleteKey(BossKindsKey);
        PlayerPrefs.DeleteKey(SpeciesKey);
        PlayerPrefs.Save();
    }
}
