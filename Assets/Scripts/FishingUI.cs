using UnityEngine;
using TMPro;  // VIKTIGT: TextMeshPro namespace!

public class FishingUI : MonoBehaviour
{
    [Header("UI Text Reference")]
    public TextMeshProUGUI promptText;  // ÄNDRAT: TextMeshProUGUI istället för Text

    [Header("Messages - Anpassa dessa!")]
    public string enterZoneMessage = "Tryck E för att fiska";
    public string waitingMessage = "Väntar på napp...";
    public string biteMessage = "NAPP! Håll in E!";
    public string reelingMessage = "Håll kvar E! {0}%";

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color biteColor = Color.yellow;
    public Color reelingColor = Color.green;

    private FishingRod fishingRod;

    void Start()
    {
        // Hitta FishingRod på samma GameObject
        fishingRod = GetComponent<FishingRod>();

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("FishingUI: Ingen TextMeshProUGUI satt! Dra in din TextMeshPro text i Inspector.");
        }
    }

    void Update()
    {
        if (promptText == null || fishingRod == null) return;

        // Visa bara text om vi är i fiskezonen
        if (fishingRod.fishingZone != null && fishingRod.fishingZone.playerInZone)
        {
            promptText.gameObject.SetActive(true);
            UpdatePromptMessage();
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void UpdatePromptMessage()
    {
        if (fishingRod.isReelingIn)
        {
            // Visar progress under uppdragning
            float percent = (fishingRod.reelInProgress / fishingRod.reelInDuration) * 100f;
            promptText.text = string.Format(reelingMessage, percent.ToString("F0"));
            promptText.color = reelingColor;
        }
        else if (fishingRod.hasBite)
        {
            // Det har nappat!
            promptText.text = biteMessage;
            promptText.color = biteColor;
        }
        else if (fishingRod.isWaitingForBite)
        {
            // Väntar på napp
            promptText.text = waitingMessage;
            promptText.color = normalColor;
        }
        else
        {
            // Står bara i zonen
            promptText.text = enterZoneMessage;
            promptText.color = normalColor;
        }
    }
}