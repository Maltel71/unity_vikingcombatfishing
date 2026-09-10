using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{

    public static bool IsPaused { get; private set; }

    [Header("Scenes")]
    public string mainMenuSceneName = "MenuScene";

    [Header("Behaviour")]
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("UI References")]
    [Tooltip("Canvas that shows or hides the whole menu. Leave empty to build the menu in code.")]
    public GameObject menuRoot;
    public GameObject mainPanel;
    public GameObject guidePanel;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueLabel;
    [Tooltip("Empty container for the fish rows. Cleared and refilled each time the guide opens.")]
    public RectTransform guideRows;

    [Header("Buttons")]
    public Button guideButton;
    public Button resumeButton;
    public Button quitButton;
    public Button backButton;

    private static readonly string[] AutoCreateInScenes = { "MainScene" };

    private TMP_FontAsset gameFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        IsPaused = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        Time.timeScale = 1f;
        IsPaused = false;

        bool wanted = false;
        foreach (string sceneName in AutoCreateInScenes)
        {
            if (scene.name == sceneName) { wanted = true; break; }
        }
        if (!wanted) return;

        PauseMenu existing = FindFirstObjectByType<PauseMenu>(FindObjectsInactive.Include);

        if (existing != null)
        {
            if (!existing.gameObject.activeSelf)
            {

                existing.gameObject.SetActive(true);
                Debug.LogWarning("The PauseMenu object in the scene was disabled and has been switched on. " +
                                 "Disable PauseMenuCanvas instead if you want to hide the menu in the editor.");
            }
            return;
        }

        GameObject go = new GameObject("PauseMenu");
        go.AddComponent<PauseMenu>();
    }

    void Start()
    {
        gameFont = UiKit.FindGameFont();

        if (menuRoot == null)
        {
            BuildInto(transform);
        }

        HookUpButtons();
        SetVisible(false);

        VolumeSettings.Apply(VolumeSettings.Load());
    }

    void Update()
    {

        if (NameEntryScreen.IsActive) return;

        if (Input.GetKeyDown(toggleKey))
        {
            if (IsPaused)
            {

                if (guidePanel != null && guidePanel.activeSelf) ShowGuide(false);
                else Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    void OnDestroy()
    {
        if (IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f;
        }
    }

    void HookUpButtons()
    {
        if (guideButton != null)
        {
            guideButton.onClick.RemoveListener(OpenGuide);
            guideButton.onClick.AddListener(OpenGuide);
        }
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(CloseGuide);
            backButton.onClick.AddListener(CloseGuide);
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(Resume);
            resumeButton.onClick.AddListener(Resume);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitToMainMenu);
            quitButton.onClick.AddListener(QuitToMainMenu);
        }
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        SetVisible(true);
        ShowGuide(false);
        RefreshVolumeUI();
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SetVisible(false);
    }

    public void QuitToMainMenu()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void OpenGuide() { ShowGuide(true); }
    public void CloseGuide() { ShowGuide(false); }

    void SetVisible(bool visible)
    {
        if (menuRoot != null) menuRoot.SetActive(visible);
    }

    void ShowGuide(bool show)
    {
        if (mainPanel != null) mainPanel.SetActive(!show);
        if (guidePanel != null) guidePanel.SetActive(show);
        if (show) BuildGuideRows();
    }

    void RefreshVolumeUI()
    {
        if (volumeSlider == null) return;
        float v = VolumeSettings.Load();
        volumeSlider.SetValueWithoutNotify(v);
        UpdateVolumeLabel(v);
    }

    void OnVolumeChanged(float value)
    {
        VolumeSettings.ApplyAndSave(value);
        UpdateVolumeLabel(value);
    }

    void UpdateVolumeLabel(float value)
    {
        UiKit.SetText(volumeValueLabel, Mathf.RoundToInt(value * 100f) + "%");
    }

    public void BuildInto(Transform parent)
    {
        if (gameFont == null) gameFont = UiKit.FindGameFont();

        UiKit.EnsureEventSystem();

        menuRoot = UiKit.CreateCanvas("PauseMenuCanvas", parent, 500);

        Image dim = UiKit.CreateImage("Dim", menuRoot.transform, UiKit.Dim);
        UiKit.Stretch(dim.rectTransform);

        BuildMainPanel();
        BuildGuidePanel();

        mainPanel.SetActive(true);
        guidePanel.SetActive(false);
    }

    void BuildMainPanel()
    {
        RectTransform content;
        mainPanel = UiKit.CreatePanel(menuRoot.transform, "MainPanel", 552f, 540f, out content);

        TextMeshProUGUI title = UiKit.CreateText("Title", content, "PAUSE", 64f, UiKit.Border, gameFont);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 12f;
        UiKit.AnchorTop(title.rectTransform, 0f, -46f, 480f, 80f);

        TextMeshProUGUI volLabel = UiKit.CreateText("VolumeLabel", content, "Volume", 32f, UiKit.TextColor, gameFont);
        volLabel.alignment = TextAlignmentOptions.Left;
        UiKit.AnchorTop(volLabel.rectTransform, -110f, -150f, 240f, 44f);

        volumeValueLabel = UiKit.CreateText("VolumeValue", content, "75%", 32f, UiKit.Border, gameFont);
        volumeValueLabel.alignment = TextAlignmentOptions.Right;
        UiKit.AnchorTop(volumeValueLabel.rectTransform, 150f, -150f, 160f, 44f);

        volumeSlider = UiKit.CreateSlider("VolumeSlider", content);
        UiKit.AnchorTop(volumeSlider.GetComponent<RectTransform>(), 0f, -205f, 440f, 34f);

        guideButton = UiKit.CreateButton("GuideButton", content, "Fish Guide", gameFont);
        UiKit.AnchorTop(guideButton.GetComponent<RectTransform>(), 0f, -288f, 400f, 64f);

        resumeButton = UiKit.CreateButton("ResumeButton", content, "Resume", gameFont);
        UiKit.AnchorTop(resumeButton.GetComponent<RectTransform>(), 0f, -366f, 400f, 64f);

        quitButton = UiKit.CreateButton("QuitButton", content, "Quit", gameFont);
        UiKit.AnchorTop(quitButton.GetComponent<RectTransform>(), 0f, -444f, 400f, 64f);

        TextMeshProUGUI hint = UiKit.CreateText("Hint", content, "Esc to close", 22f, UiKit.TextDim, gameFont);
        UiKit.AnchorTop(hint.rectTransform, 0f, -508f, 500f, 34f);
    }

    void BuildGuidePanel()
    {
        RectTransform content;
        guidePanel = UiKit.CreatePanel(menuRoot.transform, "GuidePanel", 760f, 640f, out content);

        TextMeshProUGUI title = UiKit.CreateText("Title", content, "Fish Guide", 52f, UiKit.Border, gameFont);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 10f;
        UiKit.AnchorTop(title.rectTransform, 0f, -46f, 700f, 70f);

        BuildRow(content, "Header", -104f, "Catch", "HP", "Points", "Chance", UiKit.Border, 26f);

        Image line = UiKit.CreateImage("HeaderLine", content, UiKit.Border);
        UiKit.AnchorTop(line.rectTransform, 0f, -124f, 680f, 2f);

        GameObject rows = new GameObject("Rows", typeof(RectTransform));
        rows.transform.SetParent(content, false);
        guideRows = rows.GetComponent<RectTransform>();
        UiKit.AnchorTop(guideRows, 0f, -136f, 700f, 1f);

        backButton = UiKit.CreateButton("BackButton", content, "Back", gameFont);
        UiKit.AnchorTop(backButton.GetComponent<RectTransform>(), 0f, -578f, 400f, 62f);
    }

    void BuildGuideRows()
    {
        if (guideRows == null) return;

        for (int i = guideRows.childCount - 1; i >= 0; i--)
        {

            GameObject old = guideRows.GetChild(i).gameObject;
            old.SetActive(false);
            Destroy(old);
        }

        FishingRod rod = FindFirstObjectByType<FishingRod>();
        if (rod == null || rod.fishTypes == null || rod.fishTypes.Length == 0)
        {
            TextMeshProUGUI empty = UiKit.CreateText("Empty", guideRows, "No fish configured", 26f, UiKit.TextDim, gameFont);
            UiKit.AnchorTop(empty.rectTransform, 0f, -40f, 680f, 40f);
            return;
        }

        List<string> names = new List<string>();
        List<FlyingFish> infos = new List<FlyingFish>();
        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach (FishType ft in rod.fishTypes)
        {
            if (ft == null || ft.fishPrefab == null) continue;

            FlyingFish fish = ft.fishPrefab.GetComponent<FlyingFish>();
            string name = fish != null && !string.IsNullOrEmpty(fish.fishName)
                ? fish.fishName
                : ft.fishPrefab.name;

            float w = Mathf.Max(0f, ft.weight);
            totalWeight += w;

            int at = names.IndexOf(name);
            if (at >= 0)
            {
                weights[at] += w;
            }
            else
            {
                names.Add(name);
                infos.Add(fish);
                weights.Add(w);
            }
        }

        int count = names.Count;

        float rowHeight = count > 12 ? 400f / count : 34f;
        float y = -rowHeight * 0.5f - 4f;

        for (int i = 0; i < count; i++)
        {
            FlyingFish fish = infos[i];

            string hp = fish != null ? FormatSigned(fish.healthValue) : "-";
            string score = fish != null ? FormatSigned(fish.scoreValue) : "-";
            string chance = totalWeight > 0f
                ? Mathf.RoundToInt(weights[i] / totalWeight * 100f) + "%"
                : "-";

            bool junk = fish != null && fish.scoreValue <= 0 && fish.healthValue <= 0;
            Color color = junk ? UiKit.TextDim : UiKit.TextColor;

            BuildRow(guideRows, "Row" + i, y, names[i], hp, score, chance, color,
                     rowHeight > 30f ? 25f : 20f);
            y -= rowHeight;
        }
    }

    static string FormatSigned(int value)
    {
        if (value > 0) return "+" + value;
        if (value < 0) return value.ToString();
        return "0";
    }

    void BuildRow(Transform parent, string name, float y, string c1, string c2, string c3, string c4, Color color, float size)
    {
        GameObject row = new GameObject(name, typeof(RectTransform));
        row.transform.SetParent(parent, false);
        UiKit.AnchorTop(row.GetComponent<RectTransform>(), 0f, y, 680f, 32f);

        MakeCell(row.transform, "Name", c1, -170f, 300f, TextAlignmentOptions.Left, color, size);
        MakeCell(row.transform, "Hp", c2, 60f, 120f, TextAlignmentOptions.Right, color, size);
        MakeCell(row.transform, "Score", c3, 190f, 120f, TextAlignmentOptions.Right, color, size);
        MakeCell(row.transform, "Chance", c4, 310f, 100f, TextAlignmentOptions.Right, color, size);
    }

    void MakeCell(Transform parent, string name, string content, float x, float width,
                  TextAlignmentOptions align, Color color, float size)
    {
        TextMeshProUGUI cell = UiKit.CreateText(name, parent, content, size, color, gameFont);
        cell.alignment = align;
        RectTransform rt = cell.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, 0f);
        rt.sizeDelta = new Vector2(width, 32f);
    }
}
