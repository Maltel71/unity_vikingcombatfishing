using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    public Image fillImage;
    private PlayerScript player;

    void Start()
    {
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
        }

        player = FindObjectOfType<PlayerScript>();
    }

    void Update()
    {
        if (player != null && fillImage != null)
        {
            fillImage.fillAmount = (float)player.playerHealth / (float)player.maxHealth;
        }
    }
}