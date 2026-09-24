using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class UppercaseUI
{
    public static bool Enabled = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene();
    }

    public static void ApplyToScene()
    {
        if (!Enabled) return;

        TMP_Text[] labels = Object.FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (TMP_Text label in labels)
        {
            Apply(label);
        }
    }

    public static void Apply(TMP_Text label)
    {
        if (!Enabled || label == null) return;
        if ((label.fontStyle & FontStyles.UpperCase) != 0) return;

        label.fontStyle |= FontStyles.UpperCase;
    }
}
