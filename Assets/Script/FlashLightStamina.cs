using UnityEngine;
using UnityEngine.Rendering.Universal; // Pour Light2D

public class FlashlightStamina : MonoBehaviour
{
    // Pour récupérer l'état du jeu (pause ou non)
    [Header("Pause")]
    public UIController uiController;

    [Header("Spotlight")]
    public Light2D spotlight;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float drainPerSecond = 5f;
    private float currentStamina;

    [Header("Flicker (sous 10%)")]
    public float flickerMinInterval = 0.05f;
    public float flickerMaxInterval = 0.2f;
    public float weakIntensity = 0.3f;
    public float normalIntensity = 1f;

    private bool isDead = false;
    private bool isFlickering = false;
    private float flickerTimer = 0f;
    private float nextFlicker = 0f;
    private bool flickerState = true;

    // Propriété publique lue par FlashlightUI
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    void Start()
    {
        currentStamina = maxStamina;

        if (spotlight == null)
            spotlight = GetComponent<Light2D>();
    }

    void Update()
    {
        // Définit un bool basé sur l'état du jeu (pause ou non)
        bool curPause = uiController.Pause;
        // Vérifie que le jeu n'est pas en pause avant d'éxecuter le code
        if (!curPause) {
            if (isDead) return;

            // --- Drain ---
            currentStamina -= drainPerSecond * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

            float percent = currentStamina / maxStamina;

            // --- 0% : mort complète ---
            if (currentStamina <= 0f)
            {
                spotlight.enabled = false;
                isDead = true;
                isFlickering = false;
                return;
            }

            // --- Sous 10% : flicker ---
            if (percent <= 0.10f)
            {
                isFlickering = true;
                spotlight.intensity = weakIntensity;
                HandleFlicker();
            }
            else
            {
                isFlickering = false;
                spotlight.enabled = true;
                spotlight.intensity = normalIntensity;
            }
        }
    }

    void HandleFlicker()
    {
        flickerTimer += Time.deltaTime;

        if (flickerTimer >= nextFlicker)
        {
            flickerTimer = 0f;
            nextFlicker = Random.Range(flickerMinInterval, flickerMaxInterval);

            flickerState = !flickerState;
            spotlight.enabled = flickerState;
        }
    }

    // Appelé par Battery.cs
    public void RechargeStamina()
    {
        currentStamina = maxStamina;
        isDead = false;
        isFlickering = false;
        spotlight.enabled = true;
        spotlight.intensity = normalIntensity;
    }
}