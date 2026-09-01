using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Pausmeny som oppnas med Escape.
///
/// TVA LAGEN:
/// 1. Ligger menyn i scenen (referenserna nedan ar ifyllda) anvands den rakt av.
///    Sa far du riktiga GameObjects i Hierarchy som du kan flytta och styla om.
///    Skapa den med menyn: Tools > Ragnar > Skapa pausmeny i scenen
/// 2. Ar referenserna tomma bygger scriptet menyn i kod vid start, som reserv,
///    sa spelet aldrig star utan pausmeny.
///
/// Fiskguiden fylls alltid i vid runtime fran FishingRod.fishTypes, oavsett lage.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    // Andra scripts kan kolla detta for att sluta lyssna pa input medan spelet ar pausat
    public static bool IsPaused { get; private set; }

    [Header("Scener")]
    public string mainMenuSceneName = "MenuScene";

    [Header("Beteende")]
    public KeyCode toggleKey = KeyCode.Escape;

    [Header("UI-referenser")]
    [Tooltip("Canvasen som gommer/visar hela menyn. Ar den tom byggs menyn i kod vid start.")]
    public GameObject menuRoot;
    public GameObject mainPanel;
    public GameObject guidePanel;
    public Slider volumeSlider;
    public TextMeshProUGUI volumeValueLabel;
    [Tooltip("Tom container som fiskraderna laggs i. Rensas och fylls varje gang guiden oppnas.")]
    public RectTransform guideRows;

    [Header("Knappar")]
    public Button guideButton;
    public Button resumeButton;
    public Button quitButton;
    public Button backButton;

    // Scener dar pausmenyn ska skapas automatiskt om ingen finns
    private static readonly string[] AutoCreateInScenes = { "MainScene" };

    private TMP_FontAsset gameFont;

    // ---------------------------------------------------------------
    // Bootstrap - skapar menyn automatiskt om scenen saknar en
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

        // Ingen meny utlagd i scenen - bygg en i kod som reserv
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

    // ---------------------------------------------------------------
    // Bygge - anvands bade av reservlaget och av editorverktyget
    // ---------------------------------------------------------------

    /// <summary>
    /// Bygger hela menyn som riktiga GameObjects under `parent` och fyller i
    /// referenserna ovan. Anropas av Tools > Ragnar > Skapa pausmeny i scenen.
    /// </summary>
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

    // ---------------------------------------------------------------
    // Fiskguidens innehall - lases alltid fran spelets faktiska data
    // ---------------------------------------------------------------

    void BuildGuideRows()
    {
        if (guideRows == null) return;

        for (int i = guideRows.childCount - 1; i >= 0; i--)
        {
            // Destroy sker forst i slutet av framen - gom raden direkt
            // sa gamla och nya inte ritas ovanpa varandra en frame
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

        float totalWeight = 0f;
        int count = 0;
        foreach (FishType ft in rod.fishTypes)
        {
            if (ft == null || ft.fishPrefab == null) continue;
            totalWeight += Mathf.Max(0f, ft.weight);
            count++;
        }

        // Krymp raderna om listan vaxer sa den alltid far plats
        float rowHeight = count > 12 ? 400f / count : 34f;
        float y = -rowHeight * 0.5f - 4f;
        int index = 0;

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

            BuildRow(guideRows, "Row" + index, y, name, hp, score, chance, color, rowHeight > 30f ? 25f : 20f);
            y -= rowHeight;
            index++;
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
