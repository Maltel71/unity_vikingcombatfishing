using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Text;

/// <summary>
/// Pausmeny som oppnas med Escape. Bygger hela sitt UI i kod, sa den behover
/// inte laggas in manuellt i scenen - den skapar sig sjalv i de scener som
/// star i AutoCreateInScenes.
///
/// Innehaller aven fiskguiden, som lases direkt ur FishingRod.fishTypes vid
/// varje oppning. Andrar du HP eller poang pa en fiskprefab syns det direkt -
/// inget att uppdatera for hand.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // Andra scripts kan kolla detta for att sluta lyssna pa input medan spelet ar pausat
    public static bool IsPaused { get; private set; }

    [Header("Scener")]
    public string mainMenuSceneName = "MenuScene";

    [Header("Beteende")]
    public KeyCode toggleKey = KeyCode.Escape;

    // Scener dar pausmenyn ska skapas automatiskt
    private static readonly string[] AutoCreateInScenes = { "MainScene" };

    private GameObject root;
    private GameObject mainPanel;
    private GameObject guidePanel;
    private Transform guideRows;
    private Slider volumeSlider;
    private TextMeshProUGUI volumeValueLabel;
    private TMP_FontAsset gameFont;

    // ---------------------------------------------------------------
    // Bootstrap - skapar menyn automatiskt nar ratt scen laddas
    // ---------------------------------------------------------------

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
        // Efter en scenladdning ar spelet alltid igang igen
        Time.timeScale = 1f;
        IsPaused = false;

        bool wanted = false;
        foreach (string sceneName in AutoCreateInScenes)
        {
            if (scene.name == sceneName) { wanted = true; break; }
        }
        if (!wanted) return;

        if (FindFirstObjectByType<PauseMenu>() != null) return;

        GameObject go = new GameObject("PauseMenu");
        go.AddComponent<PauseMenu>();
    }

    // ---------------------------------------------------------------

    void Start()
    {
        gameFont = UiKit.FindGameFont();
        BuildUI();
        SetVisible(false);

        VolumeSettings.Apply(VolumeSettings.Load());
    }

    void Update()
    {
        // Namninmatningen efter doden ager tangentbordet
        if (NameEntryScreen.IsActive) return;

        if (Input.GetKeyDown(toggleKey))
        {
            if (IsPaused)
            {
                // Star man i guiden tar Escape en tillbaka till menyn forst
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

    // ---------------------------------------------------------------
    // Kommandon
    // ---------------------------------------------------------------

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

    void SetVisible(bool visible)
    {
        if (root != null) root.SetActive(visible);
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

    // ---------------------------------------------------------------
    // UI-bygge
    // ---------------------------------------------------------------

    void BuildUI()
    {
        UiKit.EnsureEventSystem();

        root = UiKit.CreateCanvas("PauseMenuCanvas", transform, 500);

        Image dim = UiKit.CreateImage("Dim", root.transform, UiKit.Dim);
        UiKit.Stretch(dim.rectTransform);

        BuildMainPanel();
        BuildGuidePanel();
        ShowGuide(false);
    }

    void BuildMainPanel()
    {
        Image panel = UiKit.CreatePanel(root.transform, 552f, 540f);
        mainPanel = panel.gameObject;

        TextMeshProUGUI title = UiKit.CreateText("Title", panel.transform, "PAUS", 64f, UiKit.Border, gameFont);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 12f;
        UiKit.AnchorTop(title.rectTransform, 0f, -46f, 480f, 80f);

        TextMeshProUGUI volLabel = UiKit.CreateText("VolumeLabel", panel.transform, "Volym", 32f, UiKit.TextColor, gameFont);
        volLabel.alignment = TextAlignmentOptions.Left;
        UiKit.AnchorTop(volLabel.rectTransform, -110f, -150f, 240f, 44f);

        volumeValueLabel = UiKit.CreateText("VolumeValue", panel.transform, "75%", 32f, UiKit.Border, gameFont);
        volumeValueLabel.alignment = TextAlignmentOptions.Right;
        UiKit.AnchorTop(volumeValueLabel.rectTransform, 150f, -150f, 160f, 44f);

        volumeSlider = UiKit.CreateSlider("VolumeSlider", panel.transform);
        UiKit.AnchorTop(volumeSlider.GetComponent<RectTransform>(), 0f, -205f, 440f, 34f);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        Button guideBtn = UiKit.CreateButton("GuideButton", panel.transform, "Fiskguide", gameFont);
        UiKit.AnchorTop(guideBtn.GetComponent<RectTransform>(), 0f, -288f, 400f, 64f);
        guideBtn.onClick.AddListener(delegate { ShowGuide(true); });

        Button resumeBtn = UiKit.CreateButton("ResumeButton", panel.transform, "Fortsatt", gameFont);
        UiKit.AnchorTop(resumeBtn.GetComponent<RectTransform>(), 0f, -366f, 400f, 64f);
        resumeBtn.onClick.AddListener(Resume);

        Button quitBtn = UiKit.CreateButton("QuitButton", panel.transform, "Till huvudmenyn", gameFont);
        UiKit.AnchorTop(quitBtn.GetComponent<RectTransform>(), 0f, -444f, 400f, 64f);
        quitBtn.onClick.AddListener(QuitToMainMenu);

        TextMeshProUGUI hint = UiKit.CreateText("Hint", panel.transform, "Esc for att stanga", 22f, UiKit.TextDim, gameFont);
        UiKit.AnchorTop(hint.rectTransform, 0f, -508f, 500f, 34f);
    }

    void BuildGuidePanel()
    {
        Image panel = UiKit.CreatePanel(root.transform, 760f, 640f);
        guidePanel = panel.gameObject;

        TextMeshProUGUI title = UiKit.CreateText("Title", panel.transform, "FISKGUIDE", 52f, UiKit.Border, gameFont);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 10f;
        UiKit.AnchorTop(title.rectTransform, 0f, -46f, 700f, 70f);

        // Kolumnrubriker
        BuildRow(panel.transform, "Header", -104f, "Fangst", "HP", "Poang", "Chans", UiKit.Border, 26f);

        Image line = UiKit.CreateImage("HeaderLine", panel.transform, UiKit.Border);
        UiKit.AnchorTop(line.rectTransform, 0f, -124f, 680f, 2f);

        GameObject rows = new GameObject("Rows", typeof(RectTransform));
        rows.transform.SetParent(panel.transform, false);
        UiKit.AnchorTop(rows.GetComponent<RectTransform>(), 0f, -136f, 700f, 1f);
        guideRows = rows.transform;

        Button backBtn = UiKit.CreateButton("BackButton", panel.transform, "Tillbaka", gameFont);
        UiKit.AnchorTop(backBtn.GetComponent<RectTransform>(), 0f, -578f, 400f, 62f);
        backBtn.onClick.AddListener(delegate { ShowGuide(false); });
    }

    /// <summary>Bygger om listan fran spelets faktiska fiskdata.</summary>
    void BuildGuideRows()
    {
        if (guideRows == null) return;

        for (int i = guideRows.childCount - 1; i >= 0; i--)
        {
            // Destroy sker forst i slutet av framen - gom raderna direkt
            // sa gamla och nya inte ritas ovanpa varandra en frame
            GameObject old = guideRows.GetChild(i).gameObject;
            old.SetActive(false);
            Destroy(old);
        }

        FishingRod rod = FindFirstObjectByType<FishingRod>();
        if (rod == null || rod.fishTypes == null || rod.fishTypes.Length == 0)
        {
            TextMeshProUGUI empty = UiKit.CreateText("Empty", guideRows, "Ingen fisk konfigurerad", 26f, UiKit.TextDim, gameFont);
            UiKit.AnchorTop(empty.rectTransform, 0f, -40f, 680f, 40f);
            return;
        }

        // Total vikt for att kunna visa chansen i procent
        float totalWeight = 0f;
        foreach (FishType ft in rod.fishTypes)
        {
            if (ft == null || ft.fishPrefab == null) continue;
            totalWeight += Mathf.Max(0f, ft.weight);
        }

        int count = 0;
        foreach (FishType ft in rod.fishTypes)
        {
            if (ft == null || ft.fishPrefab == null) continue;
            count++;
        }

        // Krymp raderna om listan vaxer sa den alltid faar plats
        float rowHeight = count > 12 ? 400f / count : 34f;
        float y = -rowHeight * 0.5f - 4f;

        foreach (FishType ft in rod.fishTypes)
        {
            if (ft == null || ft.fishPrefab == null) continue;

            FlyingFish fish = ft.fishPrefab.GetComponent<FlyingFish>();

            string name = fish != null && !string.IsNullOrEmpty(fish.fishName) ? fish.fishName : ft.fishPrefab.name;
            string hp = fish != null ? FormatSigned(fish.healthValue) : "-";
            string score = fish != null ? FormatSigned(fish.scoreValue) : "-";
            string chance = totalWeight > 0f
                ? Mathf.RoundToInt(Mathf.Max(0f, ft.weight) / totalWeight * 100f) + "%"
                : "-";

            // Skrapfangst (0 eller minus i bada) far dovare farg
            bool junk = fish != null && fish.scoreValue <= 0 && fish.healthValue <= 0;
            Color color = junk ? UiKit.TextDim : UiKit.TextColor;

            BuildRow(guideRows, "Row" + count, y, name, hp, score, chance, color, rowHeight > 30f ? 25f : 20f);
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
