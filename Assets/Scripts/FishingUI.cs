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
    public float reelingIconMinScale = 1.0f;
    public float reelingIconMaxScale = 1.3f;

    [Header("Waiting Icon Animation")]
    public float waitingIconBobSpeed = 1f;
    public float waitingIconBobAmount = 10f;
    public float waitingIconRotateSpeed = 1f;
    public float waitingIconRotateAmount = 15f;

    [Header("Reeling Icon Animation")]
    public float reelingIconShakeSpeed = 20f;
    public float reelingIconShakeMin = -5f;
    public float reelingIconShakeMax = 5f;

    private FishingRod fishingRod;
    private Vector3 biteIconOriginalScale;
    private Vector3 reelingIconOriginalScale;
    private Vector3 waitingIconOriginalPosition;
    private Quaternion waitingIconOriginalRotation;
    private Vector3 reelingIconOriginalPosition;
    private float nextShakeTime = 0f;
    private Vector3 currentShakeOffset = Vector3.zero;

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
            reelingIconOriginalPosition = reelingIcon.transform.localPosition;
        }
        if (waitingIcon != null)
        {
            waitingIconOriginalPosition = waitingIcon.transform.localPosition;
            waitingIconOriginalRotation = waitingIcon.transform.localRotation;
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
            // Show reeling icon with fill amount and progressive scale
            ShowOnlyIcon(reelingIcon);
            if (reelingIcon != null)
            {
                float progress = fishingRod.reelInProgress / fishingRod.reelInDuration;
                reelingIcon.fillAmount = progress;

                // Scale up gradually from min to max as reeling progresses
                float currentScale = Mathf.Lerp(reelingIconMinScale, reelingIconMaxScale, progress);
                reelingIcon.transform.localScale = reelingIconOriginalScale * currentScale;

                // Shake effect with speed control
                if (Time.time >= nextShakeTime)
                {
                    float shakeX = Random.Range(reelingIconShakeMin, reelingIconShakeMax);
                    float shakeY = Random.Range(reelingIconShakeMin, reelingIconShakeMax);
                    currentShakeOffset = new Vector3(shakeX, shakeY, 0);
                    nextShakeTime = Time.time + (1f / reelingIconShakeSpeed);
                }
                reelingIcon.transform.localPosition = reelingIconOriginalPosition + currentShakeOffset;
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
            if (waitingIcon != null)
            {
                // Bob up and down
                float bobOffset = Mathf.Sin(Time.time * waitingIconBobSpeed) * waitingIconBobAmount;
                waitingIcon.transform.localPosition = waitingIconOriginalPosition + new Vector3(0, bobOffset, 0);

                // Rotate back and forth
                float rotationAngle = Mathf.Sin(Time.time * waitingIconRotateSpeed) * waitingIconRotateAmount;
                waitingIcon.transform.localRotation = waitingIconOriginalRotation * Quaternion.Euler(0, 0, rotationAngle);
            }
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
        if (waitingIcon != null)
        {
            waitingIcon.gameObject.SetActive(false);
            waitingIcon.transform.localPosition = waitingIconOriginalPosition;
            waitingIcon.transform.localRotation = waitingIconOriginalRotation;
        }
        if (biteIcon != null)
        {
            biteIcon.gameObject.SetActive(false);
            biteIcon.transform.localScale = biteIconOriginalScale;
        }
        if (reelingIcon != null)
        {
            reelingIcon.gameObject.SetActive(false);
            reelingIcon.transform.localScale = reelingIconOriginalScale;
            reelingIcon.transform.localPosition = reelingIconOriginalPosition;
        }
    }

    void ShowOnlyIcon(Image iconToShow)
    {
        if (enterZoneIcon != null) enterZoneIcon.gameObject.SetActive(enterZoneIcon == iconToShow);
        if (waitingIcon != null)
        {
            bool isActive = waitingIcon == iconToShow;
            waitingIcon.gameObject.SetActive(isActive);
            if (!isActive)
            {
                waitingIcon.transform.localPosition = waitingIconOriginalPosition;
                waitingIcon.transform.localRotation = waitingIconOriginalRotation;
            }
        }
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
                reelingIcon.transform.localPosition = reelingIconOriginalPosition;
            }
        }
    }
}