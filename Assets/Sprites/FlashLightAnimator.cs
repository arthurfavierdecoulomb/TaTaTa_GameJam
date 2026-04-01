using UnityEngine;

public class FlashlightAnimator : MonoBehaviour
{
    [Header("Référence")]
    public FlashlightStamina flashlightStamina;

    [Header("Sprites")]
    public Sprite spriteOn;   // Frame allumée
    public Sprite spriteOff;  // Frame éteinte

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float percent = flashlightStamina.CurrentStamina / flashlightStamina.MaxStamina;
        bool lightIsOn = flashlightStamina.spotlight.enabled;

        // Lampe éteinte (stamina à 0) OU en flicker ET actuellement off
        if (percent <= 0f || !lightIsOn)
            sr.sprite = spriteOff;
        else
            sr.sprite = spriteOn;
    }
}