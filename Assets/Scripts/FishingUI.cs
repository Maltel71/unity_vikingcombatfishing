using UnityEngine;
using UnityEngine.UI;

public class FishingUI : MonoBehaviour
{
    [Header("UI Icon References")]
    public Image fishingIcon;
    public Image enterZoneIcon;
    public Image waitingIcon;
    public Image biteIcon;
    public Image reelingIcon;

    [Header("Animation Settings")]
    public float biteIconPulseSpeed = 2f;
    public float biteIconMinScale = 0.8f;
    public float biteIconMaxScale = 1.2f;
    public float reelingIconScale = 1.3f;

    private FishingRod fishingRod;
    private Vector3 biteIconOriginalScale;
    private Vector3 reelingIconOriginalScale;

    void Start()
    {
        fishingRod = GetComponent<FishingRod>();

        // Store original scales
        if (biteIcon != null)
        {
            biteIconOriginalScale = biteIcon.transform.localScale;
        }
        if (reelingIcon != null)
        {
            reelingIconOriginalScale = reelingIcon.transform.localScale;
        }

        // Hide all icons at start
        HideAllIcons();
    }

    void Update()
    {
        if (fishingRod == null) return;

        if (fishingRod.fishingZone != null && fishingRod.fishingZone.playerInZone)
        {
            if (fishingIcon != null)
            {
                fishingIcon.gameObject.SetActive(true);
            }
            UpdateIcons();
        }
        else
        {
            HideAllIcons();
        }
    }

    void UpdateIcons()
    {
        if (fishingRod.isReelingIn)
        {
            // Show reeling icon with fill amount and scale
            ShowOnlyIcon(reelingIcon);
            if (reelingIcon != null)
            {
                reelingIcon.fillAmount = fishingRod.reelInProgress / fishingRod.reelInDuration;
                reelingIcon.transform.localScale = reelingIconOriginalScale * reelingIconScale;
            }
        }
        else if (fishingRod.hasBite)
        {
            // Show pulsating bite icon
            ShowOnlyIcon(biteIcon);
            if (biteIcon != null)
            {
                float scale = Mathf.Lerp(biteIconMinScale, biteIconMaxScale,
                    (Mathf.Sin(Time.time * biteIconPulseSpeed) + 1f) / 2f);
                biteIcon.transform.localScale = biteIconOriginalScale * scale;
            }
        }
        else if (fishingRod.isWaitingForBite)
        {
            ShowOnlyIcon(waitingIcon);
        }
        else
        {
            ShowOnlyIcon(enterZoneIcon);
        }
    }

    void HideAllIcons()
    {
        if (fishingIcon != null) fishingIcon.gameObject.SetActive(false);
        if (enterZoneIcon != null) enterZoneIcon.gameObject.SetActive(false);
        if (waitingIcon != null) waitingIcon.gameObject.SetActive(false);
        if (biteIcon != null)
        {
            biteIcon.gameObject.SetActive(false);
            biteIcon.transform.localScale = biteIconOriginalScale;
        }
        if (reelingIcon != null)
        {
            reelingIcon.gameObject.SetActive(false);
            reelingIcon.transform.localScale = reelingIconOriginalScale;
        }
    }

    void ShowOnlyIcon(Image iconToShow)
    {
        if (enterZoneIcon != null) enterZoneIcon.gameObject.SetActive(enterZoneIcon == iconToShow);
        if (waitingIcon != null) waitingIcon.gameObject.SetActive(waitingIcon == iconToShow);
        if (biteIcon != null)
        {
            bool isActive = biteIcon == iconToShow;
            biteIcon.gameObject.SetActive(isActive);
            if (!isActive) biteIcon.transform.localScale = biteIconOriginalScale;
        }
        if (reelingIcon != null)
        {
            bool isActive = reelingIcon == iconToShow;
            reelingIcon.gameObject.SetActive(isActive);
            if (!isActive)
            {
                reelingIcon.fillAmount = 0f;
                reelingIcon.transform.localScale = reelingIconOriginalScale;
            }
        }
    }
}