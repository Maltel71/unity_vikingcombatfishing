using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Rutan dar man skriver in sitt namn nar man tagit sig in pa topplistan.
/// Bygger sitt UI i kod och skapas av DeathScreen. Skriver med Input.inputString
/// istallet for TMP_InputField sa den fungerar utan nagot uppsatt i editorn.
/// </summary>
public class NameEntryScreen : MonoBehaviour
{
    public static bool IsActive { get; private set; }

    System.Action<string> onConfirm;
    TextMeshProUGUI nameLabel;
    string typedName = "";
    float caretTimer = 0f;
    bool caretVisible = true;
    bool confirmed = false;

    /// <summary>Skapar och visar rutan. onConfirm far det inskrivna namnet.</summary>
    public static NameEntryScreen Show(int score, int placement, System.Action<string> onConfirm)
    {
        GameObject go = new GameObject("NameEntryScreen");
        NameEntryScreen screen = go.AddComponent<NameEntryScreen>();
        screen.onConfirm = onConfirm;
        screen.Build(score, placement);
        IsActive = true;
        return screen;
    }

    void Build(int score, int placement)
    {
        UiKit.EnsureEventSystem();
        TMP_FontAsset font = UiKit.FindGameFont();

        GameObject canvas = UiKit.CreateCanvas("NameEntryCanvas", transform, 600);

        Image dim = UiKit.CreateImage("Dim", canvas.transform, UiKit.Dim);
        UiKit.Stretch(dim.rectTransform);

        Image panel = UiKit.CreatePanel(canvas.transform, 620f, 420f);

        TextMeshProUGUI title = UiKit.CreateText("Title", panel.transform, "NYTT REKORD", 58f, UiKit.Border, font);
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 10f;
        UiKit.AnchorTop(title.rectTransform, 0f, -52f, 560f, 80f);

        string placementText = placement > 0
            ? "Plats " + placement + " - " + score + " poang"
            : score + " poang";
        TextMeshProUGUI sub = UiKit.CreateText("Subtitle", panel.transform, placementText, 30f, UiKit.TextColor, font);
        UiKit.AnchorTop(sub.rectTransform, 0f, -112f, 560f, 44f);

        TextMeshProUGUI prompt = UiKit.CreateText("Prompt", panel.transform, "Skriv ditt namn", 26f, UiKit.TextDim, font);
        UiKit.AnchorTop(prompt.rectTransform, 0f, -172f, 560f, 40f);

        // Inmatningsfaltet ar bara en ruta med text - vi laser tangenterna sjalva
        Image field = UiKit.CreateImage("Field", panel.transform, UiKit.Track);
        UiKit.AnchorTop(field.rectTransform, 0f, -226f, 480f, 66f);

        nameLabel = UiKit.CreateText("NameLabel", field.transform, "", 38f, UiKit.TextColor, font);
        UiKit.Stretch(nameLabel.rectTransform);

        Button doneBtn = UiKit.CreateButton("DoneButton", panel.transform, "Klar", font);
        UiKit.AnchorTop(doneBtn.GetComponent<RectTransform>(), 0f, -312f, 400f, 62f);
        doneBtn.onClick.AddListener(Confirm);

        TextMeshProUGUI hint = UiKit.CreateText("Hint", panel.transform, "Enter for att spara", 22f, UiKit.TextDim, font);
        UiKit.AnchorTop(hint.rectTransform, 0f, -368f, 520f, 34f);

        UpdateLabel();
    }

    void Update()
    {
        if (confirmed) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (typedName.Length > 0)
                {
                    typedName = typedName.Substring(0, typedName.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r')
            {
                Confirm();
                return;
            }
            else if (!char.IsControl(c) && typedName.Length < Highscores.MaxNameLength)
            {
                typedName += c;
            }
        }

        // Blinkande markor. Ospalad tid sa den blinkar aven om spelet ar pausat.
        caretTimer += Time.unscaledDeltaTime;
        if (caretTimer >= 0.5f)
        {
            caretTimer = 0f;
            caretVisible = !caretVisible;
        }

        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (nameLabel == null) return;
        UiKit.SetText(nameLabel, typedName + (caretVisible ? "_" : " "));
    }

    void Confirm()
    {
        if (confirmed) return;
        confirmed = true;
        IsActive = false;

        string finalName = string.IsNullOrEmpty(typedName.Trim()) ? "Viking" : typedName.Trim();

        if (onConfirm != null)
        {
            onConfirm(finalName);
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        IsActive = false;
    }
}
