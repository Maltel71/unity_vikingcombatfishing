using UnityEngine;
using UnityEngine.UI;

public class RageBar : MonoBehaviour
{
    [Tooltip("The Image that fills up. Set Image Type to Filled, Horizontal, Origin Left.")]
    public Image fillImage;
    [Tooltip("Optional. Shown only while the berserk swing is available.")]
    public GameObject readyLabel;

    private RageMeter meter;

    void Start()
    {
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
        }

        meter = FindFirstObjectByType<RageMeter>(FindObjectsInactive.Include);

        if (readyLabel != null)
        {
            readyLabel.SetActive(false);
        }
    }

    void Update()
    {
        if (PlayerScript.IsGameOver)
        {
            gameObject.SetActive(false);
            return;
        }

        if (meter == null)
        {
            meter = FindFirstObjectByType<RageMeter>(FindObjectsInactive.Include);
            return;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = meter.Fill;
        }

        if (readyLabel != null && readyLabel.activeSelf != meter.IsFull)
        {
            readyLabel.SetActive(meter.IsFull);
        }
    }
}
