using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance { get; private set; }

    [Header("Références UI")]
    [SerializeField] GameObject deathPanel;
    [SerializeField] TextMeshProUGUI deathTitle;
    [SerializeField] TextMeshProUGUI deathCounter;
    [SerializeField] TextMeshProUGUI deathTimer; // Nouveau champ UI pour le timer

    [Header("Timing")]
    [SerializeField] float displayDuration = 1.8f;
    [SerializeField] float fadeDuration = 0.6f;

    int deathCount = 0;
    float sessionTimer = 0f;
    bool timerRunning = true;

    static readonly string[] rageTitles = new string[]
    {
        "GG EZ",
        "T'es une légende... du skill négatif.",
        "Même ma grand-mère fait mieux.",
        "Touche pas à la manette.",
        "Tu l'as cherché.",
        "C'est pathétique.",
        "Réessaie dans 10 ans.",
        "Wow. Incroyable. Vraiment.",
        "Le sol t'a dominé.",
        "Tu joues avec tes pieds ?",
        "Même le tutoriel pleure pour toi.",
        "Ici repose ton skill : 0",
        "Bravo. Tu as trouvé comment perdre.",
        "Le jeu te regarde avec pitié.",
        "Retourne jouer à Candy Crush.",
        "Pitié...",
        "Tu es la raison pour laquelle les jeux ont des modes faciles.",
        "Pourquoi tu joues encore ?",
        "Gros naze...",
        "Tu as fait pleurer les développeurs.",
        "N'oublie pas, appuie sur P pour mettre en pause... ou pour pleurer.",
        "Tu pleures ? C'est normal.",
        "Tu confonds ta gauche et ta droite visiblement.",
        "C'est les gens comme toi qui mettent des mauvaises reviews sur Steam...",
        "T'appelles ça esquiver ?",
        "L'obstacle t'a même pas regardé.",
        "Le sol est ton meilleur ami apparemment.",
        "Tu sautes comme une brique.",
        "L'obstacle était statique. Statique.",
        "Il fallait sauter. T'as pas sauté.",
        "Ton instinct de survie est en vacances."
    };

    // Phrases avec le timer intégré — {0} = minutes, {1} = secondes
    static readonly string[] timerTaunts = new string[]
    {
        "tu as mis {0} min et {1} sec pour ça...",
        "{0} minutes et {1} secondes de souffrance. Bravo.",
        "mort en {0} min {1} sec. Historique.",
        "{0} min {1} sec... et toujours rien appris.",
        "t'as duré {0} minutes et {1} secondes. C'est tout.",
        "{1} secondes de plus et t'aurais battu ton record de nullité.",
        "ça fait {0} min {1} sec que tu fais semblant de jouer.",
        "{0} min {1} sec pour arriver là. Impressionnant (non).",
        "RIP. Durée de vie : {0} min {1} sec.",
        "même moi j'espérais mieux après {0} min {1} sec..."
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (deathPanel != null) deathPanel.SetActive(false);
    }

    void Update()
    {
        if (timerRunning)
            sessionTimer += Time.deltaTime;
    }

    // Appelé depuis l'extérieur si tu veux stopper/reprendre le timer
    public void PauseTimer() => timerRunning = false;
    public void ResumeTimer() => timerRunning = true;
    public void ResetTimer() => sessionTimer = 0f;

    public void ShowDeathScreen(System.Action onComplete)
    {
        deathCount++;
        timerRunning = false; // On fige le timer à la mort
        StartCoroutine(DeathRoutine(onComplete));
    }

    IEnumerator DeathRoutine(System.Action onComplete)
    {
        // 50/50 : phrase rage seule OU phrase timer seule
        if (Random.value < 0.5f)
        {
            // Phrase rageur classique
            if (deathTitle != null)
                deathTitle.text = rageTitles[Random.Range(0, rageTitles.Length)];
        }
        else
        {
            // Phrase avec timer
            int totalSeconds = Mathf.FloorToInt(sessionTimer);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            string timerPhrase = string.Format(timerTaunts[Random.Range(0, timerTaunts.Length)], minutes, seconds);

            if (deathTitle != null)
                deathTitle.text = timerPhrase;
        }

        // Compteur de morts
        if (deathCounter != null)
            deathCounter.text = "tu es mort " + deathCount + " fois...";

        SetPanelAlpha(1f);
        if (deathPanel != null) deathPanel.SetActive(true);

        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeOut());

        if (deathPanel != null) deathPanel.SetActive(false);

        timerRunning = true;
        onComplete?.Invoke();
    }



    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetPanelAlpha(Mathf.Lerp(1f, 0f, elapsed / fadeDuration));
            yield return null;
        }
        SetPanelAlpha(0f);
    }

    void SetPanelAlpha(float alpha)
    {
        if (deathPanel == null) return;
        CanvasGroup cg = deathPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = alpha;
        }
        else
        {
            Image img = deathPanel.GetComponent<Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = alpha;
                img.color = c;
            }
        }
    }
}