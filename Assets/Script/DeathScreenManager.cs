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
        "Vous êtes une légende... du skill négatif.",
        "Même ma grand-mère fait mieux.",
        "Touchez pas à la manette.",
        "Vous l'avez cherché.",
        "C'est pathétique.",
        "Réessayez dans 10 ans.",
        "Wow. Incroyable. Vraiment.",
        "Le sol vous a dominé.",
        "Vous jouez avec vos pieds ?",
        "Même le tutoriel pleure pour vous.",
        "Ici repose votre skill : 0",
        "Bravo. Vous avez trouvé comment perdre.",
        "Le jeu vous regarde avec pitié.",
        "Retournez jouer à Candy Crush.",
        "Pitié...",
        "Vous êtes la raison pour laquelle les jeux ont des modes faciles.",
        "Pourquoi vous jouez encore ?",
        "Gros naze...",
        "Vous faites pleurer les développeurs.",
        "N'oubliez pas, appuiez sur P pour mettre en pause... ou pour pleurer.",
        "Vous pleurez ? C'est normal.",
        "Tu confonds ta gauche et ta droite visiblement.",
        "C'est les gens comme vous qui mettent des mauvaises reviews sur Steam...",
        "Vous appellez ça esquiver ?",
        "Le sol est votre meilleur ami apparemment.",
        "L'obstacle était statique. Statique.",
        "Il fallait sauter. vous avez pas sauté.",
        "Votre instinct de survie est en vacances."
    };

    // Phrases avec le timer intégré — {0} = minutes, {1} = secondes
    static readonly string[] timerTaunts = new string[]
    {
        "vous avez mis {0} min et {1} sec pour ça...",
        "{0} minutes et {1} secondes de souffrance. Bravo.",
        "mort en {0} min {1} sec. Historique.",
        "{0} min {1} sec... et toujours rien appris.",
        "Vous avez {0} minutes et {1} secondes. C'est tout.",
        "{1} secondes de plus et t'aurais battu ton record de nullité.",
        "ça fait {0} min {1} sec que vous faites semblant de jouer.",
        "{0} min {1} sec pour arriver là. Impressionnant...",
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