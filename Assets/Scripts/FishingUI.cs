using UnityEngine;
using TMPro;

public class FishingUI : MonoBehaviour
{
    [Header("UI Text Reference")]
    public TextMeshProUGUI promptText;

    [Header("English Messages - Customize these!")]
    public string enterZoneMessage = "Press E to fish";
    public string waitingMessage = "Waiting for a bite...";
    public string biteMessage = "BITE! Hold E!";
    public string reelingMessage = "Keep holding E! {0}%";

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color biteColor = Color.yellow;
    public Color reelingColor = Color.green;

    private FishingRod fishingRod;

    void Start()
    {
        fishingRod = GetComponent<FishingRod>();

        if (promptText != null)
        {
            promptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (promptText == null || fishingRod == null) return;

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
            float percent = (fishingRod.reelInProgress / fishingRod.reelInDuration) * 100f;
            promptText.text = string.Format(reelingMessage, percent.ToString("F0"));
            promptText.color = reelingColor;
        }
        else if (fishingRod.hasBite)
        {
            promptText.text = biteMessage;
            promptText.color = biteColor;
        }
        else if (fishingRod.isWaitingForBite)
        {
            promptText.text = waitingMessage;
            promptText.color = normalColor;
        }
        else
        {
            promptText.text = enterZoneMessage;
            promptText.color = normalColor;
        }
    }
}