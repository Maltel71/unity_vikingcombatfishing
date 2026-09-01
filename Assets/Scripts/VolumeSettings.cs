using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Gemensam volymhantering sa att startmenyn och pausmenyn styr samma sak
/// och landar pa samma sparade varde.
/// </summary>
public static class VolumeSettings
{
    public const string PrefKey = "MasterVolume";
    public const float DefaultVolume = 0.75f;

    // Satts av StartMenuController om en AudioMixer ar inkopplad i inspektorn.
    // Ar den null faller vi tillbaka pa AudioListener.volume.
    public static AudioMixer Mixer;
    public static string MixerParameter = "MasterVolume";

    public static float Load()
    {
        return Mathf.Clamp01(PlayerPrefs.GetFloat(PrefKey, DefaultVolume));
    }

    public static void Apply(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (Mixer != null)
        {
            // Mixern jobbar i decibel (logaritmiskt), slidern i 0-1
            float dB = volume > 0.0001f ? 20f * Mathf.Log10(volume) : -80f;
            Mixer.SetFloat(MixerParameter, dB);
        }
        else
        {
            AudioListener.volume = volume;
        }
    }

    public static void Save(float volume)
    {
        PlayerPrefs.SetFloat(PrefKey, Mathf.Clamp01(volume));
        PlayerPrefs.Save();
    }

    public static void ApplyAndSave(float volume)
    {
        Apply(volume);
        Save(volume);
    }
}
