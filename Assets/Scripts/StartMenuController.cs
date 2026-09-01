using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class StartMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParameter = "MasterVolume"; // Name of the exposed parameter in your Audio Mixer

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene"; // Change this to your main game scene name
    [SerializeField] private float defaultVolume = 0.75f;

    private void Start()
    {
        // Set up button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        // Dela mixer-installningen med pausmenyn sa bada styr samma volym
        VolumeSettings.Mixer = audioMixer;
        VolumeSettings.MixerParameter = masterVolumeParameter;

        // Set up volume slider
        if (masterVolumeSlider != null)
        {
            // Load saved volume or use default
            float savedVolume = PlayerPrefs.GetFloat(VolumeSettings.PrefKey, defaultVolume);
            masterVolumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);

            // Add listener for slider changes
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        else
        {
            // Ingen slider i menyn - se anda till att sparad volym galler
            VolumeSettings.Apply(VolumeSettings.Load());
        }
    }

    private void OnStartButtonClicked()
    {
        // Load your main game scene
        // Make sure to add your game scene to Build Settings (File > Build Settings)
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        // If running in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // If running as a build
            Application.Quit();
#endif
    }

    private void SetMasterVolume(float volume)
    {
        VolumeSettings.ApplyAndSave(volume);
    }

    private void OnDestroy()
    {
        // Clean up listeners to prevent memory leaks
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        }
    }
}