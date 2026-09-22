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
    [Tooltip("Multiplier on walking speed.")]
    public float speedMultiplier = 1.5f;
    [Tooltip("Multiplier on attacks per second. 2 swings twice as often, 0.5 half as often.")]
    public float attackSpeedMultiplier = 1f;

    [Tooltip("Send rushing gnomes at Ragnar while this boss is alive. Set the prefab and timing on the wave manager.")]
    public bool spawnsRushers = false;

    [Header("Arrival")]
    [Tooltip("Where this boss appears. Leave empty to use the Boss Spawn Points list on the wave manager.")]
    public Transform spawnPoint;

    [Header("Look")]
    [Tooltip("Tint laid over the sprite so bosses sharing one model can still be told apart.")]
    public Color tint = Color.white;
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
