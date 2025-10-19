using UnityEngine;
using UnityEngine.UI;

public class MonkeyMood : MonoBehaviour
{
    public Image trustIconImage;
    public Sprite highTrustSprite;
    public Sprite midTrustSprite;
    public Sprite lowTrustSprite;
    public HealthBar healthBar;
    public int maxHealth = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trustIconImage.sprite = midTrustSprite;
    }

    private void Update()
    {
        UpdateTrustIcon((float)healthBar.GetHealth());
    }

    // Update is called once per frame
    void UpdateTrustIcon(float trustMeter)
    {
        if (trustMeter <= maxHealth * 0.4f)
        {
            trustIconImage.sprite = lowTrustSprite;
        }
        else if (trustMeter >= maxHealth * 0.6f)
        {
            trustIconImage.sprite = highTrustSprite;
        }
        else
        {
            trustIconImage.sprite = midTrustSprite;
        }
    }
}
