using UnityEngine;

/// <summary>
/// En bosstyp. Lagg till en post per boss i EndlessWaveManager.bosses.
/// Innan alla bossar ar modellerade kan flera poster peka pa samma prefab -
/// de far anda olika namn, utrop och grundvarden.
///
/// OBS: den har klassen MASTE ligga i en egen fil. Lag den i WaveManager.cs
/// tog Unity fileID 11500000 fran EndlessWaveManager och gav den till BossType,
/// vilket gav "'BossType' is missing the class attribute 'ExtensionOfNativeClass'"
/// och "references runtime script in scene file. Fixing!".
/// </summary>
[System.Serializable]
public class BossType
{
    public string bossName = "Muscle";
    public GameObject prefab;

    [Tooltip("Texten som ropas ut. Lamnas den tom anvands bossName i versaler.")]
    public string announcementText = "";

    [Header("Grundvarden - galler forsta gangen bossen dyker upp")]
    [Tooltip("Multiplikator pa prefabens health.")]
    public float healthMultiplier = 1f;
    [Tooltip("Multiplikator pa prefabens damage.")]
    public float damageMultiplier = 1f;
    [Tooltip("Bossens storlek. Till skillnad fran vanliga gnomer slumpas den inte.")]
    public float sizeMultiplier = 1.6f;
    public float speedMultiplier = 1.5f;
}

public enum CombatMusicMode
{
    Off,        // aldrig stridsmusik
    AnyEnemy,   // sa fort det finns nagon levande fiende
    BossOnly    // bara under bossmoten
}

public enum BossOrder
{
    InOrder,    // Muscle -> Troll -> Nacken -> borja om
    Random      // slumpad varje gang
}
