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

        // Set up volume slider
        if (masterVolumeSlider != null)
        {
            // Load saved volume or use default
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
            masterVolumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);

            // Add listener for slider changes
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("Start button clicked - Loading game scene");

        // Load your main game scene
        // Make sure to add your game scene to Build Settings (File > Build Settings)
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked");

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
        if (audioMixer != null)
        {
            // Convert linear slider value (0-1) to decibels (-80 to 0)
            // Audio mixers use logarithmic scale (decibels)
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;

            audioMixer.SetFloat(masterVolumeParameter, dB);
        }
        else
        {
            // Fallback to AudioListener if no mixer is assigned
            AudioListener.volume = volume;
        }

        // Save the volume setting
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
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