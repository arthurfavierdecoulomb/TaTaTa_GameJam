using UnityEngine;
using TMPro; 
public class FlashlightUI : MonoBehaviour
{
    [Header("Références")]
    public FlashlightStamina flashlight;
    public TMP_Text staminaText; 

    void Update()
    {
        if (flashlight == null || staminaText == null) return;

        float percent = (flashlight.CurrentStamina / flashlight.MaxStamina) * 100f;
        staminaText.text = Mathf.CeilToInt(percent) + "";

        // Couleur selon le niveau
        if (percent > 30f)
            staminaText.color = Color.white;
        else if (percent > 10f)
            staminaText.color = Color.yellow;
        else
            staminaText.color = Color.red;
    }
}