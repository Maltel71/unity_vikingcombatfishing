using UnityEngine;
using UnityEditor;

public static class AchievementTools
{
    const string ResetPath = "Tools/Ragnar/Reset Achievement Progress";
    const string ReportPath = "Tools/Ragnar/Show Achievement Progress";

    [MenuItem(ResetPath)]
    static void ResetProgress()
    {
        bool confirmed = EditorUtility.DisplayDialog(
            "Reset achievement progress",
            "This wipes the locally stored achievement progress on this machine: gnome counts, caught species, "
            + "defeated bosses and every unlocked flag.\n\n"
            + "Achievements already granted on your Steam account are not touched. Clear those in Steamworks.",
            "Reset", "Cancel");

        if (!confirmed) return;

        SteamAchievements.ResetLocalProgress();
        Debug.Log("Achievement progress cleared. The game now behaves as it would for a new player.");
    }

    [MenuItem(ReportPath)]
    static void ShowProgress()
    {
        string[] ids =
        {
            SteamAchievements.FirstCatch, SteamAchievements.OldBoot, SteamAchievements.AllFish,
            SteamAchievements.Gnomes50, SteamAchievements.Gnomes250, SteamAchievements.FirstBoss,
            SteamAchievements.AllBosses, SteamAchievements.Wave10, SteamAchievements.Score1000,
            SteamAchievements.DanceBoss
        };

        string report = "Achievement progress on this machine\n";
        report += "Gnomes killed: " + PlayerPrefs.GetInt("ACH_GNOMES_TOTAL", 0) + "\n";
        report += "Species caught: " + PlayerPrefs.GetString("ACH_FISH_SPECIES", "none") + "\n";
        report += "Bosses beaten: " + PlayerPrefs.GetString("ACH_BOSS_KINDS", "none") + "\n";

        foreach (string id in ids)
        {
            report += (SteamAchievements.IsUnlocked(id) ? "  unlocked  " : "  locked    ") + id + "\n";
        }

        Debug.Log(report);
    }
}
