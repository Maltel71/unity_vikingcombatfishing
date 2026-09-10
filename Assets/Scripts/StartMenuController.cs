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
    [SerializeField] private string masterVolumeParameter = "MasterVolume";

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float defaultVolume = 0.75f;

    private void Start()
    {

        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }

        VolumeSettings.Mixer = audioMixer;
        VolumeSettings.MixerParameter = masterVolumeParameter;

        if (masterVolumeSlider != null)
        {

            float savedVolume = PlayerPrefs.GetFloat(VolumeSettings.PrefKey, defaultVolume);
            masterVolumeSlider.value = savedVolume;
            SetMasterVolume(savedVolume);

            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        else
        {

            VolumeSettings.Apply(VolumeSettings.Load());
        }
    }

    private void OnStartButtonClicked()
    {

        SceneManager.LoadScene(gameSceneName);
    }

    private void OnQuitButtonClicked()
    {
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#else

            Application.Quit();
#endif
    }

    private void SetMasterVolume(float volume)
    {
        VolumeSettings.ApplyAndSave(volume);
    }

    private void OnDestroy()
    {

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
