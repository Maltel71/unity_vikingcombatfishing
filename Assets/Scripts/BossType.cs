using UnityEngine;

[System.Serializable]
public class BossType
{
    public string bossName = "Muscle";
    public GameObject prefab;

    [Tooltip("Announcement text. Leave empty to use bossName in capitals.")]
    public string announcementText = "";

    [Header("Base Values (First Boss)")]
    [Tooltip("Multiplier on the prefab health.")]
    public float healthMultiplier = 1f;
    [Tooltip("Multiplier on the prefab damage.")]
    public float damageMultiplier = 1f;
    [Tooltip("Boss size. Unlike regular gnomes this is not randomised.")]
    public float sizeMultiplier = 1.6f;
    public float speedMultiplier = 1.5f;
}

public enum CombatMusicMode
{
    Off,
    AnyEnemy,
    BossOnly
}

public enum BossOrder
{
    InOrder,
    Random
}
